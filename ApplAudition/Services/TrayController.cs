using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using ApplAudition.Constants;
using ApplAudition.Models;
using ApplAudition.Views;
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
    private Action? _settingsCallback;
    private TrayPopup? _trayPopup;
    private float _latestDbA;
    private ExposureCategory _latestCategory;

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
            Icon = LoadAppIcon(), // Charger icône personnalisée depuis Resources
            Text = "Appli Audition",
            Visible = true
        };

        // Menu contextuel
        var contextMenu = new ContextMenuStrip();

        var showMenuItem = new ToolStripMenuItem("Afficher");
        showMenuItem.Click += (s, e) => ShowMainWindow();
        showMenuItem.Font = new Font(showMenuItem.Font, System.Drawing.FontStyle.Bold);
        contextMenu.Items.Add(showMenuItem);

        var settingsMenuItem = new ToolStripMenuItem("Paramètres");
        settingsMenuItem.Click += (s, e) => OpenSettings();
        contextMenu.Items.Add(settingsMenuItem);

        contextMenu.Items.Add(new ToolStripSeparator());

        var quitMenuItem = new ToolStripMenuItem("Quitter");
        quitMenuItem.Click += (s, e) => QuitApplication();
        contextMenu.Items.Add(quitMenuItem);

        _notifyIcon.ContextMenuStrip = contextMenu;

        // Clic gauche pour afficher popup
        _notifyIcon.Click += (s, e) =>
        {
            var mouseEvent = e as MouseEventArgs;
            if (mouseEvent?.Button == MouseButtons.Left)
            {
                ShowPopup();
            }
        };

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

        // Sauvegarder les valeurs pour le popup
        _latestDbA = currentDbA;
        _latestCategory = category;

        // Formater tooltip (max 63 caractères sur Windows)
        string categoryText = category switch
        {
            ExposureCategory.Safe => "Sûr",
            ExposureCategory.Moderate => "Modéré",
            ExposureCategory.Hazardous => "Dangereux",
            _ => "Inconnu"
        };

        string tooltip = $"Appli Audition - {currentDbA:F0} dB(A) ({categoryText})";

        // Windows limite les tooltips NotifyIcon à 63 caractères (64 avec null terminator)
        // Tronquer proprement si dépassement
        if (tooltip.Length > AppConstants.MAX_TOOLTIP_LENGTH)
        {
            tooltip = tooltip.Substring(0, AppConstants.MAX_TOOLTIP_LENGTH - 3) + "...";
            _logger.Debug("Tooltip tronqué pour respecter la limite Windows de {MaxLength} caractères", AppConstants.MAX_TOOLTIP_LENGTH);
        }

        _notifyIcon.Text = tooltip;

        // Mettre à jour le popup s'il est affiché
        if (_trayPopup != null && _trayPopup.IsVisible)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _trayPopup.ViewModel.UpdateValues(currentDbA, category);
            });
        }
    }

    /// <summary>
    /// Affiche une notification balloon tip (méthode héritée).
    /// Note : Les notifications critiques utilisent maintenant ToastNotificationService.
    /// </summary>
    public void ShowBalloonTip(string title, string text, int timeout = 3000)
    {
        if (_notifyIcon == null || _isDisposed)
            return;

        try
        {
            _notifyIcon.ShowBalloonTip(timeout, title, text, ToolTipIcon.None);
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
    /// Définit le callback appelé lorsque l'utilisateur clique sur "Paramètres".
    /// </summary>
    public void SetSettingsCallback(Action callback)
    {
        _settingsCallback = callback;
        _logger.Debug("Callback des paramètres défini");
    }

    /// <summary>
    /// Ouvre la fenêtre de paramètres via le callback.
    /// </summary>
    private void OpenSettings()
    {
        if (_settingsCallback != null)
        {
            _logger.Debug("Ouverture des paramètres depuis le menu tray");
            Application.Current.Dispatcher.Invoke(_settingsCallback);
        }
        else
        {
            _logger.Warning("Callback des paramètres non défini");
        }
    }

    /// <summary>
    /// Affiche le popup tray avec la jauge dB miniature (style EarTrumpet).
    /// </summary>
    public void ShowPopup()
    {
        try
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Fermer popup existant s'il y en a un
                HidePopup();

                // Créer nouveau popup via DI
                var app = Application.Current as App;
                if (app == null) return;

                var serviceProvider = app.GetType().GetField("_serviceProvider",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.GetValue(app) as IServiceProvider;

                if (serviceProvider == null)
                {
                    _logger.Warning("ServiceProvider introuvable pour créer TrayPopup");
                    return;
                }

                _trayPopup = serviceProvider.GetService(typeof(TrayPopup)) as TrayPopup;
                if (_trayPopup == null)
                {
                    _logger.Warning("Impossible de créer TrayPopup via DI");
                    return;
                }

                // Mettre à jour les valeurs
                _trayPopup.ViewModel.UpdateValues(_latestDbA, _latestCategory);

                // Calculer la position près de l'icône tray
                var position = GetTrayIconPosition();
                _trayPopup.Left = position.X;
                _trayPopup.Top = position.Y;

                _trayPopup.Show();
                _trayPopup.Activate();

                _logger.Debug("Popup tray affiché");
            });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Erreur lors de l'affichage du popup tray");
        }
    }

    /// <summary>
    /// Masque le popup tray s'il est affiché.
    /// </summary>
    public void HidePopup()
    {
        try
        {
            if (_trayPopup != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _trayPopup.Close();
                    _trayPopup = null;
                });

                _logger.Debug("Popup tray masqué");
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Erreur lors de la fermeture du popup tray");
        }
    }

    /// <summary>
    /// Calcule la position du popup près de l'icône tray.
    /// Prend en compte le DPI scaling pour un positionnement correct.
    /// </summary>
    private System.Windows.Point GetTrayIconPosition()
    {
        // Obtenir la position de la souris (approximation de l'icône tray)
        var cursorPosition = System.Windows.Forms.Cursor.Position;

        // Obtenir l'écran où se trouve le curseur
        var screen = System.Windows.Forms.Screen.FromPoint(cursorPosition);
        var workingArea = screen.WorkingArea;

        // Obtenir le facteur de DPI scaling
        // SystemParameters renvoie des valeurs en DIU (Device Independent Units)
        // Screen renvoie des valeurs en pixels physiques
        var dpiScaleX = SystemParameters.PrimaryScreenWidth / System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
        var dpiScaleY = SystemParameters.PrimaryScreenHeight / System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;

        // Dimensions du popup (en DIU - correspondent aux valeurs XAML)
        const double popupWidth = 280;
        const double popupHeight = 280;
        const double margin = 10; // Marge depuis les bords de l'écran

        // Convertir les coordonnées physiques en DIU
        double cursorX = cursorPosition.X * dpiScaleX;
        double cursorY = cursorPosition.Y * dpiScaleY;
        double workingLeft = workingArea.Left * dpiScaleX;
        double workingTop = workingArea.Top * dpiScaleY;
        double workingRight = workingArea.Right * dpiScaleX;
        double workingBottom = workingArea.Bottom * dpiScaleY;

        // Calculer position initiale (centrée horizontalement sur le curseur, au-dessus de la barre des tâches)
        double x = cursorX - (popupWidth / 2);
        double y = workingBottom - popupHeight - margin;

        // Ajuster X si dépasse le bord gauche
        if (x < workingLeft + margin)
        {
            x = workingLeft + margin;
        }

        // Ajuster X si dépasse le bord droit
        if (x + popupWidth > workingRight - margin)
        {
            x = workingRight - popupWidth - margin;
        }

        // Ajuster Y si dépasse le haut de l'écran (cas rare)
        if (y < workingTop + margin)
        {
            y = workingTop + margin;
        }

        // Ajuster Y si dépasse le bas de l'écran
        if (y + popupHeight > workingBottom - margin)
        {
            y = workingBottom - popupHeight - margin;
        }

        _logger.Debug("Position du popup TrayPopup calculée : X={X:F0}, Y={Y:F0} DIU (Cursor: {CursorX}px, {CursorY}px, DPI Scale: {DpiX:F2}x{DpiY:F2})",
            x, y, cursorPosition.X, cursorPosition.Y, dpiScaleX, dpiScaleY);

        return new System.Windows.Point(x, y);
    }

    /// <summary>
    /// Quitte l'application complètement.
    /// </summary>
    private void QuitApplication()
    {
        _logger.Information("Fermeture de l'application depuis le menu tray");

        // Fermer le popup s'il est ouvert
        HidePopup();

        // Disposer le NotifyIcon avant de quitter (sinon icône fantôme)
        Dispose();

        // Fermer l'application WPF
        Application.Current.Dispatcher.Invoke(() =>
        {
            Application.Current.Shutdown();
        });
    }

    /// <summary>
    /// Charge l'icône de l'application depuis les ressources.
    /// </summary>
    private Icon LoadAppIcon()
    {
        try
        {
            // Chemin vers l'icône dans le projet
            string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "icon.ico");

            if (File.Exists(iconPath))
            {
                _logger.Debug("Icône trouvée : {IconPath}", iconPath);
                return new Icon(iconPath);
            }
            else
            {
                _logger.Warning("Icône introuvable à {IconPath}, utilisation de l'icône système", iconPath);
                return SystemIcons.Application;
            }
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Erreur lors du chargement de l'icône personnalisée, utilisation de l'icône système");
            return SystemIcons.Application;
        }
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
