using System.Windows;

namespace ApplAudition.Services;

/// <summary>
/// Interface pour le contrôleur du system tray (zone de notification Windows).
/// Gère l'icône, le menu contextuel et les interactions utilisateur.
/// </summary>
public interface ITrayController : IDisposable
{
    /// <summary>
    /// Initialise le contrôleur tray avec la fenêtre principale.
    /// </summary>
    /// <param name="mainWindow">Fenêtre principale de l'application</param>
    void Initialize(Window mainWindow);

    /// <summary>
    /// Met à jour le tooltip de l'icône tray avec le niveau dB actuel.
    /// </summary>
    /// <param name="currentDbA">Niveau sonore actuel en dB(A)</param>
    /// <param name="category">Catégorie d'exposition (Safe/Moderate/Hazardous)</param>
    void UpdateTooltip(float currentDbA, Models.ExposureCategory category);

    /// <summary>
    /// Affiche une notification balloon tip.
    /// </summary>
    /// <param name="title">Titre de la notification</param>
    /// <param name="text">Texte de la notification</param>
    /// <param name="timeout">Durée d'affichage en millisecondes (défaut: 3000ms)</param>
    void ShowBalloonTip(string title, string text, int timeout = 3000);

    /// <summary>
    /// Affiche la fenêtre principale (restaure depuis tray).
    /// </summary>
    void ShowMainWindow();

    /// <summary>
    /// Masque la fenêtre principale (minimise vers tray).
    /// </summary>
    void HideMainWindow();

    /// <summary>
    /// Indique si l'icône tray est actuellement visible.
    /// </summary>
    bool IsVisible { get; }

    /// <summary>
    /// Définit le callback appelé lorsque l'utilisateur clique sur "Paramètres" dans le menu tray.
    /// </summary>
    /// <param name="callback">Action à exécuter pour ouvrir la fenêtre de paramètres</param>
    void SetSettingsCallback(Action callback);

    /// <summary>
    /// Affiche le popup tray avec la jauge dB miniature.
    /// </summary>
    void ShowPopup();

    /// <summary>
    /// Masque le popup tray s'il est affiché.
    /// </summary>
    void HidePopup();
}
