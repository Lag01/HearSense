using ApplAudition.Models;

namespace ApplAudition.Services;

/// <summary>
/// Interface du service de catégorisation de l'exposition sonore.
/// Responsable de classer les niveaux dB(A) en catégories Safe/Moderate/Hazardous/Critical.
/// </summary>
public interface IExposureCategorizer
{
    /// <summary>
    /// Catégorise un niveau sonore en dB(A) selon les seuils définis.
    /// </summary>
    /// <param name="dbA">Niveau sonore en dB(A)</param>
    /// <returns>Catégorie d'exposition (Safe, Moderate, Hazardous ou Critical)</returns>
    ExposureCategory CategorizeExposure(float dbA);
}
