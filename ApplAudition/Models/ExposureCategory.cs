namespace ApplAudition.Models;

/// <summary>
/// Catégorie d'exposition sonore basée sur les seuils WHO/NIOSH avec biais conservateur.
/// </summary>
public enum ExposureCategory
{
    /// <summary>
    /// Niveau sûr (< 70 dB(A) avec biais conservateur).
    /// </summary>
    Safe,

    /// <summary>
    /// Niveau modéré (70-80 dB(A) avec biais conservateur).
    /// Exposition prolongée à limiter.
    /// </summary>
    Moderate,

    /// <summary>
    /// Niveau dangereux (95-110 dB(A)).
    /// Risque de dommage auditif permanent.
    /// </summary>
    Hazardous,

    /// <summary>
    /// Niveau critique (> 110 dB(A)).
    /// Risque immédiat de dommage auditif sévère et permanent.
    /// </summary>
    Critical
}
