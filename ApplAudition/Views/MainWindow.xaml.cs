using System.ComponentModel;
using System.Windows;
using ApplAudition.Services;
using ApplAudition.ViewModels;

namespace ApplAudition.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly ITrayController _trayController;
    private readonly ISettingsService _settingsService;

    public MainWindow(MainViewModel viewModel, ITrayController trayController, ISettingsService settingsService)
    {
        InitializeComponent();
        DataContext = viewModel;
        _viewModel = viewModel;
        _trayController = trayController;
        _settingsService = settingsService;

        // Hook événement fermeture pour minimiser vers tray
        Closing += OnWindowClosing;

        // Le tray est maintenant initialisé dans App.xaml.cs au démarrage
    }

    /// <summary>
    /// Gère la fermeture de la fenêtre : minimise vers tray au lieu de quitter.
    /// </summary>
    private void OnWindowClosing(object? sender, CancelEventArgs e)
    {
        // Vérifier si l'utilisateur veut minimiser vers le tray à la fermeture
        if (_settingsService.Settings.MinimizeToTrayOnClose)
        {
            // Annuler la fermeture et masquer vers le tray
            e.Cancel = true;
            Hide();

            // Afficher notification (modération : seulement si première fois?)
            _trayController.ShowBalloonTip(
                "Appli Audition",
                "Application toujours active en arrière-plan. Double-cliquez l'icône pour restaurer.",
                timeout: 3000
            );
        }
        // Sinon : laisser la fermeture normale (e.Cancel = false par défaut)
    }
}
