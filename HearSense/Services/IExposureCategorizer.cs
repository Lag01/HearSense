using HearSense.Models;

namespace HearSense.Services;

/// <summary>
/// Interface du service de catégorisation de l'exposition sonore.
/// Responsable de classer les niveaux dB(A) en catégories Safe/Moderate/Hazardous selon les seuils personnalisables.
/// </summary>
public interface IExposureCategorizer
{
    /// <summary>
    /// Catégorise un niveau sonore en dB(A) selon des seuils personnalisables.
    /// </summary>
    /// <param name="dbA">Niveau sonore en dB(A)</param>
    /// <param name="warningThreshold">Seuil d'avertissement (orange) en dB(A)</param>
    /// <param name="dangerThreshold">Seuil de danger (rouge) en dB(A)</param>
    /// <param name="criticalThreshold">Seuil critique (rouge très foncé) en dB(A)</param>
    /// <returns>Catégorie d'exposition</returns>
    ExposureCategory CategorizeExposure(float dbA, float warningThreshold, float dangerThreshold, float criticalThreshold);

    /// <summary>
    /// Catégorise un niveau sonore en dB(A) selon les seuils par défaut.
    /// </summary>
    /// <param name="dbA">Niveau sonore en dB(A)</param>
    /// <returns>Catégorie d'exposition</returns>
    ExposureCategory CategorizeExposure(float dbA);
}
