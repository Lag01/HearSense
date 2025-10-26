namespace HearSense.Services;

/// <summary>
/// Interface pour le calcul du niveau équivalent continu (Leq) et du pic.
/// Utilise un buffer circulaire pour maintenir l'historique des mesures.
/// </summary>
public interface ILeqCalculator
{
    /// <summary>
    /// Ajoute un nouvel échantillon dBFS au buffer circulaire.
    /// </summary>
    /// <param name="dbfs">Valeur dBFS à ajouter</param>
    void AddSample(float dbfs);

    /// <summary>
    /// Calcule le niveau équivalent continu (Leq) sur la période du buffer.
    /// Formule : Leq = 10 * log10(mean(10^(dBFS_i / 10)))
    /// </summary>
    /// <returns>Leq en dB(A)</returns>
    float GetLeq();

    /// <summary>
    /// Retourne le pic (maximum) sur la période du buffer.
    /// </summary>
    /// <returns>Pic en dBFS</returns>
    float GetPeak();

    /// <summary>
    /// Retourne le nombre d'échantillons actuellement dans le buffer.
    /// </summary>
    int GetSampleCount();

    /// <summary>
    /// Réinitialise le buffer circulaire.
    /// </summary>
    void Reset();
}
