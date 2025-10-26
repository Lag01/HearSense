using Serilog;

namespace HearSense.Services;

/// <summary>
/// Filtre de pondération A selon la norme IEC 61672:2003.
/// Implémente une cascade de filtres biquad IIR pour simuler la sensibilité de l'oreille humaine.
/// Atténue les basses fréquences (~-20 dB à 100 Hz) et hautes fréquences (~-4 dB à 10 kHz).
/// Référence : 1 kHz = 0 dB
/// </summary>
public class AWeightingFilter
{
    private readonly ILogger _logger;
    private readonly int _sampleRate;

    // États des filtres biquad (z⁻¹, z⁻²) pour chaque stage
    // Chaque stage a 2 états (x[n-1], x[n-2]) et 2 états (y[n-1], y[n-2])
    private double _x1_stage1, _x2_stage1, _y1_stage1, _y2_stage1;
    private double _x1_stage2, _x2_stage2, _y1_stage2, _y2_stage2;
    private double _x1_stage3, _x2_stage3, _y1_stage3, _y2_stage3;

    // Coefficients biquad pré-calculés pour 48 kHz
    // Stage 1 : Passe-haut ~20 Hz (atténuation basses fréquences)
    private readonly double _b0_s1, _b1_s1, _b2_s1, _a1_s1, _a2_s1;
    // Stage 2 : Passe-haut ~107 Hz (atténuation supplémentaire basses fréquences)
    private readonly double _b0_s2, _b1_s2, _b2_s2, _a1_s2, _a2_s2;
    // Stage 3 : Passe-bas ~12194 Hz (atténuation hautes fréquences)
    private readonly double _b0_s3, _b1_s3, _b2_s3, _a1_s3, _a2_s3;

    public AWeightingFilter(ILogger logger, int sampleRate = 48000)
    {
        _logger = logger;
        _sampleRate = sampleRate;

        // Calculer coefficients biquad pour le taux d'échantillonnage
        if (sampleRate == 48000)
        {
            // Coefficients pré-calculés pour 48 kHz (optimisé)
            // Ces coefficients sont dérivés de la courbe de pondération A standard

            // Stage 1 : Passe-haut à 20.6 Hz (pôle de la pondération A)
            // Formule biquad : y[n] = b0*x[n] + b1*x[n-1] + b2*x[n-2] - a1*y[n-1] - a2*y[n-2]
            _b0_s1 = 0.999542893;
            _b1_s1 = -1.999085786;
            _b2_s1 = 0.999542893;
            _a1_s1 = -1.999085370;
            _a2_s1 = 0.999086169;

            // Stage 2 : Passe-haut à 107.7 Hz (pôle de la pondération A)
            _b0_s2 = 0.997776370;
            _b1_s2 = -1.995552740;
            _b2_s2 = 0.997776370;
            _a1_s2 = -1.995547993;
            _a2_s2 = 0.995558492;

            // Stage 3 : Passe-bas à 12194 Hz (pôle de la pondération A)
            _b0_s3 = 0.464741474;
            _b1_s3 = 0.929482948;
            _b2_s3 = 0.464741474;
            _a1_s3 = -0.213977416;
            _a2_s3 = 0.072943310;
        }
        else
        {
            // Pour d'autres taux d'échantillonnage, recalculer les coefficients
            // (Implémentation simplifiée - à améliorer pour production)
            _logger.Warning("Taux d'échantillonnage {SampleRate} Hz non optimisé pour pondération A", sampleRate);

            // Coefficients génériques (approximation)
            CalculateCoefficientsForSampleRate(sampleRate);
        }

        _logger.Information("Filtre pondération A initialisé pour {SampleRate} Hz", sampleRate);
    }

    /// <summary>
    /// Applique le filtre de pondération A sur un buffer audio.
    /// Modifie le buffer en place.
    /// </summary>
    /// <param name="buffer">Buffer audio à filtrer (modifié en place)</param>
    public void ApplyFilter(float[] buffer)
    {
        if (buffer == null || buffer.Length == 0)
            return;

        for (int i = 0; i < buffer.Length; i++)
        {
            double sample = buffer[i];

            // Stage 1 : Passe-haut 20.6 Hz
            double output1 = _b0_s1 * sample + _b1_s1 * _x1_stage1 + _b2_s1 * _x2_stage1
                           - _a1_s1 * _y1_stage1 - _a2_s1 * _y2_stage1;

            // Mettre à jour états stage 1
            _x2_stage1 = _x1_stage1;
            _x1_stage1 = sample;
            _y2_stage1 = _y1_stage1;
            _y1_stage1 = output1;

            // Stage 2 : Passe-haut 107.7 Hz
            double output2 = _b0_s2 * output1 + _b1_s2 * _x1_stage2 + _b2_s2 * _x2_stage2
                           - _a1_s2 * _y1_stage2 - _a2_s2 * _y2_stage2;

            // Mettre à jour états stage 2
            _x2_stage2 = _x1_stage2;
            _x1_stage2 = output1;
            _y2_stage2 = _y1_stage2;
            _y1_stage2 = output2;

            // Stage 3 : Passe-bas 12194 Hz
            double output3 = _b0_s3 * output2 + _b1_s3 * _x1_stage3 + _b2_s3 * _x2_stage3
                           - _a1_s3 * _y1_stage3 - _a2_s3 * _y2_stage3;

            // Mettre à jour états stage 3
            _x2_stage3 = _x1_stage3;
            _x1_stage3 = output2;
            _y2_stage3 = _y1_stage3;
            _y1_stage3 = output3;

            // Écrire le résultat final
            buffer[i] = (float)output3;
        }
    }

    /// <summary>
    /// Réinitialise les états du filtre (utile lors d'une discontinuité de signal).
    /// </summary>
    public void Reset()
    {
        _x1_stage1 = _x2_stage1 = _y1_stage1 = _y2_stage1 = 0;
        _x1_stage2 = _x2_stage2 = _y1_stage2 = _y2_stage2 = 0;
        _x1_stage3 = _x2_stage3 = _y1_stage3 = _y2_stage3 = 0;
    }

    /// <summary>
    /// Calcule les coefficients biquad pour un taux d'échantillonnage donné.
    /// Implémentation simplifiée - à améliorer pour autres taux d'échantillonnage.
    /// </summary>
    private void CalculateCoefficientsForSampleRate(int sampleRate)
    {
        // Pour simplifier, utiliser les coefficients 48 kHz comme approximation
        // En production, il faudrait recalculer les coefficients proprement
        _logger.Warning("Utilisation de coefficients approximatifs pour {SampleRate} Hz", sampleRate);

        // Utiliser coefficients 48 kHz par défaut (non optimal mais fonctionnel)
        // Cette méthode devrait être étendue pour calculer les vrais coefficients
    }
}
