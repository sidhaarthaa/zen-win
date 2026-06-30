[CmdletBinding()]
param(
    [switch]$Uninstall,
    [string]$InstallDir = (Join-Path $env:LOCALAPPDATA "Programs\ZenWin")
)

$ErrorActionPreference = "Stop"
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

$architecture = [Runtime.InteropServices.RuntimeInformation]::OSArchitecture.ToString()
$rid = switch ($architecture) {
    "X64" { "win-x64" }
    "Arm64" { "win-arm64" }
    default { throw "ZenWin supports 64-bit Intel/AMD and ARM Windows. Detected: $architecture" }
}

$release = Invoke-RestMethod "https://api.github.com/repos/$repo/releases/latest"
$assetName = "ZenWin-$rid.zip"
$asset = $release.assets | Where-Object name -eq $assetName | Select-Object -First 1
if (-not $asset) {
    throw "The latest GitHub release does not contain $assetName."
}

$tempDir = Join-Path $env:TEMP "ZenWin-install-$PID"
$archive = Join-Path $tempDir $assetName

try {
    New-Item $tempDir -ItemType Directory -Force | Out-Null
    Invoke-WebRequest $asset.browser_download_url -OutFile $archive
    Get-Process "ZenWin.UI" -ErrorAction SilentlyContinue | Stop-Process -Force
    New-Item $InstallDir -ItemType Directory -Force | Out-Null
    Expand-Archive $archive -DestinationPath $InstallDir -Force

    $shell = New-Object -ComObject WScript.Shell
    foreach ($link in @($startMenuLink, $desktopLink)) {
        $shortcut = $shell.CreateShortcut($link)
        $shortcut.TargetPath = $appExe
        $shortcut.WorkingDirectory = $InstallDir
        $shortcut.Save()
    }

    New-Item $uninstallKey -Force | Out-Null
    Set-ItemProperty $uninstallKey DisplayName "ZenWin"
    Set-ItemProperty $uninstallKey DisplayVersion ([string]$release.tag_name).TrimStart("v")
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
