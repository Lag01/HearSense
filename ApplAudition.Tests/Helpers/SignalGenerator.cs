namespace ApplAudition.Tests.Helpers;

/// <summary>
/// Générateur de signaux de test pour les tests DSP.
/// </summary>
public static class SignalGenerator
{
    /// <summary>
    /// Génère un signal sinusoïdal pur.
    /// </summary>
    /// <param name="frequency">Fréquence en Hz</param>
    /// <param name="sampleRate">Taux d'échantillonnage en Hz</param>
    /// <param name="durationSeconds">Durée en secondes</param>
    /// <param name="amplitude">Amplitude (0.0 à 1.0)</param>
    /// <returns>Buffer audio contenant le signal sinusoïdal</returns>
    public static float[] GenerateSineWave(double frequency, int sampleRate, double durationSeconds, double amplitude = 1.0)
    {
        int sampleCount = (int)(sampleRate * durationSeconds);
        float[] buffer = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            // Formule : y = A * sin(2π * f * t)
            // où t = i / sampleRate
            double t = (double)i / sampleRate;
            double value = amplitude * Math.Sin(2.0 * Math.PI * frequency * t);
            buffer[i] = (float)value;
        }

        return buffer;
    }

    /// <summary>
    /// Génère un signal constant (DC).
    /// </summary>
    /// <param name="sampleCount">Nombre d'échantillons</param>
    /// <param name="value">Valeur constante</param>
    /// <returns>Buffer audio avec valeur constante</returns>
    public static float[] GenerateConstant(int sampleCount, float value)
    {
        float[] buffer = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            buffer[i] = value;
        }

        return buffer;
    }

    /// <summary>
    /// Génère un signal de bruit blanc.
    /// </summary>
    /// <param name="sampleCount">Nombre d'échantillons</param>
    /// <param name="amplitude">Amplitude maximale</param>
    /// <param name="seed">Seed pour le générateur aléatoire (pour reproductibilité)</param>
    /// <returns>Buffer audio avec bruit blanc</returns>
    public static float[] GenerateWhiteNoise(int sampleCount, double amplitude = 1.0, int? seed = null)
    {
        Random random = seed.HasValue ? new Random(seed.Value) : new Random();
        float[] buffer = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            // Génère une valeur aléatoire entre -amplitude et +amplitude
            buffer[i] = (float)(amplitude * (2.0 * random.NextDouble() - 1.0));
        }

        return buffer;
    }

    /// <summary>
    /// Calcule le RMS d'un buffer (sans fenêtrage).
    /// Utilisé pour vérifier les tests.
    /// </summary>
    /// <param name="buffer">Buffer audio</param>
    /// <returns>Valeur RMS</returns>
    public static float CalculateRmsSimple(float[] buffer)
    {
        if (buffer == null || buffer.Length == 0)
            return 0f;

        double sumSquares = 0.0;

        foreach (float sample in buffer)
        {
            sumSquares += sample * sample;
        }

        return (float)Math.Sqrt(sumSquares / buffer.Length);
    }

    /// <summary>
    /// Calcule l'amplitude RMS théorique d'une sinusoïde.
    /// Pour une sinusoïde A*sin(ωt), RMS = A / √2 ≈ 0.707 * A
    /// </summary>
    /// <param name="amplitude">Amplitude de la sinusoïde</param>
    /// <returns>Valeur RMS théorique</returns>
    public static float GetTheoreticalSineRms(double amplitude)
    {
        // RMS d'une sinusoïde = A / √2
        return (float)(amplitude / Math.Sqrt(2.0));
    }
}
