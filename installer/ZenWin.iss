#ifndef MyArch
  #define MyArch "x64"
#endif
#ifndef SourceDir
  #define SourceDir "..\artifacts\win-x64"
#endif
#ifndef OutputDir
  #define OutputDir "..\artifacts"
#endif

#if MyArch == "arm64"
  #define AllowedArchitectures "arm64"
#else
  #define AllowedArchitectures "x64compatible"
#endif

[Setup]
AppId={{E56C3498-EF78-43EF-AFF8-C232BFD49AA8}
AppName=ZenWin
AppVersion=0.2.0
AppPublisher=ZenWin
AppPublisherURL=https://github.com/sidhaarthaa/zen-win
AppSupportURL=https://github.com/sidhaarthaa/zen-win/issues
AppUpdatesURL=https://github.com/sidhaarthaa/zen-win/releases
DefaultDirName={localappdata}\Programs\ZenWin
DefaultGroupName=ZenWin
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
ArchitecturesAllowed={#AllowedArchitectures}
ArchitecturesInstallIn64BitMode={#AllowedArchitectures}
OutputDir={#OutputDir}
OutputBaseFilename=ZenWin-{#MyArch}-setup
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
CloseApplications=yes
UninstallDisplayIcon={app}\ZenWin.UI.exe
SetupLogging=yes

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\ZenWin"; Filename: "{app}\ZenWin.UI.exe"; WorkingDir: "{app}"
Name: "{autodesktop}\ZenWin"; Filename: "{app}\ZenWin.UI.exe"; WorkingDir: "{app}"

[Run]
Filename: "{app}\ZenWin.UI.exe"; Description: "Launch ZenWin"; Flags: nowait postinstall skipifsilent
