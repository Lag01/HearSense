namespace ApplAudition.Services;

/// <summary>
/// Gestionnaire simplifié d'estimation SPL.
/// </summary>
public interface IEstimationModeManager
{
    /// <summary>
    /// Initialise le gestionnaire.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Estime le niveau SPL à partir du dBFS.
    /// Formule : SPL_est = dBFS + volume_système_dB + offset_dynamique(dBFS)
    /// </summary>
    /// <param name="dbfs">Niveau en dBFS (calculé par DSP)</param>
    /// <returns>SPL estimé en dB(A)</returns>
    float EstimateSpl(float dbfs);
}
