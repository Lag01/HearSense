; Script Inno Setup pour HearSense
; Version 1.6 - Installation et désinstallation automatiques
; Créé par Erwan GUEZINGAR

#define MyAppName "HearSense"
#define MyAppVersion "1.6"
#define MyAppPublisher "Erwan GUEZINGAR"
#define MyAppURL "https://github.com/yourusername/hearsense"
#define MyAppExeName "HearSense.exe"
#define DotNetVersion "8.0"
#define DotNetRuntimeURL "https://aka.ms/dotnet/8.0/windowsdesktop-runtime-win-x64.exe"

[Setup]
; Informations de base
AppId={{B8F5A3C2-7D4E-4B9A-A1F3-8C2D5E9F6A4B}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile=
InfoBeforeFile=AVERTISSEMENT-INSTALLATION.txt
InfoAfterFile=
; Icône de l'installeur (utilise l'icône de l'application)
SetupIconFile=HearSense\Resources\icon.ico
; Configuration de sortie
OutputDir=Build
OutputBaseFilename=HearSense_{#MyAppVersion}_Setup
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern

; Prérequis système
MinVersion=10.0.17763
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64

; Configuration de désinstallation
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName}

; Privilèges (require admin pour installer dans Program Files)
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=dialog

[Languages]
Name: "french"; MessagesFile: "compiler:Languages\French.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1; Check: not IsAdminInstallMode

[Files]
; Tous les fichiers de l'application (build self-contained)
Source: "Build\Release\HearSense\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: N'utilisez pas "Flags: ignoreversion" sur les fichiers système partagés

[Icons]
; Raccourci dans le menu Démarrer
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Désinstaller {#MyAppName}"; Filename: "{uninstallexe}"
; Raccourci sur le Bureau (optionnel)
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
; Proposer de lancer l'application après installation
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
// Fonction pour vérifier si .NET 8 Runtime est installé
function IsDotNetInstalled(): Boolean;
var
  ResultCode: Integer;
  DotNetCheckPath: String;
begin
  // Vérifie si dotnet.exe existe
  DotNetCheckPath := ExpandConstant('{autopf}\dotnet\dotnet.exe');

  if not FileExists(DotNetCheckPath) then
  begin
    DotNetCheckPath := ExpandConstant('{pf64}\dotnet\dotnet.exe');
  end;

  if FileExists(DotNetCheckPath) then
  begin
    // Vérifie la version avec "dotnet --list-runtimes"
    Exec(DotNetCheckPath, '--list-runtimes', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
    Result := True; // Simplifié : si dotnet existe, on considère qu'une version est installée
  end
  else
  begin
    Result := False;
  end;
end;

// Fonction appelée lors de l'initialisation de l'installation
function InitializeSetup(): Boolean;
var
  ErrorCode: Integer;
  DotNetInstallerPath: String;
  ResultCode: Integer;
begin
  Result := True; // Par défaut, continuer l'installation

  // Vérifier si .NET est installé
  if not IsDotNetInstalled() then
  begin
    if MsgBox('HearSense nécessite .NET {#DotNetVersion} Desktop Runtime.' + #13#10 + #13#10 +
              'Le runtime n''est pas installé sur votre système.' + #13#10 +
              'Voulez-vous le télécharger et l''installer maintenant ?' + #13#10 + #13#10 +
              '(Environ 50 MB à télécharger)',
              mbConfirmation, MB_YESNO) = IDYES then
    begin
      // Télécharger et installer le runtime
      DotNetInstallerPath := ExpandConstant('{tmp}\windowsdesktop-runtime-installer.exe');

      if not FileExists(DotNetInstallerPath) then
      begin
        // Télécharger le runtime
        if MsgBox('Le téléchargement va maintenant commencer.' + #13#10 +
                  'Cela peut prendre quelques minutes selon votre connexion.' + #13#10 + #13#10 +
                  'Continuer ?', mbInformation, MB_OKCANCEL) = IDOK then
        begin
          // Note: Inno Setup ne supporte pas le téléchargement natif,
          // on ouvre simplement le navigateur pour le téléchargement manuel
          ShellExec('open', '{#DotNetRuntimeURL}', '', '', SW_SHOWNORMAL, ewNoWait, ErrorCode);

          MsgBox('Veuillez :' + #13#10 +
                 '1. Télécharger et installer .NET {#DotNetVersion} Desktop Runtime' + #13#10 +
                 '2. Relancer cet installeur après l''installation de .NET' + #13#10 + #13#10 +
                 'L''installation va maintenant s''arrêter.',
                 mbInformation, MB_OK);
          Result := False; // Arrêter l'installation
        end
        else
        begin
          Result := False; // Annuler l'installation
        end;
      end;
    end
    else
    begin
      // L'utilisateur a refusé d'installer .NET
      if MsgBox('L''installation ne peut pas continuer sans .NET {#DotNetVersion} Desktop Runtime.' + #13#10 +
                'Voulez-vous vraiment annuler l''installation ?',
                mbConfirmation, MB_YESNO) = IDYES then
      begin
        Result := False; // Annuler l'installation
      end;
    end;
  end;
end;

// Message après installation réussie
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    // Log installation réussie
  end;
end;
