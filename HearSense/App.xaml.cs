using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using HearSense.Helpers;
using HearSense.Models;
using HearSense.Services;
using HearSense.ViewModels;
using HearSense.Views;
using Serilog;

namespace HearSense;

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
        // Déterminer le niveau de log selon l'environnement
#if DEBUG
        var logLevel = Serilog.Events.LogEventLevel.Debug;
#else
        var logLevel = Serilog.Events.LogEventLevel.Information;
#endif

        // Logging
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(logLevel)
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning) // Réduire logs Microsoft
            .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning) // Réduire logs System
            .Enrich.WithProperty("Application", "ApplAudition")
            .Enrich.WithProperty("Version", "1.568")
            .WriteTo.File(
                path: System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "ApplAudition", "logs", "app-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 10,
                fileSizeLimitBytes: 10_485_760, // 10 MB
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        Log.Information("Application démarrée - Niveau de log : {LogLevel}", logLevel);

        services.AddSingleton<ILogger>(Log.Logger);

        // Services (Singletons)
        services.AddSingleton<IAudioCaptureService, AudioCaptureService>();
        services.AddSingleton<IDspEngine, DspEngine>();
        services.AddSingleton<ILeqCalculator, LeqCalculator>();
        services.AddSingleton<AWeightingFilter>(); // Pas d'interface pour le filtre (singleton direct)
        services.AddSingleton<IExposureCategorizer, ExposureCategorizer>(); // Catégorisation avec seuils personnalisables

        // Gestionnaire d'estimation simplifié
        services.AddSingleton<IEstimationModeManager, EstimationModeManager>();

        // Phase 7 : Settings et persistance
        services.AddSingleton<ISettingsService, SettingsService>();

        // Phase 9 : Export CSV
        services.AddSingleton<IExportService, ExportService>();

        // Service de volume système (correction calculs SPL)
        services.AddSingleton<ISystemVolumeService, SystemVolumeService>();

        // Détection des changements de périphérique audio
        services.AddSingleton<AudioDeviceChangeNotifier>();

        // System Tray et auto-démarrage
        services.AddSingleton<ITrayController, TrayController>();
        services.AddSingleton<IStartupManager, StartupManager>();

        // Notifications
        services.AddSingleton<IToastNotificationService, ToastNotificationService>();
        services.AddSingleton<INotificationManager, NotificationManager>();

        // Protection automatique du volume
        services.AddSingleton<IAutoVolumeProtectionService, AutoVolumeProtectionService>();

        // Enregistrement Windows (Toast Notifications avec nom d'app correct)
        services.AddSingleton<IWindowsRegistrationService, WindowsRegistrationService>();

        // ViewModels (Transient - nouvelle instance à chaque résolution)
        services.AddTransient<MainViewModel>();
        services.AddTransient<SettingsViewModel>(); // Fenêtre de paramètres
        services.AddTransient<TrayPopupViewModel>(); // Popup tray

        // Views (Singleton - fenêtre principale unique)
        services.AddSingleton<MainWindow>();
        services.AddTransient<SettingsWindow>(); // Fenêtre de paramètres (transient pour nouvelle instance à chaque ouverture)
        services.AddTransient<TrayPopup>(); // Popup tray (transient pour nouvelle instance)
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

        // Configurer l'AppUserModelID pour les Toast Notifications Windows 10/11
        // Cela permet à Windows d'identifier correctement l'application
        AppUserModelHelper.SetAppUserModelId(Log.Logger);

        // Initialiser les services requis
        await InitializeServicesAsync();

        // Récupérer la fenêtre principale
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();

        // Configurer le TrayController
        var trayController = _serviceProvider.GetRequiredService<ITrayController>();
        trayController.Initialize(mainWindow);

        // Configurer le callback pour ouvrir les paramètres depuis le menu tray
        trayController.SetSettingsCallback(() =>
        {
            var settingsWindow = _serviceProvider.GetRequiredService<SettingsWindow>();
            settingsWindow.ShowDialog(); // Modal
        });

        // Vérifier si l'argument --minimized est présent
        var settingsService = _serviceProvider.GetRequiredService<ISettingsService>();
        bool startMinimized = e.Args.Contains("--minimized") || settingsService.Settings.StartMinimized;

        // IMPORTANT : TOUJOURS afficher la fenêtre pour initialiser le contexte de rendu WPF/LiveCharts2
        // Sans cela, le contrôle CartesianChart ne peut pas créer son contexte SkiaSharp
        mainWindow.Show();

        if (startMinimized)
        {
            // Démarrage minimisé : cacher la fenêtre immédiatement après l'initialisation
            // Délai de 100ms pour garantir que le contexte de rendu est créé
            await Task.Delay(100);
            mainWindow.Hide();
            Log.Information("Application démarrée en mode minimisé (system tray)");
        }
        else
        {
            // Démarrage normal : la fenêtre reste visible
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

            // Enregistrer l'application dans Windows pour les Toast Notifications
            // (Nécessaire pour afficher le bon nom "HearSense" au lieu de l'AUMID)
            var registrationService = _serviceProvider.GetRequiredService<IWindowsRegistrationService>();
            if (!registrationService.IsRegistered())
            {
                Log.Information("Application non enregistrée dans Windows - Enregistrement en cours...");
                await registrationService.RegisterApplicationAsync();
                Log.Information("Application enregistrée avec succès");
            }
            else
            {
                Log.Debug("Application déjà enregistrée dans Windows");
            }

            // Appliquer le thème Light uniquement
            ApplyLightTheme();

            // Initialiser le service de volume système (correction calculs SPL)
            try
            {
                var systemVolumeService = _serviceProvider.GetRequiredService<ISystemVolumeService>();
                await systemVolumeService.InitializeAsync();
                Log.Information("Service de volume système initialisé");
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Warning(ex, "Permissions insuffisantes pour accéder au volume système");
                System.Windows.MessageBox.Show(
                    "⚠️ Impossible d'accéder au volume système Windows.\n\n" +
                    "L'estimation du niveau sonore sera moins précise.\n" +
                    "Pour une meilleure précision, redémarrez l'application avec les droits administrateur.",
                    "Avertissement - Permissions Limitées",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            catch (System.Runtime.InteropServices.COMException ex) when (ex.HResult == unchecked((int)0x80070490))
            {
                Log.Error(ex, "Aucun périphérique audio détecté");
                var result = System.Windows.MessageBox.Show(
                    "❌ Aucun périphérique audio détecté.\n\n" +
                    "Branchez un casque ou des haut-parleurs et cliquez sur 'Réessayer'.\n" +
                    "Ou cliquez sur 'Annuler' pour quitter l'application.",
                    "Erreur - Aucun Périphérique Audio",
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Error);

                if (result == MessageBoxResult.Cancel)
                {
                    System.Windows.Application.Current.Shutdown(1);
                    return;
                }
            }

            // Initialiser le gestionnaire d'estimation
            var estimationModeManager = _serviceProvider.GetRequiredService<IEstimationModeManager>();
            estimationModeManager.Initialize();
            Log.Information("Gestionnaire d'estimation initialisé");
        }
        catch (FileNotFoundException ex)
        {
            Log.Fatal(ex, "Fichier de configuration critique introuvable");
            System.Windows.MessageBox.Show(
                $"❌ Fichier de configuration introuvable.\n\n" +
                $"Détails : {ex.Message}\n\n" +
                "Réinstallez l'application pour corriger ce problème.",
                "Erreur Fatale",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            System.Windows.Application.Current.Shutdown(1);
        }
        catch (TypeInitializationException ex)
        {
            Log.Fatal(ex, "Erreur d'initialisation d'un composant critique");
            System.Windows.MessageBox.Show(
                $"❌ Erreur d'initialisation d'un composant critique.\n\n" +
                $"Détails : {ex.InnerException?.Message ?? ex.Message}\n\n" +
                "Vérifiez que .NET 8 Runtime est installé.",
                "Erreur Fatale",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            System.Windows.Application.Current.Shutdown(1);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Erreur inattendue lors de l'initialisation des services");
            System.Windows.MessageBox.Show(
                $"❌ Erreur inattendue empêchant le démarrage.\n\n" +
                $"Type : {ex.GetType().Name}\n" +
                $"Message : {ex.Message}\n\n" +
                "Consultez les logs pour plus de détails.",
                "Erreur Fatale",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            System.Windows.Application.Current.Shutdown(1);
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
