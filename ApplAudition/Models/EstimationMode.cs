namespace ApplAudition.Models;

/// <summary>
/// Mode d'estimation du niveau SPL.
/// </summary>
public enum EstimationMode
{
    /// <summary>
    /// Mode A : Zero-Input Conservateur.
    /// Affiche dB(A) relatif sans estimation SPL absolue.
    /// </summary>
    ModeA,

    /// <summary>
    /// Mode B : Auto-profil Heuristique.
    /// Utilise un profil de casque pour estimer SPL absolu (±6 dB marge).
    /// </summary>
    ModeB
}
