using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using ApplAudition.Services;
using ApplAudition.ViewModels;

namespace ApplAudition.Views;

/// <summary>
/// Popup tray miniature (style EarTrumpet).
/// </summary>
public partial class TrayPopup : Window
{
    private readonly ITrayController _trayController;
    private readonly IServiceProvider _serviceProvider;

    public TrayPopupViewModel ViewModel { get; }

    public TrayPopup(TrayPopupViewModel viewModel, ITrayController trayController, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = viewModel;
        _trayController = trayController;
        _serviceProvider = serviceProvider;
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
        var settingsWindow = _serviceProvider.GetRequiredService<SettingsWindow>();
        settingsWindow.ShowDialog();
        Close();
    }
}
