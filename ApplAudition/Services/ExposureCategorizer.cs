using ApplAudition.Models;
using Serilog;

namespace ApplAudition.Services;

/// <summary>
/// Service de catégorisation de l'exposition sonore avec seuils personnalisables.
/// </summary>
public class ExposureCategorizer : IExposureCategorizer
{
    private readonly ILogger _logger;

    public ExposureCategorizer(ILogger logger)
    {
        _logger = logger;
        _logger.Information("ExposureCategorizer initialisé - Seuils personnalisables");
    }

    /// <summary>
    /// Catégorise un niveau sonore en dB(A) selon des seuils personnalisables.
    /// </summary>
    /// <param name="dbA">Niveau sonore en dB(A)</param>
    /// <param name="warningThreshold">Seuil d'avertissement (orange) en dB(A)</param>
    /// <param name="dangerThreshold">Seuil de danger (rouge) en dB(A)</param>
    /// <returns>Catégorie d'exposition</returns>
    public ExposureCategory CategorizeExposure(float dbA, float warningThreshold, float dangerThreshold)
    {
        // Gérer les valeurs invalides (NaN, Infinity, valeurs extrêmes)
        if (float.IsNaN(dbA) || float.IsInfinity(dbA) || dbA < -120.0f)
        {
            _logger.Warning("Valeur dB(A) invalide reçue: {DbA}, catégorisation en Safe par défaut", dbA);
            return ExposureCategory.Safe;
        }

        // Catégorisation selon les seuils personnalisés
        if (dbA < warningThreshold)
        {
            return ExposureCategory.Safe; // Vert : en dessous du seuil d'avertissement
        }
        else if (dbA < dangerThreshold)
        {
            return ExposureCategory.Moderate; // Orange : entre avertissement et danger
        }
        else
        {
            return ExposureCategory.Hazardous; // Rouge : au-dessus du seuil de danger
        }
    }

    /// <summary>
    /// Catégorise un niveau sonore en dB(A) selon les seuils par défaut.
    /// Seuils par défaut : Warning = 70 dB(A), Danger = 85 dB(A).
    /// </summary>
    /// <param name="dbA">Niveau sonore en dB(A)</param>
    /// <returns>Catégorie d'exposition</returns>
    public ExposureCategory CategorizeExposure(float dbA)
    {
        return CategorizeExposure(dbA, 70.0f, 85.0f);
    }
}
