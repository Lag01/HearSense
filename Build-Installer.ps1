# Script de build pour l'installeur HearSense
# Génère un .exe d'installation avec Inno Setup

param(
    [string]$Configuration = "Release",
    [string]$RuntimeId = "win-x64"
)

$ErrorActionPreference = "Stop"

Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "  HearSense - Build Installeur Windows      " -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""

# ============================================
# 1. Vérification des prérequis
# ============================================

Write-Host "[1/5] Vérification des prérequis..." -ForegroundColor Yellow

# Vérifier dotnet
$dotnetPath = "C:\Program Files\dotnet\dotnet.exe"
if (-not (Test-Path $dotnetPath)) {
    $dotnetPath = (Get-Command dotnet -ErrorAction SilentlyContinue).Source
    if (-not $dotnetPath) {
        Write-Host "Erreur : dotnet.exe introuvable !" -ForegroundColor Red
        Write-Host "Installez .NET 8 SDK : https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Yellow
        exit 1
    }
}
Write-Host "   dotnet trouvé : $dotnetPath" -ForegroundColor Gray

# Vérifier Inno Setup
$innoSetupPaths = @(
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
    "${env:ProgramFiles}\Inno Setup 6\ISCC.exe",
    "${env:ProgramFiles(x86)}\Inno Setup 5\ISCC.exe",
    "${env:ProgramFiles}\Inno Setup 5\ISCC.exe"
)

$isccPath = $null
foreach ($path in $innoSetupPaths) {
    if (Test-Path $path) {
        $isccPath = $path
        break
    }
}

if (-not $isccPath) {
    Write-Host ""
    Write-Host "Erreur : Inno Setup n'est pas installé !" -ForegroundColor Red
    Write-Host ""
    Write-Host "Pour créer l'installeur, vous devez installer Inno Setup :" -ForegroundColor Yellow
    Write-Host "1. Téléchargez-le depuis : https://jrsoftware.org/isdl.php" -ForegroundColor Cyan
    Write-Host "2. Installez la version 6.x (recommandé)" -ForegroundColor Cyan
    Write-Host "3. Relancez ce script" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Voulez-vous ouvrir la page de téléchargement maintenant ? (O/N)" -ForegroundColor Yellow
    $response = Read-Host
    if ($response -eq "O" -or $response -eq "o") {
        Start-Process "https://jrsoftware.org/isdl.php"
    }
    exit 1
}
Write-Host "   Inno Setup trouvé : $isccPath" -ForegroundColor Gray

# ============================================
# 2. Nettoyage
# ============================================

Write-Host ""
Write-Host "[2/5] Nettoyage des builds précédents..." -ForegroundColor Yellow

$buildDir = ".\Build\Release"
if (Test-Path $buildDir) {
    Remove-Item -Recurse -Force $buildDir
    Write-Host "   Dossier de build nettoyé" -ForegroundColor Gray
}
New-Item -ItemType Directory -Force -Path $buildDir | Out-Null

# ============================================
# 3. Build self-contained
# ============================================

Write-Host ""
Write-Host "[3/5] Build self-contained de HearSense..." -ForegroundColor Yellow
Write-Host "   (Cela peut prendre quelques minutes...)" -ForegroundColor Gray

$publishArgs = @(
    "publish"
    "HearSense\HearSense.csproj"
    "--configuration", $Configuration
    "--runtime", $RuntimeId
    "--self-contained", "true"
    "--output", "$buildDir\HearSense"
    "/p:PublishSingleFile=false"
    "/p:DebugType=None"
    "/p:DebugSymbols=false"
)

& $dotnetPath $publishArgs

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "Erreur lors du build de l'application !" -ForegroundColor Red
    exit 1
}

Write-Host "   Build terminé avec succès" -ForegroundColor Green

# Calculer la taille du build
$buildSize = (Get-ChildItem -Path "$buildDir\HearSense" -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB
Write-Host "   Taille du build : $([math]::Round($buildSize, 2)) MB" -ForegroundColor Gray

# ============================================
# 4. Nettoyage des fichiers inutiles
# ============================================

Write-Host ""
Write-Host "[4/5] Nettoyage des fichiers inutiles..." -ForegroundColor Yellow

# Supprimer les fichiers .pdb (debug symbols) s'ils existent
Get-ChildItem -Path "$buildDir\HearSense" -Filter "*.pdb" -Recurse | Remove-Item -Force -ErrorAction SilentlyContinue
Write-Host "   Fichiers de débogage supprimés" -ForegroundColor Gray

# ============================================
# 5. Compilation du script Inno Setup
# ============================================

Write-Host ""
Write-Host "[5/5] Compilation de l'installeur avec Inno Setup..." -ForegroundColor Yellow

$issFile = "HearSense-Installer.iss"
if (-not (Test-Path $issFile)) {
    Write-Host ""
    Write-Host "Erreur : Le fichier $issFile est introuvable !" -ForegroundColor Red
    exit 1
}

# Compiler le script Inno Setup
& $isccPath $issFile

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "Erreur lors de la compilation de l'installeur !" -ForegroundColor Red
    exit 1
}

# ============================================
# Informations finales
# ============================================

Write-Host ""
Write-Host "=============================================" -ForegroundColor Green
Write-Host "  Build terminé avec succès !                " -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green
Write-Host ""

# Trouver le fichier .exe généré
$installerFile = Get-ChildItem -Path ".\Build" -Filter "HearSense_*_Setup.exe" | Select-Object -First 1

if ($installerFile) {
    Write-Host "Installeur créé : $($installerFile.FullName)" -ForegroundColor Cyan
    $installerSize = $installerFile.Length / 1MB
    Write-Host "Taille          : $([math]::Round($installerSize, 2)) MB" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Pour distribuer l'application :" -ForegroundColor Yellow
    Write-Host "1. Envoyez le fichier $($installerFile.Name) à vos amis" -ForegroundColor White
    Write-Host "2. Ils double-cliquent dessus pour installer" -ForegroundColor White
    Write-Host "3. L'application apparaîtra dans leur menu Démarrer" -ForegroundColor White
    Write-Host "4. Désinstallation facile via Paramètres Windows > Applications" -ForegroundColor White
    Write-Host ""
} else {
    Write-Host "Avertissement : Le fichier installeur n'a pas été trouvé dans .\Build\" -ForegroundColor Yellow
}

Write-Host "Appuyez sur une touche pour quitter..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
