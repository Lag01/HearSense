# Script pour remplacer les namespaces ApplAudition par HearSense

$rootPath = "C:\Users\lumin\Documents\Code\Appli Audition"

# Trouver tous les fichiers .cs (hors obj/ et bin/)
Get-ChildItem -Path $rootPath -Include *.cs -Recurse | Where-Object {
    $_.FullName -notmatch '\\obj\\' -and
    $_.FullName -notmatch '\\bin\\' -and
    $_.FullName -notmatch '\.g\.cs$' -and
    $_.FullName -notmatch '\.g\.i\.cs$'
} | ForEach-Object {
    $file = $_
    $content = Get-Content $file.FullName -Raw -Encoding UTF8

    if ($content -match 'ApplAudition') {
        $newContent = $content -replace 'namespace ApplAudition', 'namespace HearSense'
        $newContent = $newContent -replace 'using ApplAudition', 'using HearSense'

        Set-Content -Path $file.FullName -Value $newContent -Encoding UTF8 -NoNewline
        Write-Host "Mis à jour: $($file.FullName)"
    }
}

Write-Host "`nTerminé!"
