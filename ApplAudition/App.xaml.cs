using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using ApplAudition.Models;
using ApplAudition.Services;
using ApplAudition.ViewModels;
using ApplAudition.Views;
using Serilog;

namespace ApplAudition;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    private ServiceProvider? _serviceProvider;

    /// <summary>
    /// Configure les services et la dependency injection.
    /// </summary>
    private void ConfigureServices(IServiceCollection services)
    {
        // Logging
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                path: System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "ApplAudition", "logs", "app-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 10,
                fileSizeLimitBytes: 10_485_760) // 10 MB
            .CreateLogger();

        services.AddSingleton<ILogger>(Log.Logger);

        // Services (Singletons)
        services.AddSingleton<IAudioCaptureService, AudioCaptureService>();
        services.AddSingleton<IDspEngine, DspEngine>();
        services.AddSingleton<ILeqCalculator, LeqCalculator>();
        services.AddSingleton<AWeightingFilter>(); // Pas d'interface pour le filtre (singleton direct)
        services.AddSingleton<IExposureCategorizer, ExposureCategorizer>(); // Phase 3 : Catégorisation avec biais conservateur

        // Phase 4 : Système de profils heuristiques
        services.AddSingleton<IAudioDeviceService, AudioDeviceService>();
        services.AddSingleton<ProfileDatabase>();
        services.AddSingleton<IProfileMatcher, ProfileMatcher>();

        // Phase 5 : Mode B - Auto-profil Heuristique
        services.AddSingleton<IEstimationModeManager, EstimationModeManager>();

        // Phase 7 : Settings et persistance
        services.AddSingleton<ISettingsService, SettingsService>();

        // Phase 9 : Export CSV
        services.AddSingleton<IExportService, ExportService>();

        // Service de volume système (correction calculs SPL)
        services.AddSingleton<ISystemVolumeService, SystemVolumeService>();

        // System Tray et auto-démarrage
        services.AddSingleton<ITrayController, TrayController>();
        services.AddSingleton<IStartupManager, StartupManager>();

        // ViewModels (Transient - nouvelle instance à chaque résolution)
        services.AddTransient<MainViewModel>();
        services.AddTransient<CalibrationViewModel>(); // Phase 8 : Calibration

        // Views (Singleton - fenêtre principale unique)
        services.AddSingleton<MainWindow>();
    }

    /// <summary>
    /// Point d'entrée de l'application.
    /// </summary>
    private async void OnStartup(object sender, StartupEventArgs e)
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        Log.Information("Application ApplAudition démarrée");

        // Initialiser les services requis
        await InitializeServicesAsync();

        // Récupérer la fenêtre principale
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();

        // Vérifier si l'argument --minimized est présent
        var settingsService = _serviceProvider.GetRequiredService<ISettingsService>();
        bool startMinimized = e.Args.Contains("--minimized") || settingsService.Settings.StartMinimized;

        if (startMinimized)
        {
            // Démarrage minimisé : initialiser le tray sans afficher la fenêtre
            var trayController = _serviceProvider.GetRequiredService<ITrayController>();
            trayController.Initialize(mainWindow);

            Log.Information("Application démarrée en mode minimisé (system tray)");
            // Ne pas appeler mainWindow.Show()
        }
        else
        {
            // Démarrage normal : afficher la fenêtre
            mainWindow.Show();
            Log.Information("Application démarrée en mode normal (fenêtre visible)");
        }
    }

    /// <summary>
    /// Initialise les services nécessaires au démarrage.
    /// </summary>
    private async Task InitializeServicesAsync()
    {
        try
        {
            // Charger les settings (Phase 7)
            var settingsService = _serviceProvider.GetRequiredService<ISettingsService>();
            await settingsService.LoadAsync();
            Log.Information("Settings chargés");

            // Appliquer le thème Light uniquement
            ApplyLightTheme();

            // Charger les profils heuristiques
            var profileDatabase = _serviceProvider.GetRequiredService<ProfileDatabase>();
            await profileDatabase.LoadProfilesAsync();
            Log.Information("Profils heuristiques chargés");

            // Initialiser la détection de périphérique audio
            var audioDeviceService = _serviceProvider.GetRequiredService<IAudioDeviceService>();
            await audioDeviceService.InitializeAsync();
            Log.Information("Détection de périphérique audio initialisée");

            // Initialiser le service de volume système (correction calculs SPL)
            var systemVolumeService = _serviceProvider.GetRequiredService<ISystemVolumeService>();
            await systemVolumeService.InitializeAsync();
            Log.Information("Service de volume système initialisé");

            // Initialiser le gestionnaire de mode d'estimation (Phase 5)
            var estimationModeManager = _serviceProvider.GetRequiredService<IEstimationModeManager>();
            estimationModeManager.Initialize();
            Log.Information("Gestionnaire de mode d'estimation initialisé");

            // Appliquer ForceModeA depuis les settings (Phase 7)
            if (settingsService.Settings.ForceModeA)
            {
                estimationModeManager.SetForceModeA(true);
                Log.Information("Mode A forcé depuis les settings");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erreur lors de l'initialisation des services");
            // L'application peut continuer en mode dégradé (Mode A uniquement)
        }
    }

    /// <summary>
    /// Applique le thème Light uniquement.
    /// </summary>
    private void ApplyLightTheme()
    {
        try
        {
            var themeDict = new ResourceDictionary
            {
                Source = new Uri("/Resources/Themes/Light.xaml", UriKind.Relative)
            };

            Resources.MergedDictionaries.Add(themeDict);

            Log.Information("Thème Light appliqué");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erreur lors de l'application du thème Light");
        }
    }

    /// <summary>
    /// Nettoyage à la fermeture de l'application.
    /// </summary>
    private void OnExit(object sender, ExitEventArgs e)
    {
        Log.Information("Application ApplAudition fermée");

        // Disposer le TrayController avant les autres services (évite icône fantôme)
        try
        {
            var trayController = _serviceProvider?.GetService<ITrayController>();
            trayController?.Dispose();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Erreur lors du dispose du TrayController");
        }

        // Disposer les services
        _serviceProvider?.Dispose();
        Log.CloseAndFlush();
    }
}
