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
}
