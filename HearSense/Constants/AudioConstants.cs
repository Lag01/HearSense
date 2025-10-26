namespace HearSense.Constants;

/// <summary>
/// Constantes audio et DSP pour l'application HearSense.
/// Centralise tous les magic numbers liés au traitement audio.
/// </summary>
public static class AudioConstants
{
    // Configuration audio de base
    public const int SAMPLE_RATE = 48000; // Hz
    public const int BUFFER_DURATION_MS = 125; // Millisecondes
    public const int BUFFER_SIZE_SAMPLES = (SAMPLE_RATE * BUFFER_DURATION_MS) / 1000; // 6000 samples

    // Configuration historique
    public const int HISTORY_DURATION_SECONDS = 180; // 3 minutes
    public const int HISTORY_POINTS = (HISTORY_DURATION_SECONDS * 1000) / BUFFER_DURATION_MS; // 1440 points

    // Plancher dB (éviter -∞)
    public const float DB_FLOOR = -120.0f;

    // Lissage et throttling
    public const int SMOOTHING_WINDOW_SIZE = 4; // 4 × 125ms = 500ms de lissage
    public const int DISPLAY_THROTTLE_INTERVAL = 3; // Afficher tous les 3 buffers (375ms)
    public const int CHART_THROTTLE_INTERVAL = 4; // Graphe tous les 4 buffers (500ms)

    // Seuils d'estimation dynamique (dBFS)
    public const float SILENCE_THRESHOLD_DBFS = -80.0f;
    public const float LOW_SOUND_THRESHOLD_DBFS = -40.0f;
    public const float MEDIUM_SOUND_THRESHOLD_DBFS = -10.0f;

    // Offsets SPL (dB)
    public const float SILENCE_OFFSET_DB = 80.0f;
    public const float LOW_OFFSET_DB = 100.0f;
    public const float MEDIUM_OFFSET_DB = 110.0f;
    public const float HIGH_OFFSET_DB = 120.0f;

    // Seuils d'exposition (dB(A)) - Recommandations OMS
    public const float SAFE_THRESHOLD = 70.0f;
    public const float WARNING_THRESHOLD = 85.0f; // OMS : max 8h/jour
    public const float DANGER_THRESHOLD = 100.0f;
    public const float CRITICAL_THRESHOLD = 110.0f;
}
