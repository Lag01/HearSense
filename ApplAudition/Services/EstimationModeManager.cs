using Serilog;

namespace ApplAudition.Services;

/// <summary>
/// Gestionnaire simplifié d'estimation SPL.
/// Formule simple : SPL_est = dBFS + volume_système_dB + offset_dynamique
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

    public EstimationModeManager(
        ISystemVolumeService systemVolumeService,
        ILogger logger)
    {
        _systemVolumeService = systemVolumeService;
        _logger = logger;
    }

    /// <summary>
    /// Initialise le gestionnaire.
    /// </summary>
    public void Initialize()
    {
        _logger.Information("EstimationModeManager initialisé - offset dynamique 80-120 dB");
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
    /// Calcule l'offset dynamique selon le niveau dBFS.
    /// Interpole linéairement entre 4 zones : silence, faible, moyen, fort.
    /// </summary>
    private float CalculateDynamicOffset(float dbfs)
    {
        // Zone 1 : Silence (< -80 dBFS) → offset 80 dB
        if (dbfs < SILENCE_THRESHOLD)
        {
            return SILENCE_OFFSET;
        }

        // Zone 2 : Sons faibles (-80 à -40 dBFS) → interpolation linéaire 80 → 100 dB
        if (dbfs < LOW_SOUND_THRESHOLD)
        {
            float ratio = (dbfs - SILENCE_THRESHOLD) / (LOW_SOUND_THRESHOLD - SILENCE_THRESHOLD);
            return Lerp(SILENCE_OFFSET, LOW_OFFSET, ratio);
        }

        // Zone 3 : Sons moyens (-40 à -10 dBFS) → interpolation linéaire 100 → 110 dB
        if (dbfs < MEDIUM_SOUND_THRESHOLD)
        {
            float ratio = (dbfs - LOW_SOUND_THRESHOLD) / (MEDIUM_SOUND_THRESHOLD - LOW_SOUND_THRESHOLD);
            return Lerp(LOW_OFFSET, MEDIUM_OFFSET, ratio);
        }

        // Zone 4 : Sons forts (> -10 dBFS) → interpolation linéaire 110 → 120 dB
        // Clamp à 0 dBFS max (amplitude maximale avant saturation)
        if (dbfs > 0.0f)
        {
            dbfs = 0.0f;
        }

        float ratioHigh = (dbfs - MEDIUM_SOUND_THRESHOLD) / (0.0f - MEDIUM_SOUND_THRESHOLD);
        return Lerp(MEDIUM_OFFSET, HIGH_OFFSET, ratioHigh);
    }

    /// <summary>
    /// Interpolation linéaire entre a et b selon le ratio [0, 1].
    /// </summary>
    private float Lerp(float a, float b, float t)
    {
        // Clamp t à [0, 1]
        if (t < 0.0f) t = 0.0f;
        if (t > 1.0f) t = 1.0f;

        return a + (b - a) * t;
    }
}
