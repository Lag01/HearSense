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
}
