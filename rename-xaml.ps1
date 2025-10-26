# Script pour remplacer ApplAudition par HearSense dans les fichiers XAML

$rootPath = "C:\Users\lumin\Documents\Code\Appli Audition"

# Trouver tous les fichiers .xaml (hors obj/ et bin/)
Get-ChildItem -Path $rootPath -Include *.xaml -Recurse | Where-Object {
    $_.FullName -notmatch '\\obj\\' -and
    $_.FullName -notmatch '\\bin\\'
} | ForEach-Object {
    $file = $_
    $content = Get-Content $file.FullName -Raw -Encoding UTF8

    if ($content -match 'ApplAudition') {
        $newContent = $content -replace 'x:Class="ApplAudition', 'x:Class="HearSense'
        $newContent = $newContent -replace 'xmlns:converters="clr-namespace:ApplAudition', 'xmlns:converters="clr-namespace:HearSense'
        $newContent = $newContent -replace 'xmlns:local="clr-namespace:ApplAudition', 'xmlns:local="clr-namespace:HearSense'
        $newContent = $newContent -replace 'xmlns:controls="clr-namespace:ApplAudition', 'xmlns:controls="clr-namespace:HearSense'
        $newContent = $newContent -replace 'xmlns:viewmodels="clr-namespace:ApplAudition', 'xmlns:viewmodels="clr-namespace:HearSense'

        Set-Content -Path $file.FullName -Value $newContent -Encoding UTF8 -NoNewline
        Write-Host "Mis à jour: $($file.FullName)"
    }
}

Write-Host "`nTerminé!"
