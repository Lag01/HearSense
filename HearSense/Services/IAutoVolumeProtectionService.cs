namespace HearSense.Services;

/// <summary>
/// Interface pour le service de protection automatique du volume.
/// Surveille le niveau sonore et réduit automatiquement le volume système si le seuil est dépassé.
/// </summary>
public interface IAutoVolumeProtectionService
{
    /// <summary>
    /// Surveille le niveau sonore actuel et applique la protection si nécessaire.
    /// </summary>
    /// <param name="currentDbA">Niveau sonore actuel en dB(A)</param>
    void MonitorAndProtect(float currentDbA);

    /// <summary>
    /// Réinitialise le cooldown (utile quand l'utilisateur change les paramètres).
    /// </summary>
    void ResetCooldown();

    /// <summary>
    /// Teste la protection automatique en simulant un niveau sonore élevé.
    /// Utile pour vérifier que les notifications fonctionnent correctement.
    /// </summary>
    void TestProtection();
}
