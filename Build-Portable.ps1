# Script de build pour version portable Appli Audition
# Génère un .zip self-contained avec toutes les dépendances

param(
    [string]$Configuration = "Release",
    [string]$RuntimeId = "win-x64",
    [string]$OutputDir = ".\Build\Portable"
)

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  Appli Audition - Build Portable   " -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Nettoyage
Write-Host "[1/4] Nettoyage des builds précédents..." -ForegroundColor Yellow
if (Test-Path $OutputDir) {
    Remove-Item -Recurse -Force $OutputDir
}
New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

# Détection de dotnet
$dotnetPath = "C:\Program Files\dotnet\dotnet.exe"
if (-not (Test-Path $dotnetPath)) {
    $dotnetPath = (Get-Command dotnet -ErrorAction SilentlyContinue).Source
    if (-not $dotnetPath) {
        Write-Host "Erreur : dotnet.exe introuvable !" -ForegroundColor Red
        Write-Host "Installez .NET 8 SDK : https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Yellow
        exit 1
    }
}
Write-Host "dotnet trouvé : $dotnetPath" -ForegroundColor Gray

# Publish self-contained
Write-Host "[2/4] Build self-contained (cela peut prendre quelques minutes)..." -ForegroundColor Yellow
& $dotnetPath publish "ApplAudition\ApplAudition.csproj" `
    --configuration $Configuration `
    --runtime $RuntimeId `
    --self-contained true `
    --output "$OutputDir\ApplAudition" `
    /p:PublishSingleFile=true `
    /p:IncludeNativeLibrariesForSelfExtract=true `
    /p:EnableCompressionInSingleFile=true `
    /p:DebugType=None `
    /p:DebugSymbols=false

if ($LASTEXITCODE -ne 0) {
    Write-Host "Erreur lors du build !" -ForegroundColor Red
    exit 1
}

# Copier README portable
Write-Host "[3/4] Copie des fichiers supplémentaires..." -ForegroundColor Yellow
Copy-Item "README-Portable.txt" -Destination "$OutputDir\ApplAudition\README.txt" -ErrorAction SilentlyContinue

# Créer l'archive .zip
Write-Host "[4/4] Création de l'archive .zip..." -ForegroundColor Yellow
$version = "1.0.0"
$zipName = "ApplAudition_${version}_portable.zip"
$zipPath = ".\Build\$zipName"

if (Test-Path $zipPath) {
    Remove-Item -Force $zipPath
}

# Compression
Compress-Archive -Path "$OutputDir\ApplAudition\*" -DestinationPath $zipPath -CompressionLevel Optimal

# Informations finales
Write-Host ""
Write-Host "=====================================" -ForegroundColor Green
Write-Host "  Build terminé avec succès !       " -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host ""
Write-Host "Archive créée : $zipPath" -ForegroundColor Cyan

$zipSize = (Get-Item $zipPath).Length / 1MB
Write-Host "Taille        : $([math]::Round($zipSize, 2)) MB" -ForegroundColor Cyan
Write-Host ""
Write-Host "Contenu de l'archive :" -ForegroundColor Yellow
Get-ChildItem "$OutputDir\ApplAudition" | Select-Object Name, @{Name="Taille (MB)";Expression={[math]::Round($_.Length / 1MB, 2)}} | Format-Table -AutoSize
Write-Host ""
Write-Host "Pour tester : Extraire le .zip et lancer ApplAudition.exe" -ForegroundColor Green
