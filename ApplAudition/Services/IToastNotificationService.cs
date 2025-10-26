namespace ApplAudition.Services;

/// <summary>
/// Interface pour le service de notifications Toast Windows 10/11.
/// </summary>
public interface IToastNotificationService
{
    /// <summary>
    /// Affiche une notification Toast native Windows.
    /// </summary>
    /// <param name="title">Titre de la notification</param>
    /// <param name="message">Message de la notification</param>
    /// <param name="iconPath">Chemin absolu vers l'icône à afficher (optionnel)</param>
    void ShowToast(string title, string message, string? iconPath = null);
}
