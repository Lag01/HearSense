# Script de création du projet MSIX Packaging
# Ce script configure automatiquement le projet MSIX pour Appli Audition

param(
    [string]$SolutionPath = "..\ApplAudition.sln",
    [string]$PackageProjectName = "ApplAudition.Package",
    [string]$MainProjectName = "ApplAudition"
)

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  Création du projet MSIX Packaging      " -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Vérifier que Visual Studio est installé
$vsWhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
if (-not (Test-Path $vsWhere)) {
    Write-Host "Erreur : Visual Studio 2022 non trouvé !" -ForegroundColor Red
    Write-Host "Ce script nécessite Visual Studio 2022 avec le workload 'Windows Application Packaging'" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Télécharger : https://visualstudio.microsoft.com/downloads/" -ForegroundColor Cyan
    exit 1
}

$vsPath = & $vsWhere -latest -property installationPath
Write-Host "Visual Studio trouvé : $vsPath" -ForegroundColor Green

# Vérifier que le workload Windows Application Packaging est installé
$hasPackagingWorkload = & $vsWhere -latest -requires Microsoft.VisualStudio.Workload.ManagedDesktop -property productId
if (-not $hasPackagingWorkload) {
    Write-Host "Avertissement : Le workload 'Windows Application Packaging' n'est peut-être pas installé." -ForegroundColor Yellow
    Write-Host "Installez-le via Visual Studio Installer si le projet ne compile pas." -ForegroundColor Yellow
    Write-Host ""
}

Write-Host ""
Write-Host "IMPORTANT : Ce script nécessite d'être exécuté avec Visual Studio 2022 installé." -ForegroundColor Yellow
Write-Host "Pour créer le projet MSIX, suivez les instructions manuelles dans MSIX-Templates\README.md" -ForegroundColor Yellow
Write-Host ""
Write-Host "Étapes manuelles recommandées :" -ForegroundColor Cyan
Write-Host "  1. Ouvrir ApplAudition.sln dans Visual Studio 2022" -ForegroundColor White
Write-Host "  2. Clic droit sur la solution → Add → New Project" -ForegroundColor White
Write-Host "  3. Rechercher 'Windows Application Packaging Project'" -ForegroundColor White
Write-Host "  4. Nom : ApplAudition.Package" -ForegroundColor White
Write-Host "  5. Ajouter une référence au projet ApplAudition" -ForegroundColor White
Write-Host "  6. Remplacer Package.appxmanifest par le template fourni" -ForegroundColor White
Write-Host ""

# Créer un fichier d'instructions
$instructionsPath = "..\MSIX-SETUP-INSTRUCTIONS.txt"
$instructions = @"
=============================================
  INSTRUCTIONS : Création du projet MSIX
=============================================

Le projet MSIX Packaging permet de créer un installer Windows (.msix) pour Appli Audition.

PRÉREQUIS
---------
- Visual Studio 2022
- Workload "Windows Application Packaging"
- .NET 8 SDK

ÉTAPES (À FAIRE DANS VISUAL STUDIO 2022)
-----------------------------------------

1. Ouvrir ApplAudition.sln dans Visual Studio 2022

2. Ajouter un nouveau projet :
   - Clic droit sur la solution → "Add" → "New Project"
   - Rechercher "Windows Application Packaging Project"
   - Nom : ApplAudition.Package
   - Location : Dossier racine de la solution
   - Version minimale : Windows 10, version 1809 (Build 17763)
   - Cliquer "Create"

3. Référencer le projet principal :
   - Dans ApplAudition.Package, clic droit sur "Applications"
   - "Add Reference..."
   - Cocher "ApplAudition"
   - Cliquer "OK"

4. Configurer le manifeste :
   - Ouvrir Package.appxmanifest dans ApplAudition.Package
   - Remplacer le contenu par celui de MSIX-Templates\Package.appxmanifest
   - Adapter Publisher et PublisherDisplayName

5. Ajouter les assets visuels :
   - Créer le dossier ApplAudition.Package\Images\
   - Copier les images depuis MSIX-Templates\Images\ (à créer)
   - Tailles requises :
     * Square44x44Logo.png (44×44)
     * Square150x150Logo.png (150×150)
     * Wide310x150Logo.png (310×150)
     * StoreLogo.png (50×50)
     * SplashScreen.png (620×300)

6. Créer un certificat de test :
   - ApplAudition.Package → Properties → Signing
   - "Choose Certificate..." → "Create Test Certificate..."
   - Laisser le mot de passe vide (développement uniquement)

7. Builder le package :
   - Configuration : Release
   - Platform : x64
   - Clic droit sur ApplAudition.Package → "Publish" → "Create App Packages..."
   - Choisir "Sideloading"
   - Sélectionner x64
   - Cliquer "Create"

RÉSULTAT
--------
Le package MSIX sera créé dans :
ApplAudition.Package\AppPackages\ApplAudition.Package_1.0.0.0_x64_Test\

Fichiers générés :
- ApplAudition.Package_1.0.0.0_x64.msix (installer)
- ApplAudition.Package_1.0.0.0_x64.cer (certificat)
- Install.ps1 (script d'installation)

INSTALLATION
------------
1. Si certificat auto-signé : installer d'abord le .cer dans "Trusted People"
2. Double-cliquer sur le .msix
3. Cliquer "Installer"

DOCUMENTATION COMPLÈTE
----------------------
Voir MSIX-Templates\README.md pour plus de détails.

=============================================
Date : $(Get-Date -Format "yyyy-MM-dd HH:mm")
=============================================
"@

Set-Content -Path $instructionsPath -Value $instructions -Encoding UTF8
Write-Host "Instructions créées : $instructionsPath" -ForegroundColor Green
Write-Host ""

# Créer un template de .gitignore pour le projet MSIX
$gitignorePath = "..\ApplAudition.Package.gitignore"
$gitignore = @"
# MSIX Packaging - fichiers à ignorer

# Certificats de test (sensibles)
*.pfx
*.cer

# Packages générés
AppPackages/
BundleArtifacts/

# Fichiers temporaires Visual Studio
*.user
*.suo
*.cache
bin/
obj/

# Logs
*.log
"@

Set-Content -Path $gitignorePath -Value $gitignore -Encoding UTF8
Write-Host "Template .gitignore créé : $gitignorePath" -ForegroundColor Green
Write-Host "  → À renommer en .gitignore une fois le projet MSIX créé" -ForegroundColor Gray
Write-Host ""

Write-Host "==========================================" -ForegroundColor Green
Write-Host "  Configuration terminée !                " -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Prochaines étapes :" -ForegroundColor Cyan
Write-Host "  1. Lire MSIX-SETUP-INSTRUCTIONS.txt" -ForegroundColor White
Write-Host "  2. Ouvrir Visual Studio 2022" -ForegroundColor White
Write-Host "  3. Suivre les étapes manuelles" -ForegroundColor White
Write-Host ""
Write-Host "Documentation complète : MSIX-Templates\README.md" -ForegroundColor Cyan
