using Serilog;
using System.IO;

namespace HearSense.Services;

/// <summary>
/// Gestionnaire de notifications pour les dépassements de seuil critique.
/// Surveille le niveau dB(A) et envoie des notifications Toast natives Windows.
/// </summary>
public class NotificationManager : INotificationManager
{
    private readonly ISettingsService _settingsService;
    private readonly IToastNotificationService _toastNotificationService;
    private readonly ILogger _logger;

    private DateTime _lastNotificationTime = DateTime.MinValue;
    private bool _hasNotifiedThisSession = false;
    private bool _isAboveThreshold = false;

    // Circuit breaker pour éviter les boucles d'erreurs
    private int _consecutiveFailures = 0;
    private const int MAX_CONSECUTIVE_FAILURES = 3;
    private bool _circuitBreakerOpen = false;
    private DateTime _circuitBreakerResetTime = DateTime.MinValue;
    private readonly TimeSpan _circuitBreakerResetDuration = TimeSpan.FromMinutes(5);

    public NotificationManager(
        ISettingsService settingsService,
        IToastNotificationService toastNotificationService,
        ILogger logger)
    {
        _settingsService = settingsService;
        _toastNotificationService = toastNotificationService;
        _logger = logger;
    }

    /// <summary>
    /// Vérifie si le niveau dB(A) actuel dépasse le seuil critique
    /// et déclenche une notification si nécessaire.
    /// </summary>
    public void CheckThreshold(float currentDbA)
    {
        var settings = _settingsService.Settings;

        // Vérifier si les notifications sont activées
        if (!settings.EnableNotifications)
            return;

        // Vérifier si le niveau dépasse le seuil
        bool isCurrentlyAboveThreshold = currentDbA >= settings.CriticalThresholdDbA;

        // Si le niveau est repassé en dessous du seuil, réinitialiser l'état
        if (_isAboveThreshold && !isCurrentlyAboveThreshold)
        {
            _isAboveThreshold = false;
            _logger.Debug("Niveau repassé sous le seuil critique ({Threshold} dB(A))",
                settings.CriticalThresholdDbA);
            return;
        }

        // Si le niveau est au-dessus du seuil
        if (isCurrentlyAboveThreshold)
        {
            // Marquer qu'on est au-dessus du seuil
            if (!_isAboveThreshold)
            {
                _isAboveThreshold = true;
                _logger.Warning("Niveau sonore au-dessus du seuil critique : {CurrentDbA:F1} dB(A) >= {Threshold:F1} dB(A)",
                    currentDbA, settings.CriticalThresholdDbA);
            }

            // Vérifier si on peut notifier selon les règles
            if (ShouldNotify())
            {
                SendNotification(currentDbA, settings.CriticalThresholdDbA);
            }
        }
    }

    /// <summary>
    /// Détermine si une notification doit être envoyée selon les règles configurées.
    /// </summary>
    private bool ShouldNotify()
    {
        var settings = _settingsService.Settings;

        // Règle 1 : Notification unique par session
        if (settings.NotificationShowOncePerSession && _hasNotifiedThisSession)
        {
            return false;
        }

        // Règle 2 : Cooldown entre notifications
        var timeSinceLastNotification = DateTime.Now - _lastNotificationTime;
        var cooldownDuration = TimeSpan.FromMinutes(settings.NotificationCooldownMinutes);

        if (timeSinceLastNotification < cooldownDuration)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Envoie une notification de dépassement de seuil via Toast Notification native.
    /// </summary>
    private void SendNotification(float currentDbA, float threshold)
    {
        try
        {
            // Vérifier le circuit breaker
            if (_circuitBreakerOpen)
            {
                if (DateTime.Now >= _circuitBreakerResetTime)
                {
                    // Réinitialiser le circuit breaker
                    _circuitBreakerOpen = false;
                    _consecutiveFailures = 0;
                    _logger.Information("Circuit breaker des notifications réinitialisé");
                }
                else
                {
                    // Circuit breaker toujours ouvert, ne pas envoyer
                    _logger.Debug("Circuit breaker ouvert, notification ignorée");
                    return;
                }
            }

            string title = "⚠️ Niveau sonore élevé";
            string message = $"Niveau actuel : {currentDbA:F0} dB(A)\n" +
                           $"Limite recommandée : {threshold:F0} dB(A)";

            // Chemin de l'icône personnalisée
            string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "icon.ico");

            // Utiliser le service Toast Notification avec icône personnalisée
            _toastNotificationService.ShowToast(title, message, iconPath);

            _lastNotificationTime = DateTime.Now;
            _hasNotifiedThisSession = true;
            _consecutiveFailures = 0; // Reset sur succès

            _logger.Information("Notification Toast envoyée : {CurrentDbA:F1} dB(A) >= {Threshold:F1} dB(A)",
                currentDbA, threshold);
        }
        catch (Exception ex)
        {
            _consecutiveFailures++;
            _logger.Error(ex, "Erreur lors de l'envoi de la notification Toast (échec {FailureCount}/{MaxFailures})",
                _consecutiveFailures, MAX_CONSECUTIVE_FAILURES);

            // Ouvrir le circuit breaker après trop d'échecs
            if (_consecutiveFailures >= MAX_CONSECUTIVE_FAILURES)
            {
                _circuitBreakerOpen = true;
                _circuitBreakerResetTime = DateTime.Now.Add(_circuitBreakerResetDuration);

                _logger.Warning(
                    "Circuit breaker des notifications ouvert après {FailureCount} échecs consécutifs. " +
                    "Réinitialisation prévue à {ResetTime}",
                    _consecutiveFailures,
                    _circuitBreakerResetTime);
            }
        }
    }

    /// <summary>
    /// Réinitialise l'état de notification.
    /// Utile après changement de seuil ou pour forcer une nouvelle notification.
    /// </summary>
    public void ResetNotificationState()
    {
        _hasNotifiedThisSession = false;
        _isAboveThreshold = false;
        _lastNotificationTime = DateTime.MinValue;

        _logger.Debug("État de notification réinitialisé");
    }
}
