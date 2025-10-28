namespace HearSense.Services;

/// <summary>
/// Interface pour le service d'enregistrement de l'application dans Windows.
/// Permet d'enregistrer l'AppUserModelID dans le registre et de créer les raccourcis nécessaires
/// pour que les Toast Notifications affichent le bon nom et la bonne icône.
/// </summary>
public interface IWindowsRegistrationService
{
    /// <summary>
    /// Enregistre l'application dans le registre Windows pour les Toast Notifications.
    /// </summary>
    Task RegisterApplicationAsync();

    /// <summary>
    /// Vérifie si l'application est déjà enregistrée.
    /// </summary>
    bool IsRegistered();
}
