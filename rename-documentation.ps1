# Script pour remplacer Appli Audition/ApplAudition par HearSense dans la documentation

$rootPath = "C:\Users\lumin\Documents\Code\Appli Audition"

# Trouver tous les fichiers .md et .txt
Get-ChildItem -Path $rootPath -Include *.md,*.txt -Recurse | Where-Object {
    $_.FullName -notmatch '\\obj\\' -and
    $_.FullName -notmatch '\\bin\\'
} | ForEach-Object {
    $file = $_
    $content = Get-Content $file.FullName -Raw -Encoding UTF8

    if ($content -match 'Appli Audition|ApplAudition|appli-audition|APPLI_AUDITION') {
        $newContent = $content -replace 'Appli Audition', 'HearSense'
        $newContent = $newContent -replace 'ApplAudition', 'HearSense'
        $newContent = $newContent -replace 'appli-audition', 'hearsense'
        $newContent = $newContent -replace 'APPLI_AUDITION', 'HEARSENSE'

        Set-Content -Path $file.FullName -Value $newContent -Encoding UTF8 -NoNewline
        Write-Host "Mis à jour: $($file.Name)"
    }
}

Write-Host "`nTerminé!"
