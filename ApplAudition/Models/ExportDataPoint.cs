namespace ApplAudition.Models;

/// <summary>
/// Représente un point de données complet pour l'export CSV.
/// Contient toutes les informations nécessaires pour tracer l'historique d'exposition.
/// </summary>
public class ExportDataPoint
{
    /// <summary>
    /// Timestamp absolu du point de données.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Niveau numérique en dBFS (Full Scale).
    /// </summary>
    public float DbFs { get; set; }

    /// <summary>
    /// Niveau dB(A) (SPL estimé).
    /// </summary>
    public float DbA { get; set; }

    /// <summary>
    /// Niveau équivalent continu sur 1 minute (Leq).
    /// </summary>
    public float Leq1Min { get; set; }

    /// <summary>
    /// Pic (maximum) sur la période.
    /// </summary>
    public float Peak { get; set; }

    /// <summary>
    /// Constructeur par défaut.
    /// </summary>
    public ExportDataPoint()
    {
        Timestamp = DateTime.Now;
    }

    /// <summary>
    /// Constructeur avec tous les paramètres.
    /// </summary>
    public ExportDataPoint(
        DateTime timestamp,
        float dbFs,
        float dbA,
        float leq1Min,
        float peak)
    {
        Timestamp = timestamp;
        DbFs = dbFs;
        DbA = dbA;
        Leq1Min = leq1Min;
        Peak = peak;
    }
}
