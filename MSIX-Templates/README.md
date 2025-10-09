# Templates MSIX pour Appli Audition

Ce dossier contient les templates et ressources nécessaires pour créer un package MSIX (installer Windows).

---

## Contenu

- `Package.appxmanifest` : Manifeste du package MSIX (template)
- `Images/` : Logos et assets visuels (à créer)
- `CreateMSIXProject.md` : Guide pas-à-pas pour créer le projet MSIX

---

## Prérequis

- **Visual Studio 2022** avec workload "Windows Application Packaging"
- **.NET 8 SDK** installé
- **Certificat de signature** (auto-signé pour dev, ou certificat valide pour production)

---

## Guide rapide

### 1. Créer le projet MSIX Packaging dans Visual Studio

1. Ouvrir `ApplAudition.sln` dans Visual Studio 2022
2. Clic droit sur la solution → "Add" → "New Project"
3. Rechercher "Windows Application Packaging Project"
4. Nom : `ApplAudition.Package`
5. Location : Dossier racine de la solution
6. Version minimale : Windows 10, version 1809 (Build 17763)
7. Cliquer "Create"

### 2. Référencer le projet principal

1. Dans le projet `ApplAudition.Package`, clic droit sur "Applications"
2. "Add Reference..."
3. Cocher `ApplAudition`
4. Cliquer "OK"

### 3. Configurer le manifeste

1. Dans `ApplAudition.Package`, ouvrir `Package.appxmanifest`
2. Remplacer le contenu par celui de `MSIX-Templates\Package.appxmanifest`
3. Adapter les valeurs :
   - `Publisher` : Votre nom ou organisation
   - `Version` : Version actuelle (ex: 1.0.0.0)
   - `PublisherDisplayName` : Nom d'affichage

### 4. Ajouter les assets visuels

Créer les images suivantes dans `ApplAudition.Package\Images\` :

| Fichier | Taille | Description |
|---------|--------|-------------|
| `Square44x44Logo.png` | 44×44 | Icône application (liste) |
| `Square150x150Logo.png` | 150×150 | Tuile moyenne menu Démarrer |
| `Wide310x150Logo.png` | 310×150 | Tuile large menu Démarrer |
| `StoreLogo.png` | 50×50 | Logo Microsoft Store (si publication) |
| `SplashScreen.png` | 620×300 | Écran de démarrage |

**Astuce** : Utiliser des outils comme [App Icon Generator](https://www.appicon.co/) pour générer toutes les tailles.

### 5. Créer un certificat de test

Dans Visual Studio :
1. Projet `ApplAudition.Package` → Properties → Signing
2. "Choose Certificate..." → "Create Test Certificate..."
3. Laisser le mot de passe vide (dev uniquement)
4. Cliquer "OK"

**Ou** en PowerShell :
```powershell
New-SelfSignedCertificate -Type Custom -Subject "CN=ApplAudition" -KeyUsage DigitalSignature -FriendlyName "Appli Audition Dev Certificate" -CertStoreLocation "Cert:\CurrentUser\My" -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}")
```

### 6. Builder le package

1. Configuration : **Release**
2. Platform : **x64**
3. Clic droit sur `ApplAudition.Package` → "Publish" → "Create App Packages..."
4. Choisir "Sideloading" (installation locale, pas Microsoft Store)
5. Cocher "Enable automatic updates" (optionnel)
6. Sélectionner architecture : x64 (ou x64 + ARM64 pour support complet)
7. Cliquer "Create"

### 7. Résultat

Le package MSIX sera créé dans :
```
ApplAudition.Package\AppPackages\ApplAudition.Package_1.0.0.0_x64_Test\
```

Fichiers générés :
- `ApplAudition.Package_1.0.0.0_x64.msix` : Package d'installation
- `ApplAudition.Package_1.0.0.0_x64.cer` : Certificat (à installer si auto-signé)
- `Install.ps1` : Script d'installation automatique

---

## Installation du package MSIX

### Sur la machine de développement

Double-cliquer sur `ApplAudition.Package_1.0.0.0_x64.msix`

Windows peut demander :
1. "Voulez-vous installer cette application ?" → Cliquer "Installer"
2. Si certificat non reconnu → Installer d'abord le .cer (voir ci-dessous)

### Sur une autre machine (certificat auto-signé)

1. **Installer le certificat** :
   - Clic droit sur `ApplAudition.Package_1.0.0.0_x64.cer`
   - "Install Certificate"
   - "Local Machine" → "Next"
   - "Place all certificates in the following store" → "Browse..."
   - Sélectionner "Trusted People" → "OK"
   - "Next" → "Finish"

2. **Installer l'application** :
   - Double-cliquer sur le .msix
   - Cliquer "Installer"

**Ou** utiliser le script PowerShell fourni :
```powershell
.\Install.ps1
```

---

## Certificat de production

Pour une distribution publique, utiliser un certificat de signature de code valide :

### Option 1 : Certificat commercial
- **DigiCert** (https://www.digicert.com/code-signing)
- **GlobalSign** (https://www.globalsign.com/en/code-signing-certificate)
- **Sectigo** (https://sectigo.com/ssl-certificates-tls/code-signing)

Prix : ~200-400€/an

### Option 2 : Microsoft Store
- Publier sur le Microsoft Store (certificat géré par Microsoft)
- Gratuit, mais processus de validation

### Configurer le certificat de production

1. Importer le certificat .pfx dans Visual Studio :
   - Projet → Properties → Signing
   - "Choose Certificate..." → "Select from file..."
   - Sélectionner le .pfx → Entrer le mot de passe

2. Rebuilder le package avec le certificat de production

---

## Paramètres avancés du manifeste

### Déclarations de capacités (Capabilities)

Le manifeste actuel utilise `runFullTrust` (application de bureau complète).

Pour ajouter d'autres capacités :
```xml
<Capabilities>
  <rescap:Capability Name="runFullTrust" />
  <!-- Exemple : accès aux bibliothèques musicales -->
  <uap:Capability Name="musicLibrary" />
  <!-- Exemple : accès réseau (si fonctionnalité future) -->
  <Capability Name="internetClient" />
</Capabilities>
```

### Extensions et protocoles

Pour enregistrer un protocole custom (ex: `appliaudition://`) :
```xml
<Extensions>
  <uap:Extension Category="windows.protocol">
    <uap:Protocol Name="appliaudition">
      <uap:DisplayName>Appli Audition Protocol</uap:DisplayName>
    </uap:Protocol>
  </uap:Extension>
</Extensions>
```

### Mise à jour automatique

Dans le fichier .wapproj :
```xml
<PropertyGroup>
  <AppxAutoIncrementPackageRevision>True</AppxAutoIncrementPackageRevision>
  <GenerateAppInstallerFile>True</GenerateAppInstallerFile>
  <AppInstallerUpdateFrequency>1</AppInstallerUpdateFrequency>
  <AppInstallerCheckForUpdateFrequency>OnApplicationRun</AppInstallerCheckForUpdateFrequency>
</PropertyGroup>
```

---

## Dépannage

### Erreur : "DEP0700: Registration of the app failed"

**Cause** : Conflit avec une installation précédente.

**Solution** :
1. Désinstaller l'application via "Ajouter/Supprimer des programmes"
2. Nettoyer le cache :
   ```powershell
   Get-AppxPackage *ApplAudition* | Remove-AppxPackage
   ```
3. Rebuilder et réinstaller

### Erreur : "The package signature is invalid"

**Cause** : Certificat expiré, corrompu, ou non installé.

**Solution** :
1. Vérifier la date d'expiration du certificat
2. Réinstaller le certificat dans "Trusted People"
3. Recréer le certificat si nécessaire

### Le package MSIX est trop volumineux (> 500 MB)

**Cause** : Fichiers de debug ou dépendances inutiles.

**Solution** :
1. Builder en mode **Release** (pas Debug)
2. Vérifier que `DebugType=None` dans le .csproj
3. Utiliser le trimming (voir BUILD.md)

---

## Structure du projet MSIX final

```
ApplAudition.Package/
├── Package.appxmanifest              # Manifeste du package
├── ApplAudition.Package.wapproj      # Fichier projet (généré par VS)
├── Images/                           # Assets visuels
│   ├── Square44x44Logo.png
│   ├── Square150x150Logo.png
│   ├── Wide310x150Logo.png
│   ├── StoreLogo.png
│   └── SplashScreen.png
├── ApplAudition.Package_TemporaryKey.pfx  # Certificat de test (ne pas commiter)
└── AppPackages/                      # Packages générés (ignoré par git)
    └── ApplAudition.Package_1.0.0.0_x64_Test/
        ├── ApplAudition.Package_1.0.0.0_x64.msix
        ├── ApplAudition.Package_1.0.0.0_x64.cer
        └── Install.ps1
```

---

## Ressources

- [Documentation MSIX Microsoft](https://docs.microsoft.com/en-us/windows/msix/)
- [Package.appxmanifest schema](https://docs.microsoft.com/en-us/uwp/schemas/appxpackage/appxmanifestschema/schema-root)
- [MSIX Packaging Tool](https://www.microsoft.com/en-us/p/msix-packaging-tool/9n5lw3jbcxkf)
- [App Icon Generator](https://www.appicon.co/)

---

**Dernière mise à jour** : 2025-10-08
