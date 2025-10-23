namespace ApplAudition.Models;

/// <summary>
/// Modèle pour les paramètres de l'application (Phase 7 - Tâche 18).
/// Persisté dans %LOCALAPPDATA%\ApplAudition\settings.json.
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Indique si le Mode A est forcé (ignore les profils détectés).
    /// </summary>
    public bool ForceModeA { get; set; }

    /// <summary>
    /// Constante de calibration personnalisée (optionnelle, Phase 8).
    /// Si null, utilise la constante du profil heuristique.
    /// </summary>
    public float? CalibrationConstantC { get; set; }

    /// <summary>
    /// Indique si l'application doit démarrer automatiquement avec Windows.
    /// </summary>
    public bool StartWithWindows { get; set; }

    /// <summary>
    /// Indique si l'application doit démarrer minimisée dans le system tray.
    /// </summary>
    public bool StartMinimized { get; set; }

    /// <summary>
    /// Indique si la fermeture de la fenêtre minimise vers le tray au lieu de quitter.
    /// </summary>
    public bool MinimizeToTrayOnClose { get; set; } = true;

    /// <summary>
    /// Seuil critique de niveau sonore en dB(A) qui déclenche une notification.
    /// Recommandation OMS/française : 85 dB(A) maximum 8h/jour.
    /// </summary>
    public float CriticalThresholdDbA { get; set; } = 85.0f;

    /// <summary>
    /// Indique si les notifications de dépassement de seuil sont activées.
    /// </summary>
    public bool EnableNotifications { get; set; } = true;

    /// <summary>
    /// Durée minimale en minutes entre deux notifications (cooldown).
    /// Évite le spam si l'utilisateur reste au-dessus du seuil.
    /// </summary>
    public int NotificationCooldownMinutes { get; set; } = 5;

    /// <summary>
    /// Si true, ne notifie qu'une seule fois par session (jusqu'au redémarrage de l'application).
    /// </summary>
    public bool NotificationShowOncePerSession { get; set; } = false;
}
