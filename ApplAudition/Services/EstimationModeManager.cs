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

    // Seuils pour l'interpolation progressive de l'offset
    private const float SILENCE_THRESHOLD = -80.0f;      // En dessous = silence absolu (0 dB)
    private const float LOW_SOUND_THRESHOLD = -40.0f;    // Transition sons faibles → moyens
    private const float MEDIUM_SOUND_THRESHOLD = -10.0f; // Transition sons moyens → forts

    // Offsets pour chaque zone
    private const float SILENCE_OFFSET = 80.0f;          // Offset minimum (zone silence)
    private const float LOW_OFFSET = 100.0f;             // Offset pour sons faibles
    private const float MEDIUM_OFFSET = 110.0f;          // Offset pour sons moyens
    private const float HIGH_OFFSET = 120.0f;            // Offset pour sons forts

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
        _logger.Information("EstimationModeManager initialisé - Mode A avec offset dynamique (80-120 dB)");
    }

    /// <summary>
    /// Estime le niveau SPL avec offset dynamique adaptatif.
    /// Formule : SPL_est = dBFS + volume_système_dB + offset_dynamique(dBFS)
    /// L'offset varie de 80 à 120 dB selon le niveau du signal pour améliorer la précision.
    /// </summary>
    /// <param name="dbfs">Niveau en dBFS</param>
    /// <returns>SPL estimé en dB(A), clamped à 0 dB minimum</returns>
    public float EstimateSpl(float dbfs)
    {
        // Cas spécial : silence absolu (< -80 dBFS)
        if (dbfs < SILENCE_THRESHOLD)
        {
            return 0.0f; // Afficher 0 dB pour le silence complet
        }

        // Récupérer le volume système Windows (en dB)
        float volumeSystemDb = _systemVolumeService.GetVolumeDb();

        // Calculer l'offset dynamique selon le niveau du signal
        float dynamicOffset = CalculateDynamicOffset(dbfs);

        // Formule adaptative : dBFS + volume système + offset dynamique
        float splEstimated = dbfs + volumeSystemDb + dynamicOffset;

        // Clamping à 0 dB minimum pour éviter valeurs négatives à l'affichage
        if (splEstimated < 0.0f)
        {
            return 0.0f;
        }

        return splEstimated;
    }

    /// <summary>
    /// Calcule un offset dynamique basé sur le niveau dBFS du signal.
    /// Utilise une interpolation linéaire progressive pour des transitions lisses.
    /// </summary>
    /// <param name="dbfs">Niveau en dBFS</param>
    /// <returns>Offset à ajouter (80 à 120 dB)</returns>
    private float CalculateDynamicOffset(float dbfs)
    {
        // Zone 1 : Sons faibles (-80 à -40 dBFS)
        // Interpolation linéaire de 80 dB à 100 dB
        if (dbfs < LOW_SOUND_THRESHOLD)
        {
            float ratio = (dbfs - SILENCE_THRESHOLD) / (LOW_SOUND_THRESHOLD - SILENCE_THRESHOLD);
            ratio = Math.Clamp(ratio, 0.0f, 1.0f); // Sécurité
            return SILENCE_OFFSET + ratio * (LOW_OFFSET - SILENCE_OFFSET);
        }

        // Zone 2 : Sons moyens (-40 à -10 dBFS)
        // Interpolation linéaire de 100 dB à 110 dB
        if (dbfs < MEDIUM_SOUND_THRESHOLD)
        {
            float ratio = (dbfs - LOW_SOUND_THRESHOLD) / (MEDIUM_SOUND_THRESHOLD - LOW_SOUND_THRESHOLD);
            ratio = Math.Clamp(ratio, 0.0f, 1.0f);
            return LOW_OFFSET + ratio * (MEDIUM_OFFSET - LOW_OFFSET);
        }

        // Zone 3 : Sons forts (-10 à 0 dBFS)
        // Interpolation linéaire de 110 dB à 120 dB
        float ratioHigh = (dbfs - MEDIUM_SOUND_THRESHOLD) / (0.0f - MEDIUM_SOUND_THRESHOLD);
        ratioHigh = Math.Clamp(ratioHigh, 0.0f, 1.0f);
        return MEDIUM_OFFSET + ratioHigh * (HIGH_OFFSET - MEDIUM_OFFSET);
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
