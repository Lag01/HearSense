using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ApplAudition.Services;
using Serilog;
using System.Windows;
using Application = System.Windows.Application;

namespace ApplAudition.ViewModels;

/// <summary>
/// ViewModel pour la fenêtre de paramètres.
/// Gère la configuration de la limite critique, notifications, et comportement de l'application.
/// </summary>
public partial class SettingsViewModel : BaseViewModel
{
    private readonly ISettingsService _settingsService;
    private readonly IStartupManager _startupManager;
    private readonly INotificationManager _notificationManager;
    private readonly ITrayController _trayController;
    private readonly ILogger _logger;

    #region Propriétés observables

    [ObservableProperty]
    private float _criticalThresholdDbA;

    [ObservableProperty]
    private bool _enableNotifications;

    [ObservableProperty]
    private int _notificationCooldownMinutes;

    [ObservableProperty]
    private bool _notificationShowOncePerSession;

    [ObservableProperty]
    private bool _startWithWindows;

    [ObservableProperty]
    private bool _startMinimized;

    [ObservableProperty]
    private bool _minimizeToTrayOnClose;

    [ObservableProperty]
    private string _statusMessage = "";

    #endregion

    public SettingsViewModel(
        ISettingsService settingsService,
        IStartupManager startupManager,
        INotificationManager notificationManager,
        ITrayController trayController,
        ILogger logger)
    {
        _settingsService = settingsService;
        _startupManager = startupManager;
        _notificationManager = notificationManager;
        _trayController = trayController;
        _logger = logger;

        // Charger les paramètres actuels
        LoadCurrentSettings();
    }

    /// <summary>
    /// Charge les paramètres actuels depuis SettingsService.
    /// </summary>
    private void LoadCurrentSettings()
    {
        var settings = _settingsService.Settings;

        CriticalThresholdDbA = settings.CriticalThresholdDbA;
        EnableNotifications = settings.EnableNotifications;
        NotificationCooldownMinutes = settings.NotificationCooldownMinutes;
        NotificationShowOncePerSession = settings.NotificationShowOncePerSession;
        StartWithWindows = settings.StartWithWindows;
        StartMinimized = settings.StartMinimized;
        MinimizeToTrayOnClose = settings.MinimizeToTrayOnClose;

        _logger.Debug("Paramètres chargés dans SettingsViewModel");
    }

    /// <summary>
    /// Commande pour enregistrer les paramètres.
    /// </summary>
    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        try
        {
            // Validation stricte du seuil critique
            if (float.IsNaN(CriticalThresholdDbA) || float.IsInfinity(CriticalThresholdDbA))
            {
                StatusMessage = "⚠ Valeur invalide pour le seuil critique";
                _logger.Warning("Seuil critique invalide (NaN/Infinity)");
                return;
            }

            const float MIN_REALISTIC_THRESHOLD = 60.0f; // En dessous = trop silencieux
            const float MAX_REALISTIC_THRESHOLD = 110.0f; // Au-dessus = dommages immédiats

            if (CriticalThresholdDbA < MIN_REALISTIC_THRESHOLD || CriticalThresholdDbA > MAX_REALISTIC_THRESHOLD)
            {
                StatusMessage = $"⚠ Le seuil critique doit être entre {MIN_REALISTIC_THRESHOLD} et {MAX_REALISTIC_THRESHOLD} dB(A)\n" +
                               "Recommandation OMS : 85 dB(A) pour 8h/jour maximum";
                _logger.Warning("Seuil critique hors limites réalistes : {Threshold} dB(A)", CriticalThresholdDbA);
                return;
            }

            // Avertissement si valeur dangereusement élevée (> 100 dB)
            if (CriticalThresholdDbA > 100.0f)
            {
                var result = System.Windows.MessageBox.Show(
                    $"Un seuil de {CriticalThresholdDbA:F0} dB(A) est extrêmement élevé.\n\n" +
                    "⚠️ DANGER : Vous ne serez averti qu'en cas de niveau très dangereux.\n" +
                    "Des dommages auditifs irréversibles peuvent survenir avant cette limite.\n\n" +
                    "Êtes-vous sûr de vouloir définir un seuil aussi élevé ?",
                    "⚠️ Confirmation - Seuil Critique Élevé",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                {
                    _logger.Information("Utilisateur a annulé la définition du seuil élevé : {Threshold} dB(A)", CriticalThresholdDbA);
                    return;
                }
            }

            // Validation du cooldown
            if (NotificationCooldownMinutes < 1 || NotificationCooldownMinutes > 60)
            {
                StatusMessage = "⚠ Le cooldown doit être entre 1 et 60 minutes";
                _logger.Warning("Cooldown invalide : {Cooldown} minutes", NotificationCooldownMinutes);
                return;
            }

            // Mettre à jour les settings
            var settings = _settingsService.Settings;
            settings.CriticalThresholdDbA = CriticalThresholdDbA;
            settings.EnableNotifications = EnableNotifications;
            settings.NotificationCooldownMinutes = NotificationCooldownMinutes;
            settings.NotificationShowOncePerSession = NotificationShowOncePerSession;
            settings.StartWithWindows = StartWithWindows;
            settings.StartMinimized = StartMinimized;
            settings.MinimizeToTrayOnClose = MinimizeToTrayOnClose;

            // Sauvegarder
            await _settingsService.SaveAsync();

            // Gérer le démarrage automatique Windows
            _startupManager.SetStartup(StartWithWindows, StartMinimized);

            // Réinitialiser l'état de notification (nouveau seuil)
            _notificationManager.ResetNotificationState();

            StatusMessage = "✓ Paramètres enregistrés avec succès";
            _logger.Information("Paramètres sauvegardés : Seuil={Threshold} dB(A), Notifications={Enabled}",
                CriticalThresholdDbA, EnableNotifications);

            // Fermer la fenêtre après un court délai
            await Task.Delay(1000);
            CloseWindow();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Erreur lors de la sauvegarde des paramètres");
            StatusMessage = $"⚠ Erreur : {ex.Message}";
        }
    }

    /// <summary>
    /// Commande pour annuler et fermer la fenêtre sans sauvegarder.
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        _logger.Debug("Annulation des modifications de paramètres");
        CloseWindow();
    }

    /// <summary>
    /// Commande pour tester la notification.
    /// </summary>
    [RelayCommand]
    private void TestNotification()
    {
        try
        {
            _trayController.ShowBalloonTip(
                "⚠️ Niveau sonore élevé (Test)",
                $"Ceci est une notification de test.\nSeuil configuré : {CriticalThresholdDbA:F0} dB(A)",
                timeout: 5000
            );

            StatusMessage = "Notification de test envoyée";
            _logger.Information("Notification de test envoyée");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Erreur lors du test de notification");
            StatusMessage = $"⚠ Erreur : {ex.Message}";
        }
    }

    /// <summary>
    /// Ferme la fenêtre de paramètres.
    /// </summary>
    private void CloseWindow()
    {
        // La fenêtre sera fermée via le code-behind
        Application.Current.Dispatcher.Invoke(() =>
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.Close();
                    break;
                }
            }
        });
    }
}
