[Setup]
AppId={{8F4C4E2D-7F3D-4C87-9A5E-3D8B3B57C1A1}
AppName=Atlas
AppVersion=1.0
AppPublisher=Atlas Browser
AppPublisherURL=https://localhost/atlas
DefaultDirName={autopf}\Atlas
DefaultGroupName=Atlas
AllowNoIcons=yes
OutputDir=.\Installer
OutputBaseFilename=AtlasSetup
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "portuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[Tasks]
Name: "desktopicon"; Description: "Criar atalho na área de trabalho"; GroupDescription: "Atalhos:"; Flags: unchecked

[Files]
Source: "bin\Release\net8.0-windows\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Atlas"; Filename: "{app}\Atlas.exe"
Name: "{group}\Desinstalar Atlas"; Filename: "{uninstallexe}"
Name: "{autodesktop}\Atlas"; Filename: "{app}\Atlas.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\Atlas.exe"; Description: "Abrir o Atlas agora"; Flags: nowait postinstall skipifsilent
