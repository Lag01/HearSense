# Script pour renommer le dossier racine "Appli Audition" en "HearSense"
# ATTENTION : Ce script doit être exécuté en DEHORS du dossier à renommer

$oldPath = "C:\Users\lumin\Documents\Code\Appli Audition"
$newPath = "C:\Users\lumin\Documents\Code\HearSense"

if (Test-Path $oldPath) {
    try {
        # Changer le répertoire de travail vers un autre dossier
        Set-Location "C:\Users\lumin\Documents\Code"

        # Renommer le dossier
        Move-Item -Path $oldPath -Destination $newPath -Force
        Write-Host "✓ Dossier racine renommé avec succès de 'Appli Audition' en 'HearSense'"
        Write-Host "  Nouveau chemin : $newPath"
    }
    catch {
        Write-Host "✗ Erreur lors du renommage : $_"
        Write-Host "  Assurez-vous que le dossier n'est pas ouvert dans l'explorateur ou un terminal."
    }
}
else {
    Write-Host "✓ Le dossier a déjà été renommé en 'HearSense' ou n'existe plus"
}

Write-Host "`nAppuyez sur une touche pour continuer..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
