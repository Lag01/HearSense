using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using ApplAudition.Models;
using Serilog;
using Application = System.Windows.Application;

namespace ApplAudition.Services;

/// <summary>
/// Contrôleur du system tray (zone de notification Windows).
/// Gère l'icône NotifyIcon, le menu contextuel et les interactions.
/// </summary>
public class TrayController : ITrayController
{
    private readonly ILogger _logger;
    private NotifyIcon? _notifyIcon;
    private Window? _mainWindow;
    private bool _isDisposed;

    public bool IsVisible => _notifyIcon?.Visible ?? false;

    public TrayController(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Initialise le contrôleur tray avec la fenêtre principale.
    /// </summary>
    public void Initialize(Window mainWindow)
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(TrayController));

        if (_notifyIcon != null)
        {
            _logger.Warning("TrayController déjà initialisé");
            return;
        }

        _mainWindow = mainWindow;

        // Créer l'icône NotifyIcon
        _notifyIcon = new NotifyIcon
        {
            // Utiliser icône système par défaut (temporaire)
            // TODO: Remplacer par icône custom Resources/Icons/tray-icon.ico
            Icon = SystemIcons.Application,
            Text = "Appli Audition",
            Visible = true
        };

        // Menu contextuel
        var contextMenu = new ContextMenuStrip();

        var showMenuItem = new ToolStripMenuItem("Afficher");
        showMenuItem.Click += (s, e) => ShowMainWindow();
        showMenuItem.Font = new Font(showMenuItem.Font, System.Drawing.FontStyle.Bold);
        contextMenu.Items.Add(showMenuItem);

        contextMenu.Items.Add(new ToolStripSeparator());

        var quitMenuItem = new ToolStripMenuItem("Quitter");
        quitMenuItem.Click += (s, e) => QuitApplication();
        contextMenu.Items.Add(quitMenuItem);

        _notifyIcon.ContextMenuStrip = contextMenu;

        // Double-clic pour restaurer fenêtre
        _notifyIcon.DoubleClick += (s, e) => ShowMainWindow();

        _logger.Information("TrayController initialisé");
    }

    /// <summary>
    /// Met à jour le tooltip avec le niveau dB actuel.
    /// </summary>
    public void UpdateTooltip(float currentDbA, ExposureCategory category)
    {
        if (_notifyIcon == null || _isDisposed)
            return;

        // Formater tooltip (max 63 caractères sur Windows)
        string categoryText = category switch
        {
            ExposureCategory.Safe => "Sûr",
            ExposureCategory.Moderate => "Modéré",
            ExposureCategory.Hazardous => "Dangereux",
            _ => "Inconnu"
        };

        string tooltip = $"Appli Audition - {currentDbA:F0} dB(A) ({categoryText})";

        // Tronquer si nécessaire (limite Windows)
        if (tooltip.Length > 63)
            tooltip = tooltip.Substring(0, 60) + "...";

        _notifyIcon.Text = tooltip;
    }

    /// <summary>
    /// Affiche une notification balloon tip.
    /// </summary>
    public void ShowBalloonTip(string title, string text, int timeout = 3000)
    {
        if (_notifyIcon == null || _isDisposed)
            return;

        try
        {
            _notifyIcon.ShowBalloonTip(timeout, title, text, ToolTipIcon.Info);
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Erreur lors de l'affichage du balloon tip");
        }
    }

    /// <summary>
    /// Affiche la fenêtre principale (restaure depuis tray).
    /// </summary>
    public void ShowMainWindow()
    {
        if (_mainWindow == null)
            return;

        // Dispatcher pour thread-safety (WPF UI thread)
        _mainWindow.Dispatcher.Invoke(() =>
        {
            _mainWindow.Show();
            _mainWindow.WindowState = WindowState.Normal;
            _mainWindow.Activate();
            _logger.Debug("Fenêtre principale restaurée depuis tray");
        });
    }

    /// <summary>
    /// Masque la fenêtre principale (minimise vers tray).
    /// </summary>
    public void HideMainWindow()
    {
        if (_mainWindow == null)
            return;

        _mainWindow.Dispatcher.Invoke(() =>
        {
            _mainWindow.Hide();
            _logger.Debug("Fenêtre principale masquée vers tray");
        });
    }

    /// <summary>
    /// Quitte l'application complètement.
    /// </summary>
    private void QuitApplication()
    {
        _logger.Information("Fermeture de l'application depuis le menu tray");

        // Disposer le NotifyIcon avant de quitter (sinon icône fantôme)
        Dispose();

        // Fermer l'application WPF
        Application.Current.Dispatcher.Invoke(() =>
        {
            Application.Current.Shutdown();
        });
    }

    /// <summary>
    /// Libère les ressources non managées (NotifyIcon).
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed)
            return;

        _logger.Debug("Dispose TrayController");

        if (_notifyIcon != null)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _notifyIcon = null;
        }

        _isDisposed = true;
    }
}
