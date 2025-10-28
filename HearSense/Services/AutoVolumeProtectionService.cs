using Serilog;
using System.IO;

namespace HearSense.Services;

/// <summary>
/// Service de protection automatique du volume.
/// Surveille le niveau sonore en temps réel et réduit automatiquement le volume système
/// lorsque le seuil configuré est dépassé.
/// </summary>
public class AutoVolumeProtectionService : IAutoVolumeProtectionService
{
    private readonly ILogger _logger;
    private readonly ISystemVolumeService _systemVolumeService;
    private readonly ISettingsService _settingsService;
    private readonly IToastNotificationService _toastNotificationService;

    private DateTime _lastProtectionTrigger = DateTime.MinValue;
    private const int COOLDOWN_SECONDS = 5;
    private int _monitorCallCount = 0;

    public AutoVolumeProtectionService(
        ILogger logger,
        ISystemVolumeService systemVolumeService,
        ISettingsService settingsService,
        IToastNotificationService toastNotificationService)
    {
        _logger = logger;
        _systemVolumeService = systemVolumeService;
        _settingsService = settingsService;
        _toastNotificationService = toastNotificationService;
    }

    /// <summary>
    /// Surveille le niveau sonore actuel et applique la protection si nécessaire.
    /// </summary>
    /// <param name="currentDbA">Niveau sonore actuel en dB(A)</param>
    public void MonitorAndProtect(float currentDbA)
    {
        _monitorCallCount++;

        // Vérifier si la protection est activée
        if (!_settingsService.Settings.IsAutoVolumeProtectionEnabled)
        {
            // Log uniquement tous les 100 appels pour éviter le spam
            if (_monitorCallCount % 100 == 0)
            {
                _logger.Debug("Protection automatique désactivée (appel #{Count}, niveau actuel: {DbA:F1} dB(A))",
                    _monitorCallCount, currentDbA);
            }
            return;
        }

        // Vérifier si le seuil est dépassé
        float threshold = _settingsService.Settings.VolumeProtectionThresholdDbA;
        if (currentDbA <= threshold)
        {
            // Log périodique pour monitoring
            if (_monitorCallCount % 100 == 0)
            {
                _logger.Debug("Monitoring protection auto : {CurrentDbA:F1} dB(A) <= {Threshold:F1} dB(A) (OK, appel #{Count})",
                    currentDbA, threshold, _monitorCallCount);
            }
            return;
        }

        // ⚠️ SEUIL DÉPASSÉ !
        _logger.Warning("⚠️ Seuil de protection DÉPASSÉ : {CurrentDbA:F1} dB(A) > {Threshold:F1} dB(A)",
            currentDbA, threshold);

        // Vérifier le cooldown pour éviter les ajustements trop fréquents
        TimeSpan timeSinceLastTrigger = DateTime.Now - _lastProtectionTrigger;
        if (timeSinceLastTrigger.TotalSeconds < COOLDOWN_SECONDS)
        {
            _logger.Debug("Cooldown actif : {Elapsed:F1}s / {Cooldown}s - Protection non déclenchée",
                timeSinceLastTrigger.TotalSeconds, COOLDOWN_SECONDS);
            return;
        }

        // Calculer la réduction nécessaire
        ApplyVolumeReduction(currentDbA, threshold);

        // Mettre à jour le timestamp du dernier déclenchement
        _lastProtectionTrigger = DateTime.Now;
    }

    /// <summary>
    /// Applique la réduction de volume nécessaire pour ramener le niveau sous le seuil.
    /// </summary>
    private void ApplyVolumeReduction(float currentDbA, float threshold)
    {
        try
        {
            // Calculer l'excès en dB
            float excessDb = currentDbA - threshold;

            // Convertir l'excès dB en facteur de volume
            // Formule : Pour réduire de X dB, multiplier le volume par 10^(-X/20)
            float reductionFactor = (float)Math.Pow(10, -excessDb / 20.0);

            // Obtenir le volume actuel
            float currentVolume = _systemVolumeService.GetCurrentVolume();

            // Calculer le nouveau volume (avec une marge de sécurité de -2 dB supplémentaires)
            float targetVolume = currentVolume * reductionFactor * 0.794f; // 0.794 ≈ 10^(-2/20)

            // S'assurer que le volume ne tombe pas en dessous de 10%
            targetVolume = Math.Max(targetVolume, 0.1f);

            // Appliquer le nouveau volume
            _systemVolumeService.SetVolume(targetVolume);

            _logger.Warning(
                "Protection automatique activée - Niveau: {CurrentDbA:F1} dB(A), Seuil: {Threshold:F1} dB(A), " +
                "Volume réduit de {OldVolume:P0} à {NewVolume:P0}",
                currentDbA,
                threshold,
                currentVolume,
                targetVolume);

            // Chemin de l'icône personnalisée
            string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "icon.ico");

            // Afficher une notification Toast avec l'icône de l'application
            _toastNotificationService.ShowToast(
                "Protection Auditive Activée",
                $"Niveau sonore dangereux détecté ({currentDbA:F0} dB). Volume réduit automatiquement pour protéger votre audition.",
                iconPath);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Erreur lors de l'application de la réduction automatique du volume");
        }
    }

    /// <summary>
    /// Réinitialise le cooldown (utile quand l'utilisateur change les paramètres).
    /// </summary>
    public void ResetCooldown()
    {
        _lastProtectionTrigger = DateTime.MinValue;
        _logger.Debug("Cooldown de la protection automatique réinitialisé");
    }

    /// <summary>
    /// Teste la protection automatique en simulant un niveau sonore élevé (95 dB(A)).
    /// Utile pour vérifier que les notifications fonctionnent correctement.
    /// </summary>
    public void TestProtection()
    {
        _logger.Information("🧪 Test de la protection automatique déclenché manuellement");

        if (!_settingsService.Settings.IsAutoVolumeProtectionEnabled)
        {
            _logger.Warning("Test annulé : la protection automatique est désactivée");
            _toastNotificationService.ShowToast(
                "Test Protection Automatique",
                "La protection automatique est désactivée. Activez-la dans les paramètres pour tester.",
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "icon.ico"));
            return;
        }

        float threshold = _settingsService.Settings.VolumeProtectionThresholdDbA;
        float simulatedLevel = threshold + 10.0f; // Simuler un dépassement de 10 dB

        _logger.Information("Simulation d'un niveau sonore de {SimulatedLevel:F1} dB(A) (seuil: {Threshold:F1} dB(A))",
            simulatedLevel, threshold);

        // Réinitialiser le cooldown pour forcer le test
        ResetCooldown();

        // Déclencher la protection avec le niveau simulé
        MonitorAndProtect(simulatedLevel);

        _logger.Information("Test de protection terminé");
    }
}
