# Installer

ZenWin can be shipped as a standalone self-contained published folder immediately:

```powershell
dotnet publish ..\src\ZenWin.UI\ZenWin.UI.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

For MSIX, create a Windows Application Packaging Project in Visual Studio and point it at `ZenWin.UI`. The package identity, publisher certificate, update URL, and signing details are publisher-specific and intentionally kept out of source control.
