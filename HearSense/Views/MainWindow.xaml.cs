using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using HearSense.Services;
using HearSense.ViewModels;

namespace HearSense.Views;

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

        // WORKAROUND pour bug LiveCharts2 : le graphique ne s'affiche pas au démarrage
        // On force un rafraîchissement après le chargement de la fenêtre
        Loaded += OnWindowLoaded;

        // Le tray est maintenant initialisé dans App.xaml.cs au démarrage
    }

    /// <summary>
    /// Gère le chargement de la fenêtre : force le rafraîchissement du graphique LiveCharts2.
    /// WORKAROUND pour bug connu : le graphique ne s'affiche pas au démarrage sans rafraîchissement forcé.
    /// </summary>
    private async void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        // Attendre que le layout WPF soit complètement chargé
        await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Loaded);
        await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render);

        // Forcer le rendu initial du graphique LiveCharts2
        if (ChartControl != null)
        {
            ChartControl.InvalidateVisual();
            ChartControl.UpdateLayout();

            // Logger uniquement en mode Debug
#if DEBUG
            Serilog.Log.Debug("Graphique LiveCharts2 forcé au rendu après chargement de la fenêtre");
#endif
        }
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
