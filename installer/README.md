# Installation

The recommended installation path downloads the latest self-contained build from
GitHub Releases:

```powershell
irm https://raw.githubusercontent.com/sidhaarthaa/zen-win/master/installer/install.ps1 | iex
```

This is a per-user installation and does not require administrator access. The same
script supports removal:

```powershell
& "$env:LOCALAPPDATA\Programs\ZenWin\install.ps1" -Uninstall
```

## Publishing

ZenWin can be shipped as a standalone self-contained published folder immediately:

```powershell
dotnet publish ..\src\ZenWin.UI\ZenWin.UI.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

Push a version tag to publish both architectures:

```powershell
git tag v0.1.0
git push origin v0.1.0
```

The `build` workflow creates ZIP files and SHA-256 checksums on the corresponding
GitHub Release. It also creates native x64 and ARM64 Inno Setup executables with
silent install and uninstall support for package managers.
