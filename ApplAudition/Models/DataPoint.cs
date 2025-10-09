namespace ApplAudition.Models;

/// <summary>
/// Représente un point de données pour le graphe historique (Phase 6 - Tâche 15).
/// </summary>
public class DataPoint
{
    /// <summary>
    /// Timestamp du point (secondes écoulées depuis le début de la capture).
    /// </summary>
    public double Time { get; set; }

    /// <summary>
    /// Valeur dB(A) (relatif ou SPL estimé selon le mode).
    /// </summary>
    public double Value { get; set; }

    public DataPoint(double time, double value)
    {
        Time = time;
        Value = value;
    }
}
