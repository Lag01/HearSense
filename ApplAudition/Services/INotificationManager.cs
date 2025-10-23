namespace ApplAudition.Services;

/// <summary>
/// Interface pour le gestionnaire de notifications de dépassement de seuil critique.
/// </summary>
public interface INotificationManager
{
    /// <summary>
    /// Vérifie si le niveau dB(A) actuel dépasse le seuil critique
    /// et déclenche une notification si nécessaire.
    /// </summary>
    /// <param name="currentDbA">Niveau sonore actuel en dB(A)</param>
    void CheckThreshold(float currentDbA);

    /// <summary>
    /// Réinitialise l'état de notification (par exemple, après changement de seuil).
    /// </summary>
    void ResetNotificationState();
}
