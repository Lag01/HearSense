using ApplAudition.Models;

namespace ApplAudition.Services;

/// <summary>
/// Service de gestion des paramètres de l'application (Phase 7 - Tâche 18).
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Événement déclenché lorsque les paramètres sont modifiés.
    /// </summary>
    event EventHandler? SettingsChanged;

    /// <summary>
    /// Charge les paramètres depuis le fichier JSON.
    /// </summary>
    Task LoadAsync();

    /// <summary>
    /// Sauvegarde les paramètres dans le fichier JSON.
    /// </summary>
    Task SaveAsync();

    /// <summary>
    /// Obtient les paramètres actuels.
    /// </summary>
    AppSettings Settings { get; }

    /// <summary>
    /// Définit si le Mode A est forcé.
    /// </summary>
    Task SetForceModeAAsync(bool force);

    /// <summary>
    /// Définit la constante de calibration personnalisée.
    /// </summary>
    Task SetCalibrationConstantAsync(float? constantC);
}
