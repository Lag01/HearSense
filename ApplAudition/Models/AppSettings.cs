namespace ApplAudition.Models;

/// <summary>
/// Modèle pour les paramètres de l'application.
/// Persisté dans %LOCALAPPDATA%\ApplAudition\settings.json.
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Seuil d'avertissement (orange) en dB(A).
    /// Défaut : 70 dB(A) (conservateur, en dessous des recommandations OMS).
    /// </summary>
    public float ThresholdWarning { get; set; } = 70.0f;

    /// <summary>
    /// Seuil de danger (rouge) en dB(A).
    /// Défaut : 85 dB(A) (recommandation OMS : max 8h/jour).
    /// </summary>
    public float ThresholdDanger { get; set; } = 85.0f;

    /// <summary>
    /// Seuil critique (rouge très foncé) en dB(A).
    /// Défaut : 100 dB(A) (risque immédiat de dommage auditif sévère).
    /// </summary>
    public float ThresholdCritical { get; set; } = 100.0f;

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
    /// Par défaut, utilise ThresholdDanger.
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
