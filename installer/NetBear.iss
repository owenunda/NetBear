#define AppVersion GetEnv("NETBEAR_VERSION")

[Setup]
AppId={{8F3C2A71-5B4E-4D2A-9C61-3E7B0D4A9F12}
AppName=NetBear
AppVersion={#AppVersion}
AppPublisher=owenunda
AppPublisherURL=https://github.com/owenunda/NetBear
DefaultDirName={localappdata}\Programs\NetBear
DisableProgramGroupPage=yes
DisableDirPage=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir=..\artifacts
OutputBaseFilename=NetBearSetup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\NetBear.exe
CloseApplications=yes

[Languages]
Name: "es"; MessagesFile: "compiler:Languages\Spanish.isl"

[Tasks]
Name: "autostart"; Description: "Iniciar NetBear con Windows"

[Files]
Source: "..\artifacts\publish\NetBear.exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{userprograms}\NetBear"; Filename: "{app}\NetBear.exe"

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "NetBear"; ValueData: """{app}\NetBear.exe"""; Tasks: autostart

[Run]
Filename: "{app}\NetBear.exe"; Description: "Abrir NetBear"; Flags: nowait postinstall skipifsilent

[UninstallRun]
Filename: "{sys}\taskkill.exe"; Parameters: "/f /im NetBear.exe"; Flags: runhidden; RunOnceId: "StopNetBear"

[Code]
// La app también puede escribir este valor desde su menú, así que se borra siempre al desinstalar.
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usPostUninstall then
    RegDeleteValue(HKCU, 'Software\Microsoft\Windows\CurrentVersion\Run', 'NetBear');
end;
