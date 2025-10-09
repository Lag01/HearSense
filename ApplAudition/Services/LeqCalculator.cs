using Serilog;

namespace ApplAudition.Services;

/// <summary>
/// Calculateur de niveau équivalent continu (Leq) et de pic.
/// Utilise un buffer circulaire de 1 minute (480 échantillons à 125 ms).
/// </summary>
public class LeqCalculator : ILeqCalculator
{
    private readonly ILogger _logger;
    private readonly int _bufferSize;
    private readonly float[] _circularBuffer;
    private int _writeIndex;
    private int _sampleCount;
    private readonly object _lock = new();

    private const float DB_FLOOR = -120.0f; // Plancher dB

    /// <summary>
    /// Initialise le calculateur Leq avec un buffer de 1 minute.
    /// </summary>
    /// <param name="logger">Logger pour diagnostics</param>
    /// <param name="durationSeconds">Durée du buffer en secondes (défaut: 60s)</param>
    /// <param name="updateIntervalMs">Intervalle de mise à jour en ms (défaut: 125ms)</param>
    public LeqCalculator(ILogger logger, int durationSeconds = 60, int updateIntervalMs = 125)
    {
        _logger = logger;

        // Calculer taille du buffer : 60s / 0.125s = 480 échantillons
        _bufferSize = (durationSeconds * 1000) / updateIntervalMs;
        _circularBuffer = new float[_bufferSize];

        // Initialiser avec valeur plancher
        for (int i = 0; i < _bufferSize; i++)
        {
            _circularBuffer[i] = DB_FLOOR;
        }

        _writeIndex = 0;
        _sampleCount = 0;

        _logger.Information("LeqCalculator initialisé : buffer de {BufferSize} échantillons ({Duration}s)",
            _bufferSize, durationSeconds);
    }

    /// <summary>
    /// Ajoute un nouvel échantillon dBFS au buffer circulaire.
    /// </summary>
    /// <param name="dbfs">Valeur dBFS à ajouter</param>
    public void AddSample(float dbfs)
    {
        lock (_lock)
        {
            // Ajouter au buffer circulaire
            _circularBuffer[_writeIndex] = dbfs;

            // Incrémenter index (circulaire)
            _writeIndex = (_writeIndex + 1) % _bufferSize;

            // Incrémenter compteur (max = bufferSize)
            if (_sampleCount < _bufferSize)
            {
                _sampleCount++;
            }
        }
    }

    /// <summary>
    /// Calcule le niveau équivalent continu (Leq) sur la période du buffer.
    /// Formule : Leq = 10 * log10(mean(10^(dBFS_i / 10)))
    /// Moyenne logarithmique énergétique conforme aux normes acoustiques.
    /// </summary>
    /// <returns>Leq en dB(A)</returns>
    public float GetLeq()
    {
        lock (_lock)
        {
            if (_sampleCount == 0)
            {
                return DB_FLOOR;
            }

            // Calculer la moyenne énergétique
            double sumEnergy = 0.0;

            for (int i = 0; i < _sampleCount; i++)
            {
                float dbfs = _circularBuffer[i];

                // Convertir dB en énergie linéaire : 10^(dBFS / 10)
                double energy = Math.Pow(10.0, dbfs / 10.0);
                sumEnergy += energy;
            }

            // Moyenne énergétique
            double meanEnergy = sumEnergy / _sampleCount;

            // Reconvertir en dB : 10 * log10(moyenne)
            float leq = 10.0f * (float)Math.Log10(meanEnergy);

            // Clamp au plancher
            if (leq < DB_FLOOR || float.IsNaN(leq) || float.IsInfinity(leq))
            {
                leq = DB_FLOOR;
            }

            return leq;
        }
    }

    /// <summary>
    /// Retourne le pic (maximum) sur la période du buffer.
    /// </summary>
    /// <returns>Pic en dBFS</returns>
    public float GetPeak()
    {
        lock (_lock)
        {
            if (_sampleCount == 0)
            {
                return DB_FLOOR;
            }

            float peak = DB_FLOOR;

            for (int i = 0; i < _sampleCount; i++)
            {
                if (_circularBuffer[i] > peak)
                {
                    peak = _circularBuffer[i];
                }
            }

            return peak;
        }
    }

    /// <summary>
    /// Retourne le nombre d'échantillons actuellement dans le buffer.
    /// </summary>
    public int GetSampleCount()
    {
        lock (_lock)
        {
            return _sampleCount;
        }
    }

    /// <summary>
    /// Réinitialise le buffer circulaire.
    /// </summary>
    public void Reset()
    {
        lock (_lock)
        {
            for (int i = 0; i < _bufferSize; i++)
            {
                _circularBuffer[i] = DB_FLOOR;
            }

            _writeIndex = 0;
            _sampleCount = 0;

            _logger.Information("Buffer Leq réinitialisé");
        }
    }
}
