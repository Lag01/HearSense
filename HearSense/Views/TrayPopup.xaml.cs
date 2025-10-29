using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using HearSense.Services;
using HearSense.ViewModels;
using Serilog;

namespace HearSense.Views;

/// <summary>
/// Popup tray miniature (style EarTrumpet).
/// </summary>
public partial class TrayPopup : Window
{
    private readonly ITrayController _trayController;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    public TrayPopupViewModel ViewModel { get; }

    public TrayPopup(TrayPopupViewModel viewModel, ITrayController trayController, IServiceProvider serviceProvider, ILogger logger)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = viewModel;
        _trayController = trayController;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Ferme le popup quand il perd le focus.
    /// </summary>
    private void OnWindowDeactivated(object sender, EventArgs e)
    {
        Close();
    }

    /// <summary>
    /// Ouvre la fenêtre principale.
    /// </summary>
    private void OnOpenMainWindow(object sender, RoutedEventArgs e)
    {
        _trayController.ShowMainWindow();
        Close();
    }

    /// <summary>
    /// Ouvre la fenêtre de paramètres.
    /// </summary>
    private void OnOpenSettings(object sender, RoutedEventArgs e)
    {
        try
        {
            _logger.Information("=== DÉBUT OnOpenSettings (clic sur bouton Paramètres depuis TrayPopup) ===");

            // Vérification du ServiceProvider
            if (_serviceProvider == null)
            {
                _logger.Fatal("ServiceProvider est NULL !");
                throw new InvalidOperationException("ServiceProvider non initialisé");
            }
            _logger.Information("ServiceProvider: OK");

            // 1. Créer la fenêtre AVANT de fermer le popup
            _logger.Information("Tentative de création de SettingsWindow via DI...");
            var settingsWindow = _serviceProvider.GetRequiredService<SettingsWindow>();
            _logger.Information("SettingsWindow créé avec succès !");

            // 2. Fermer le popup de manière ASYNCHRONE pour ne pas détruire le contexte
            _logger.Information("Programmation de la fermeture du TrayPopup (asynchrone)...");
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    Close();
                    _logger.Information("TrayPopup fermé avec succès");
                }
                catch (Exception closeEx)
                {
                    _logger.Warning(closeEx, "Erreur lors de la fermeture du TrayPopup (non critique)");
                }
            }), System.Windows.Threading.DispatcherPriority.Background);

            // 3. Afficher la fenêtre (ShowDialog est bloquant)
            _logger.Information("Affichage de SettingsWindow.ShowDialog()...");
            settingsWindow.ShowDialog();
            _logger.Information("SettingsWindow fermée par l'utilisateur - Succès complet !");
        }
        catch (Exception ex)
        {
            _logger.Fatal(ex, "💥 CRASH COMPLET lors de l'ouverture de SettingsWindow depuis TrayPopup");
            _logger.Fatal("Type d'exception: {ExceptionType}", ex.GetType().FullName);
            _logger.Fatal("Message: {Message}", ex.Message);
            _logger.Fatal("StackTrace: {StackTrace}", ex.StackTrace ?? "Aucun");

            if (ex.InnerException != null)
            {
                _logger.Fatal("InnerException: {InnerType} - {InnerMessage}",
                    ex.InnerException.GetType().FullName,
                    ex.InnerException.Message);
            }

            // Fermer le popup de manière sécurisée
            try
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    try { Close(); } catch { }
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
            catch { }

            // Afficher un message d'erreur DÉTAILLÉ à l'utilisateur
            try
            {
                System.Windows.MessageBox.Show(
                    $"❌ CRASH lors de l'ouverture des paramètres.\n\n" +
                    $"Type: {ex.GetType().Name}\n" +
                    $"Message: {ex.Message}\n\n" +
                    $"InnerException: {(ex.InnerException != null ? ex.InnerException.Message : "Aucune")}\n\n" +
                    $"Stack trace (premiers 400 caractères):\n" +
                    $"{(ex.StackTrace != null && ex.StackTrace.Length > 400 ? ex.StackTrace.Substring(0, 400) + "..." : ex.StackTrace ?? "Aucun")}\n\n" +
                    $"⚠️ Consultez les logs pour le détail complet.",
                    "Erreur critique - Paramètres",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception msgEx)
            {
                _logger.Fatal(msgEx, "Impossible d'afficher le MessageBox d'erreur !");
            }
        }
    }
}
