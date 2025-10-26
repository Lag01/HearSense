# Script pour renommer les fichiers de projet ApplAudition en HearSense

$rootPath = "C:\Users\lumin\Documents\Code\Appli Audition"

Write-Host "Mise à jour de la solution..."

# 1. Mettre à jour le contenu du fichier .sln
$slnPath = Join-Path $rootPath "ApplAudition.sln"
$slnContent = Get-Content $slnPath -Raw -Encoding UTF8
$slnContent = $slnContent -replace 'ApplAudition', 'HearSense'
Set-Content -Path $slnPath -Value $slnContent -Encoding UTF8 -NoNewline
Write-Host "Solution mise à jour"

# 2. Mettre à jour la référence de projet dans le fichier tests.csproj
$testsProjPath = Join-Path $rootPath "ApplAudition.Tests\ApplAudition.Tests.csproj"
$testsProjContent = Get-Content $testsProjPath -Raw -Encoding UTF8
$testsProjContent = $testsProjContent -replace '\.\.\\ApplAudition\\ApplAudition\.csproj', '..\HearSense\HearSense.csproj'
Set-Content -Path $testsProjPath -Value $testsProjContent -Encoding UTF8 -NoNewline
Write-Host "Référence de projet tests mise à jour"

# 3. Renommer les fichiers .csproj
$mainProjPath = Join-Path $rootPath "ApplAudition\ApplAudition.csproj"
$newMainProjPath = Join-Path $rootPath "ApplAudition\HearSense.csproj"
Move-Item -Path $mainProjPath -Destination $newMainProjPath -Force
Write-Host "ApplAudition.csproj renommé en HearSense.csproj"

$testsOldPath = Join-Path $rootPath "ApplAudition.Tests\ApplAudition.Tests.csproj"
$testsNewPath = Join-Path $rootPath "ApplAudition.Tests\HearSense.Tests.csproj"
Move-Item -Path $testsOldPath -Destination $testsNewPath -Force
Write-Host "ApplAudition.Tests.csproj renommé en HearSense.Tests.csproj"

# 4. Renommer le fichier .sln
$newSlnPath = Join-Path $rootPath "HearSense.sln"
Move-Item -Path $slnPath -Destination $newSlnPath -Force
Write-Host "ApplAudition.sln renommé en HearSense.sln"

# 5. Renommer les dossiers
$mainDirPath = Join-Path $rootPath "ApplAudition"
$newMainDirPath = Join-Path $rootPath "HearSense"
Move-Item -Path $mainDirPath -Destination $newMainDirPath -Force
Write-Host "Dossier ApplAudition renommé en HearSense"

$testsDirPath = Join-Path $rootPath "ApplAudition.Tests"
$newTestsDirPath = Join-Path $rootPath "HearSense.Tests"
Move-Item -Path $testsDirPath -Destination $newTestsDirPath -Force
Write-Host "Dossier ApplAudition.Tests renommé en HearSense.Tests"

Write-Host "`nTerminé! Tous les fichiers et dossiers ont été renommés."
