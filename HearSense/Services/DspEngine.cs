using Serilog;

namespace HearSense.Services;

/// <summary>
/// Moteur de traitement DSP (Digital Signal Processing).
/// Implémente le calcul RMS avec fenêtrage Hann et conversion dBFS.
/// </summary>
public class DspEngine : IDspEngine
{
    private readonly ILogger _logger;
    private const float DB_FLOOR = -120.0f; // Plancher dB pour éviter -∞

    public DspEngine(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Traite un buffer audio complet : fenêtrage Hann, RMS et dBFS.
    /// </summary>
    /// <param name="buffer">Buffer audio mono (float -1.0 à +1.0)</param>
    /// <returns>Résultat du traitement DSP</returns>
    public DspResult ProcessBuffer(float[] buffer)
    {
        if (buffer == null || buffer.Length == 0)
        {
            _logger.Warning("Buffer audio vide ou null");
            return new DspResult { Rms = 0, DbFs = DB_FLOOR };
        }

        var rms = CalculateRms(buffer);
        var dbfs = RmsToDbfs(rms);

        return new DspResult
        {
            Rms = rms,
            DbFs = dbfs,
            Timestamp = DateTime.Now
        };
    }

    /// <summary>
    /// Calcule la valeur RMS avec fenêtrage Hann.
    /// Formule Hann : w[n] = 0.5 * (1 - cos(2π*n/(N-1)))
    /// RMS = sqrt(Σ(samples[n]² * w[n]²) / N)
    /// </summary>
    /// <param name="buffer">Buffer audio à analyser</param>
    /// <returns>Valeur RMS</returns>
    public float CalculateRms(float[] buffer)
    {
        if (buffer == null || buffer.Length == 0)
            return 0f;

        int length = buffer.Length;
        double sumSquares = 0.0;

        // Appliquer fenêtre Hann et calculer somme des carrés
        for (int n = 0; n < length; n++)
        {
            // Fenêtre Hann : w[n] = 0.5 * (1 - cos(2π*n/(N-1)))
            double hannWindow = 0.5 * (1.0 - Math.Cos(2.0 * Math.PI * n / (length - 1)));

            // Signal fenêtré
            double windowedSample = buffer[n] * hannWindow;

            // Somme des carrés
            sumSquares += windowedSample * windowedSample;
        }

        // RMS = racine carrée de la moyenne des carrés
        double meanSquare = sumSquares / length;
        float rms = (float)Math.Sqrt(meanSquare);

        return rms;
    }

    /// <summary>
    /// Convertit une valeur RMS en dBFS.
    /// Formule : dBFS = 20 * log10(RMS)
    /// Clamp à -120 dB si RMS ≈ 0 pour éviter -∞
    /// </summary>
    /// <param name="rms">Valeur RMS à convertir</param>
    /// <returns>Niveau en dBFS (≤ 0)</returns>
    public float RmsToDbfs(float rms)
    {
        // Éviter log10(0) qui donne -∞
        if (rms <= 0.0f || float.IsNaN(rms) || float.IsInfinity(rms))
        {
            return DB_FLOOR;
        }

        // Formule : dBFS = 20 * log10(RMS)
        float dbfs = 20.0f * (float)Math.Log10(rms);

        // Clamp au plancher pour éviter valeurs extrêmes
        if (dbfs < DB_FLOOR)
        {
            dbfs = DB_FLOOR;
        }

        return dbfs;
    }
}
