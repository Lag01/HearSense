namespace HearSense.Services;

/// <summary>
/// Interface pour la gestion du démarrage automatique de l'application avec Windows.
/// Gère la clé de registre HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run.
/// </summary>
public interface IStartupManager
{
    /// <summary>
    /// Ajoute l'application au démarrage automatique Windows.
    /// </summary>
    /// <param name="startMinimized">Si true, ajoute l'argument --minimized</param>
    /// <returns>True si l'opération a réussi, false sinon</returns>
    bool AddToStartup(bool startMinimized = true);

    /// <summary>
    /// Supprime l'application du démarrage automatique Windows.
    /// </summary>
    /// <returns>True si l'opération a réussi, false sinon</returns>
    bool RemoveFromStartup();

    /// <summary>
    /// Vérifie si l'application est configurée pour démarrer automatiquement.
    /// </summary>
    /// <returns>True si l'application est dans le démarrage automatique</returns>
    bool IsInStartup();

    /// <summary>
    /// Active ou désactive le démarrage automatique.
    /// </summary>
    /// <param name="enable">True pour activer, false pour désactiver</param>
    /// <param name="startMinimized">Si enable=true, démarre minimisé si true</param>
    /// <returns>True si l'opération a réussi, false sinon</returns>
    bool SetStartup(bool enable, bool startMinimized = true);
}
