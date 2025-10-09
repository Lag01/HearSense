# Guide de build - Appli Audition

Ce document explique comment construire les différentes versions de l'application.

---

## Prérequis

- **.NET 8 SDK** installé ([Télécharger](https://dotnet.microsoft.com/download/dotnet/8.0))
- **PowerShell 5.1+** (inclus dans Windows 10/11)
- **Visual Studio 2022** (optionnel, pour build MSIX)

---

## Option 1 : Build portable avec script PowerShell (recommandé)

### Utilisation du script automatique

```powershell
.\Build-Portable.ps1
```

Le script va :
1. Nettoyer les builds précédents
2. Compiler en mode Release self-contained
3. Créer un fichier unique `ApplAudition.exe`
4. Créer l'archive `Build\ApplAudition_1.0.0_portable.zip`

### Paramètres optionnels

```powershell
# Build en mode Debug
.\Build-Portable.ps1 -Configuration Debug

# Build pour ARM64
.\Build-Portable.ps1 -RuntimeId win-arm64

# Build dans un autre dossier
.\Build-Portable.ps1 -OutputDir "C:\MyBuilds"
```

---

## Option 2 : Build portable manuel (ligne de commande)

Si le script PowerShell ne fonctionne pas, vous pouvez construire manuellement :

### 1. Ouvrir un terminal

```cmd
cd "C:\Users\lumin\Documents\Code\Appli Audition"
```

### 2. Publier l'application

```cmd
dotnet publish ApplAudition\ApplAudition.csproj ^
    --configuration Release ^
    --runtime win-x64 ^
    --self-contained true ^
    --output Build\Portable\ApplAudition ^
    /p:PublishSingleFile=true ^
    /p:IncludeNativeLibrariesForSelfExtract=true ^
    /p:EnableCompressionInSingleFile=true ^
    /p:DebugType=None ^
    /p:DebugSymbols=false
```

### 3. Copier le README

```cmd
copy README-Portable.txt Build\Portable\ApplAudition\README.txt
```

### 4. Créer l'archive .zip

```powershell
Compress-Archive -Path "Build\Portable\ApplAudition\*" -DestinationPath "Build\ApplAudition_1.0.0_portable.zip" -CompressionLevel Optimal
```

---

## Option 3 : Build MSIX (Installer Windows)

### Prérequis

- **Visual Studio 2022** avec workload "Windows Application Packaging"
- **Certificat de signature** (auto-signé pour dev, ou certificat valide pour prod)

### Étapes

#### 1. Créer le projet MSIX Packaging

1. Ouvrir `ApplAudition.sln` dans Visual Studio 2022
2. Clic droit sur la solution → "Add" → "New Project"
3. Choisir "Windows Application Packaging Project"
4. Nom : `ApplAudition.Package`
5. Version minimale : Windows 10, version 1809

#### 2. Référencer le projet principal

1. Dans `ApplAudition.Package`, clic droit sur "Applications"
2. "Add Reference..." → Cocher `ApplAudition`
3. Définir `ApplAudition.Package` comme projet de démarrage

#### 3. Configurer Package.appxmanifest

Ouvrir `Package.appxmanifest` et configurer :

```xml
<Identity
  Name="ApplAudition"
  Publisher="CN=VotreNom"
  Version="1.0.0.0" />

<Properties>
  <DisplayName>Appli Audition</DisplayName>
  <PublisherDisplayName>Votre Nom</PublisherDisplayName>
  <Logo>Images\StoreLogo.png</Logo>
  <Description>Estimation du niveau sonore au casque</Description>
</Properties>

<Dependencies>
  <TargetDeviceFamily Name="Windows.Desktop" MinVersion="10.0.17763.0" MaxVersionTested="10.0.22621.0" />
</Dependencies>

<Capabilities>
  <rescap:Capability Name="runFullTrust" />
</Capabilities>
```

#### 4. Créer un certificat auto-signé (développement)

```powershell
# Dans Visual Studio : Projet → Properties → Signing → "Create Test Certificate"
# OU en ligne de commande :
New-SelfSignedCertificate -Type Custom -Subject "CN=VotreNom" -KeyUsage DigitalSignature -FriendlyName "Appli Audition Dev" -CertStoreLocation "Cert:\CurrentUser\My" -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}")
```

#### 5. Builder le package MSIX

**Dans Visual Studio** :
1. Configuration : Release
2. Platform : x64
3. Clic droit sur `ApplAudition.Package` → "Publish" → "Create App Packages"
4. Choisir "Sideloading" (pas de Microsoft Store)
5. Suivre l'assistant

**En ligne de commande** :
```cmd
msbuild ApplAudition.Package\ApplAudition.Package.wapproj ^
  /p:Configuration=Release ^
  /p:Platform=x64 ^
  /p:AppxBundle=Always ^
  /p:UapAppxPackageBuildMode=SideloadOnly
```

#### 6. Résultat

Le package MSIX sera créé dans :
```
ApplAudition.Package\AppPackages\ApplAudition.Package_1.0.0.0_x64_Test\ApplAudition.Package_1.0.0.0_x64.msix
```

---

## Option 4 : Build dans Visual Studio (développement)

Pour tester rapidement :

1. Ouvrir `ApplAudition.sln` dans Visual Studio 2022
2. Configuration : **Release** (ou Debug pour dev)
3. Platform : **x64**
4. Build → Build Solution (Ctrl+Shift+B)
5. L'exécutable sera dans : `ApplAudition\bin\Release\net8.0-windows\`

**Note** : Cette version nécessite .NET 8 Runtime installé sur la machine cible.

---

## Vérification des builds

### Build portable (.zip)

1. Extraire `ApplAudition_1.0.0_portable.zip`
2. Lancer `ApplAudition.exe`
3. Vérifier :
   - ✅ Application démarre sans erreur
   - ✅ Pas de demande d'installation .NET
   - ✅ Taille ~80-150 MB (self-contained)

### Build MSIX (.msix)

1. Double-cliquer sur le .msix
2. Installer (peut demander d'installer le certificat la première fois)
3. Lancer depuis le menu Démarrer
4. Vérifier :
   - ✅ Application installée dans `%ProgramFiles%\WindowsApps\`
   - ✅ Icône visible dans le menu Démarrer
   - ✅ Désinstallation possible via "Ajouter/Supprimer des programmes"

---

## Dépannage

### Erreur : "dotnet command not found"

**Solution** : Installer .NET 8 SDK ([Télécharger](https://dotnet.microsoft.com/download/dotnet/8.0))

Vérifier l'installation :
```cmd
dotnet --version
```
Devrait afficher : `8.0.xxx`

---

### Erreur : "The project doesn't know how to run the profile"

**Cause** : Mauvaise configuration de la solution.

**Solution** :
1. Fermer Visual Studio
2. Supprimer les dossiers `bin` et `obj`
3. Rouvrir Visual Studio
4. Clean → Rebuild Solution

---

### Erreur MSIX : "Certificate not trusted"

**Cause** : Le certificat auto-signé n'est pas dans les certificats de confiance.

**Solution** :
1. Clic droit sur le .msix → "Properties" → "Digital Signatures"
2. Sélectionner le certificat → "Details" → "View Certificate"
3. "Install Certificate" → "Local Machine" → "Place in the following store" → "Trusted People"
4. Réinstaller le .msix

---

### Build portable trop volumineux (> 200 MB)

**Cause** : Inclusion de symboles de debug ou de fichiers inutiles.

**Solution** :
Vérifier les paramètres de publish :
```xml
<PropertyGroup>
  <DebugType>None</DebugType>
  <DebugSymbols>false</DebugSymbols>
  <PublishTrimmed>true</PublishTrimmed>
  <EnableCompressionInSingleFile>true</EnableCompressionInSingleFile>
</PropertyGroup>
```

---

## Optimisations avancées

### Réduire la taille du build portable

Ajouter au .csproj :

```xml
<PropertyGroup>
  <!-- Trimming (supprime le code inutilisé) -->
  <PublishTrimmed>true</PublishTrimmed>
  <TrimMode>link</TrimMode>

  <!-- Compression -->
  <EnableCompressionInSingleFile>true</EnableCompressionInSingleFile>

  <!-- Ready to Run (compilation AOT partielle) -->
  <PublishReadyToRun>false</PublishReadyToRun>
</PropertyGroup>
```

**Note** : Le trimming peut causer des problèmes avec la réflexion (MVVM, DI). Tester soigneusement.

---

### Build multi-plateforme (x64, ARM64)

```powershell
# Build x64
.\Build-Portable.ps1 -RuntimeId win-x64

# Build ARM64
.\Build-Portable.ps1 -RuntimeId win-arm64

# Build x86 (32-bit)
.\Build-Portable.ps1 -RuntimeId win-x86
```

---

## CI/CD (GitHub Actions)

Exemple de workflow pour automatiser les builds :

```yaml
name: Build Release

on:
  push:
    tags:
      - 'v*'

jobs:
  build:
    runs-on: windows-latest

    steps:
    - uses: actions/checkout@v3

    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x

    - name: Build Portable
      run: .\Build-Portable.ps1

    - name: Upload Artifact
      uses: actions/upload-artifact@v3
      with:
        name: ApplAudition-Portable
        path: Build/*.zip
```

---

## Support

Pour toute question ou problème de build :
- **Issues** : [GitHub Issues](https://github.com/votreRepo/ApplAudition/issues)
- **Discussions** : [GitHub Discussions](https://github.com/votreRepo/ApplAudition/discussions)

---

**Dernière mise à jour** : 2025-10-08
