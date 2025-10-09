using ApplAudition.Models;
using Serilog;

namespace ApplAudition.Services;

/// <summary>
/// Gestionnaire simplifié d'estimation SPL (Mode A uniquement).
/// Formule simple : SPL_est = dBFS + volume_système_dB + 120 dB
/// </summary>
public class EstimationModeManager : IEstimationModeManager
{
    private readonly ISystemVolumeService _systemVolumeService;
    private readonly ILogger _logger;

    private const float BASE_OFFSET = 120.0f; // Offset calibré pour casques grand public

    public EstimationMode CurrentMode => EstimationMode.ModeA; // Toujours Mode A
    public Profile? CurrentProfile => null; // Pas de profils
    public bool IsForcedModeA => false; // Pas de mode forcé
    public bool IsCalibrated => false; // Pas de calibration
    public float? CalibrationConstantC => null; // Pas de calibration

    public event EventHandler? ModeChanged;

    public EstimationModeManager(
        ISystemVolumeService systemVolumeService,
        ILogger logger)
    {
        _systemVolumeService = systemVolumeService;
        _logger = logger;
    }

    /// <summary>
    /// Initialise le gestionnaire (simplifié - rien à faire).
    /// </summary>
    public void Initialize()
    {
        _logger.Information("EstimationModeManager initialisé - Mode A uniquement, offset {Offset} dB", BASE_OFFSET);
    }

    /// <summary>
    /// Estime le niveau SPL selon la formule simplifiée.
    /// Formule : SPL_est = dBFS + volume_système_dB + 120 dB
    /// </summary>
    /// <param name="dbfs">Niveau en dBFS</param>
    /// <returns>SPL estimé en dB(A)</returns>
    public float EstimateSpl(float dbfs)
    {
        // Récupérer le volume système Windows (en dB)
        float volumeSystemDb = _systemVolumeService.GetVolumeDb();

        // Formule simple : dBFS + volume système + offset de base
        float splEstimated = dbfs + volumeSystemDb + BASE_OFFSET;

        return splEstimated;
    }

    /// <summary>
    /// Force le Mode A (non utilisé - toujours Mode A de toute façon).
    /// </summary>
    public void SetForceModeA(bool force)
    {
        // Rien à faire - toujours Mode A
        _logger.Debug("SetForceModeA appelé mais ignoré (Mode A permanent)");
    }

    /// <summary>
    /// Définit une constante de calibration (non supporté dans la version simplifiée).
    /// </summary>
    public async Task SetCalibrationConstantAsync(float? constantC)
    {
        _logger.Warning("SetCalibrationConstantAsync appelé mais non supporté dans la version simplifiée");
        await Task.CompletedTask;
    }

    /// <summary>
    /// Réinitialise la calibration (non supporté dans la version simplifiée).
    /// </summary>
    public async Task ResetCalibrationAsync()
    {
        _logger.Warning("ResetCalibrationAsync appelé mais non supporté dans la version simplifiée");
        await Task.CompletedTask;
    }
}
