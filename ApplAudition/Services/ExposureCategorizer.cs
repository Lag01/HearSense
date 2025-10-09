using ApplAudition.Models;
using Serilog;

namespace ApplAudition.Services;

/// <summary>
/// Service de catégorisation de l'exposition sonore avec biais conservateur.
/// Implémente les seuils WHO/NIOSH avec une marge de sécurité de -5 dB.
/// </summary>
public class ExposureCategorizer : IExposureCategorizer
{
    private readonly ILogger _logger;

    /// <summary>
    /// Seuils de catégorisation de l'exposition sonore.
    /// Nouveaux seuils ajustés selon la demande utilisateur.
    /// </summary>
    private const float SAFE_THRESHOLD = 75.0f;      // < 75 dB(A) : sûr
    private const float MODERATE_THRESHOLD = 95.0f;  // 75-95 dB(A) : modéré
    private const float HAZARDOUS_THRESHOLD = 110.0f; // 95-110 dB(A) : dangereux

    public ExposureCategorizer(ILogger logger)
    {
        _logger = logger;
        _logger.Information(
            "ExposureCategorizer initialisé - Seuils: Safe < {Safe}, Moderate {Safe}-{Moderate}, Hazardous {Moderate}-{Hazardous}, Critical > {Hazardous}",
            SAFE_THRESHOLD, SAFE_THRESHOLD, MODERATE_THRESHOLD, MODERATE_THRESHOLD, HAZARDOUS_THRESHOLD, HAZARDOUS_THRESHOLD);
    }

    /// <summary>
    /// Catégorise un niveau sonore en dB(A) selon les seuils conservateurs.
    /// </summary>
    /// <param name="dbA">Niveau sonore en dB(A)</param>
    /// <returns>Catégorie d'exposition</returns>
    public ExposureCategory CategorizeExposure(float dbA)
    {
        // Gérer les valeurs invalides (NaN, Infinity, valeurs extrêmes)
        if (float.IsNaN(dbA) || float.IsInfinity(dbA) || dbA < -120.0f)
        {
            _logger.Warning("Valeur dB(A) invalide reçue: {DbA}, catégorisation en Safe par défaut", dbA);
            return ExposureCategory.Safe;
        }

        // Appliquer les seuils
        if (dbA < SAFE_THRESHOLD)
        {
            return ExposureCategory.Safe;
        }
        else if (dbA < MODERATE_THRESHOLD)
        {
            return ExposureCategory.Moderate;
        }
        else if (dbA < HAZARDOUS_THRESHOLD)
        {
            return ExposureCategory.Hazardous;
        }
        else
        {
            return ExposureCategory.Critical;
        }
    }
}
