namespace ApplAudition.Services;

/// <summary>
/// Résultat du traitement DSP d'un buffer audio.
/// </summary>
public record DspResult
{
    /// <summary>
    /// Valeur RMS (Root Mean Square) du signal.
    /// </summary>
    public float Rms { get; init; }

    /// <summary>
    /// Niveau en dBFS (decibels Full Scale).
    /// 0 dBFS = amplitude maximale, valeurs négatives pour signaux plus faibles.
    /// </summary>
    public float DbFs { get; init; }

    /// <summary>
    /// Timestamp de la mesure.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.Now;
}

/// <summary>
/// Interface du moteur de traitement DSP (Digital Signal Processing).
/// Responsable du calcul RMS, dBFS et fenêtrage.
/// </summary>
public interface IDspEngine
{
    /// <summary>
    /// Traite un buffer audio et retourne les valeurs calculées.
    /// </summary>
    /// <param name="buffer">Buffer audio mono (float -1.0 à +1.0)</param>
    /// <returns>Résultat du traitement DSP</returns>
    DspResult ProcessBuffer(float[] buffer);

    /// <summary>
    /// Calcule la valeur RMS (Root Mean Square) d'un buffer avec fenêtrage Hann.
    /// </summary>
    /// <param name="buffer">Buffer audio à analyser</param>
    /// <returns>Valeur RMS</returns>
    float CalculateRms(float[] buffer);

    /// <summary>
    /// Convertit une valeur RMS en dBFS (decibels Full Scale).
    /// </summary>
    /// <param name="rms">Valeur RMS à convertir</param>
    /// <returns>Niveau en dBFS (≤ 0)</returns>
    float RmsToDbfs(float rms);
}
