[CmdletBinding()]
param(
    [switch]$Uninstall,
    [string]$InstallDir = (Join-Path $env:LOCALAPPDATA "Programs\ZenWin")
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"
$repo = "sidhaarthaa/zen-win"
$appExe = Join-Path $InstallDir "ZenWin.UI.exe"
$startMenuLink = Join-Path $env:APPDATA "Microsoft\Windows\Start Menu\Programs\ZenWin.lnk"
$desktopLink = Join-Path ([Environment]::GetFolderPath("Desktop")) "ZenWin.lnk"
$uninstallKey = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\ZenWin"

function Remove-ZenWin {
    Get-Process "ZenWin.UI" -ErrorAction SilentlyContinue | Stop-Process -Force
    Remove-Item $startMenuLink, $desktopLink -Force -ErrorAction SilentlyContinue
    Remove-Item $uninstallKey -Recurse -Force -ErrorAction SilentlyContinue

    if (Test-Path $InstallDir) {
        $cleanup = Join-Path $env:TEMP "ZenWin-uninstall-$PID.cmd"
        "@echo off`r`ntimeout /t 2 /nobreak >nul`r`nrmdir /s /q `"$InstallDir`"`r`ndel /q `"%~f0`"" |
            Set-Content $cleanup -Encoding Ascii
        Start-Process $cleanup -WindowStyle Hidden
    }

    Write-Host "ZenWin was uninstalled."
}

if ($Uninstall) {
    Remove-ZenWin
    return
}

$detectedArchitecture = [Runtime.InteropServices.RuntimeInformation]::OSArchitecture
if ($null -eq $detectedArchitecture) {
    throw "Windows architecture detection failed."
}
$architecture = $detectedArchitecture.ToString()
$rid = switch ($architecture) {
    "X64" { "win-x64" }
    "Arm64" { "win-arm64" }
    default { throw "ZenWin supports 64-bit Intel/AMD and ARM Windows. Detected: $architecture" }
}

$release = Invoke-RestMethod -UseBasicParsing "https://api.github.com/repos/$repo/releases/latest"
if (-not $release -or -not $release.tag_name -or -not $release.assets) {
    throw "GitHub returned an incomplete release response. Try again later."
}
$assetName = "ZenWin-$rid.zip"
$asset = $release.assets | Where-Object name -eq $assetName | Select-Object -First 1
if (-not $asset) {
    throw "The latest GitHub release does not contain $assetName."
}

$tempDir = Join-Path $env:TEMP "ZenWin-install-$PID"
$archive = Join-Path $tempDir $assetName

try {
    New-Item $tempDir -ItemType Directory -Force | Out-Null
    Invoke-WebRequest -UseBasicParsing $asset.browser_download_url -OutFile $archive
    Get-Process "ZenWin.UI" -ErrorAction SilentlyContinue | Stop-Process -Force
    New-Item $InstallDir -ItemType Directory -Force | Out-Null
    Expand-Archive $archive -DestinationPath $InstallDir -Force

    try {
        $shell = New-Object -ComObject WScript.Shell
        if ($null -eq $shell) {
            throw "Windows Script Host is unavailable."
        }
        foreach ($link in @($startMenuLink, $desktopLink)) {
            if ([string]::IsNullOrWhiteSpace($link)) {
                continue
            }
            $shortcut = $shell.CreateShortcut($link)
            if ($null -eq $shortcut) {
                throw "Could not create shortcut: $link"
            }
            $shortcut.TargetPath = $appExe
            $shortcut.WorkingDirectory = $InstallDir
            $shortcut.Save()
        }
    } catch {
        Write-Warning "ZenWin installed, but shortcuts could not be created: $($_.Exception.Message)"
    }

    New-Item $uninstallKey -Force | Out-Null
    Set-ItemProperty $uninstallKey DisplayName "ZenWin"
    $displayVersion = ([string]$release.tag_name).TrimStart("v")
    Set-ItemProperty $uninstallKey DisplayVersion $displayVersion
    Set-ItemProperty $uninstallKey Publisher "ZenWin"
    Set-ItemProperty $uninstallKey InstallLocation $InstallDir
    Set-ItemProperty $uninstallKey DisplayIcon $appExe
    $uninstallCommand = "powershell.exe -NoProfile -ExecutionPolicy Bypass -File `"$InstallDir\install.ps1`" -Uninstall"
    Set-ItemProperty $uninstallKey UninstallString $uninstallCommand
} finally {
    Remove-Item $tempDir -Recurse -Force -ErrorAction SilentlyContinue
}

Write-Host "ZenWin $($release.tag_name) installed in $InstallDir"
Start-Process $appExe
