using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HearSense.Models;
using HearSense.Services;
using Serilog;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.Defaults;

namespace HearSense.ViewModels;

/// <summary>
/// ViewModel principal de l'application.
/// Gère l'affichage des niveaux sonores et la capture audio.
/// </summary>
public partial class MainViewModel : BaseViewModel
{
    private readonly IAudioCaptureService _audioCaptureService;
    private readonly IDspEngine _dspEngine;
    private readonly AWeightingFilter _aWeightingFilter;
    private readonly ILeqCalculator _leqCalculator;
    private readonly IExposureCategorizer _exposureCategorizer;
    private readonly IEstimationModeManager _estimationModeManager;
    private readonly ISettingsService _settingsService;
    private readonly IExportService _exportService;
    private readonly INotificationManager _notificationManager;
    private readonly IAutoVolumeProtectionService _autoVolumeProtectionService;
    private readonly ITrayController _trayController;
    private readonly IServiceProvider _serviceProvider;
    private readonly AudioDeviceChangeNotifier _deviceChangeNotifier;
    private readonly ILogger _logger;


    #region Propriétés observables

    [ObservableProperty]
    private float _currentDbA;

    [ObservableProperty]
    private float _currentDbfs;

    [ObservableProperty]
    private float _leq1Min;

    [ObservableProperty]
    private float _peak;

    [ObservableProperty]
    private ExposureCategory _exposureCategory = ExposureCategory.Safe;

    [ObservableProperty]
    private string _statusMessage = "Prêt à démarrer";

    [ObservableProperty]
    private bool _isCapturing;


    /// <summary>
    /// Collection observable pour l'historique du graphe (Phase 6 - Tâche 15).
    /// Buffer circulaire de ~1440 points pour 3 minutes à 125ms par échantillon.
    /// </summary>
    public ObservableCollection<DataPoint> HistoryData { get; } = new();

    /// <summary>
    /// Collection observable pour les points LiveCharts2.
    /// </summary>
    private ObservableCollection<ObservablePoint> _chartValues = new();

    /// <summary>
    /// Collection observable pour l'export CSV (Phase 9 - Tâche 21).
    /// Contient toutes les données complètes (dBFS, dB(A), Leq, Peak, Mode, Profile).
    /// </summary>
    public ObservableCollection<ExportDataPoint> ExportHistoryData { get; } = new();

    /// <summary>
    /// Série LiveCharts2 pour le graphe dB(A) (Phase 6 - Tâche 15).
    /// </summary>
    [ObservableProperty]
    private ISeries[] _series = null!;

    /// <summary>
    /// Axes X et Y pour le graphe (Phase 6 - Tâche 15).
    /// </summary>
    [ObservableProperty]
    private Axis[] _xAxes = null!;

    [ObservableProperty]
    private Axis[] _yAxes = null!;

    [ObservableProperty]
    private bool _isSpeakerDetected = false;

    [ObservableProperty]
    private string _speakerWarningMessage = "";

    #endregion

    private readonly DispatcherTimer _uiRefreshTimer;
    private const int MAX_HISTORY_POINTS = 1440; // 3 minutes * 60 s * 8 échantillons/s (125ms)
    private DateTime _captureStartTime = DateTime.MinValue;

    // Lissage de l'affichage dB(A) (moyenne glissante + throttling)
    private readonly Queue<float> _smoothingBuffer = new Queue<float>(4);
    private const int SMOOTHING_WINDOW_SIZE = 4; // 4 × 125ms = 500ms de lissage
    private int _displayThrottleCounter = 0;
    private const int DISPLAY_THROTTLE_INTERVAL = 3; // Afficher tous les 3 buffers (375ms)

    // Throttling spécifique pour le graphique LiveCharts2
    private int _chartThrottleCounter = 0;
    private const int CHART_THROTTLE_INTERVAL = 4; // Ajouter un point tous les 4 buffers (500ms)

    public MainViewModel(
        IAudioCaptureService audioCaptureService,
        IDspEngine dspEngine,
        AWeightingFilter aWeightingFilter,
        ILeqCalculator leqCalculator,
        IExposureCategorizer exposureCategorizer,
        IEstimationModeManager estimationModeManager,
        ISettingsService settingsService,
        IExportService exportService,
        INotificationManager notificationManager,
        IAutoVolumeProtectionService autoVolumeProtectionService,
        ITrayController trayController,
        AudioDeviceChangeNotifier deviceChangeNotifier,
        IServiceProvider serviceProvider,
        ILogger logger)
    {
        _audioCaptureService = audioCaptureService;
        _dspEngine = dspEngine;
        _aWeightingFilter = aWeightingFilter;
        _leqCalculator = leqCalculator;
        _exposureCategorizer = exposureCategorizer;
        _estimationModeManager = estimationModeManager;
        _settingsService = settingsService;
        _exportService = exportService;
        _notificationManager = notificationManager;
        _autoVolumeProtectionService = autoVolumeProtectionService;
        _trayController = trayController;
        _deviceChangeNotifier = deviceChangeNotifier;
        _serviceProvider = serviceProvider;
        _logger = logger;

        // Souscrire aux événements du service de capture
        _audioCaptureService.DataAvailable += OnAudioDataAvailable;
        _audioCaptureService.ErrorOccurred += OnErrorOccurred;

        // Souscrire aux changements de périphérique audio
        _deviceChangeNotifier.DefaultDeviceChanged += OnDefaultDeviceChanged;
        _deviceChangeNotifier.Start();

        // Timer UI pour refresh fluide (30 Hz = ~33ms) - Phase 6 Tâche 13
        _uiRefreshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(33)
        };
        _uiRefreshTimer.Tick += OnUiRefreshTick;

        // Initialiser le graphe LiveCharts2 (Phase 6 - Tâche 15)
        InitializeChart();

        // Activer la synchronisation thread-safe pour la collection du graphe
        BindingOperations.EnableCollectionSynchronization(_chartValues, new object());

        _logger.Information("MainViewModel initialisé avec synchronisation thread-safe");

        // DÉMARRAGE AUTOMATIQUE : Lancer la capture audio dès que possible
        // On utilise Dispatcher.BeginInvoke pour éviter de bloquer le constructeur
        App.Current.Dispatcher.BeginInvoke(new Action(async () =>
        {
            await InitializeAndStartCaptureAsync();
        }), System.Windows.Threading.DispatcherPriority.Loaded);
    }

    /// <summary>
    /// Initialise et démarre la capture audio automatiquement.
    /// Appelé automatiquement au démarrage de l'application.
    /// </summary>
    private async Task InitializeAndStartCaptureAsync()
    {
        try
        {
            _logger.Information("Démarrage automatique de la capture audio");

            await _audioCaptureService.StartAsync();
            IsCapturing = true;

            // Initialiser le timestamp de début
            _captureStartTime = DateTime.Now;
            HistoryData.Clear();
            ExportHistoryData.Clear();
            _chartValues.Clear(); // Vider le graphe LiveCharts2

            _uiRefreshTimer.Start();

            _logger.Information("Capture audio démarrée automatiquement");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Erreur lors du démarrage automatique de la capture");
        }
    }

    /// <summary>
    /// Initialise le graphe LiveCharts2 avec les séries et axes (Phase 6 - Tâche 15).
    /// </summary>
    private void InitializeChart()
    {
        // Initialiser la collection des valeurs du graphe
        _chartValues = new ObservableCollection<ObservablePoint>();

        // Configuration de la série LineSeries pour dB(A)
        Series = new ISeries[]
        {
            new LineSeries<ObservablePoint>
            {
                Name = "dB(A)",
                Values = _chartValues, // Utiliser la collection observable persistante
                Stroke = new SolidColorPaint(SkiaSharp.SKColors.DodgerBlue, 2),
                Fill = null, // Pas de remplissage sous la courbe
                GeometrySize = 0, // Pas de points visibles (ligne continue)
                LineSmoothness = 0.2, // Légère courbe pour un rendu plus fluide
                GeometryStroke = null,
                GeometryFill = null
            }
        };

        // Axe X : Temps (secondes)
        XAxes = new Axis[]
        {
            new Axis
            {
                Name = "Temps (s)",
                NameTextSize = 12,
                TextSize = 10,
                MinLimit = 0,
                MaxLimit = 180, // 3 minutes = 180 secondes
                ForceStepToMin = false,
                MinStep = 10
            }
        };

        // Axe Y : dB(A)
        YAxes = new Axis[]
        {
            new Axis
            {
                Name = "dB(A)",
                NameTextSize = 12,
                TextSize = 10,
                MinLimit = 0,
                MaxLimit = 120,
                ForceStepToMin = false
            }
        };

        _logger.Information("Graphe LiveCharts2 initialisé avec succès");
    }

    #region Commandes

    /// <summary>
    /// Commande pour exporter l'historique vers CSV (Phase 9 - Tâche 21).
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExportCsv))]
    private async Task ExportCsvAsync()
    {
        try
        {
            // Créer le SaveFileDialog
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Fichiers CSV (*.csv)|*.csv|Tous les fichiers (*.*)|*.*",
                DefaultExt = ".csv",
                FileName = $"export_audition_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
                Title = "Exporter l'historique vers CSV"
            };

            // Afficher le dialog
            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                // Exporter vers CSV
                bool success = await _exportService.ExportToCsvAsync(ExportHistoryData, dialog.FileName);

                if (success)
                {
                    _logger.Information("Export CSV réussi : {Count} lignes", ExportHistoryData.Count);
                }
                else
                {
                    _logger.Warning("L'export CSV a échoué");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Erreur lors de l'export CSV");
        }
    }

    /// <summary>
    /// Indique si l'export CSV est possible (au moins un point de données).
    /// </summary>
    private bool CanExportCsv() => ExportHistoryData.Count > 0;

    /// <summary>
    /// Commande pour ouvrir la fenêtre de paramètres.
    /// </summary>
    [RelayCommand]
    private void OpenSettings()
    {
        try
        {
            var settingsWindow = _serviceProvider.GetService(typeof(Views.SettingsWindow)) as Views.SettingsWindow;
            if (settingsWindow != null)
            {
                settingsWindow.ShowDialog();
                _logger.Information("Fenêtre de paramètres ouverte depuis MainWindow");
            }
            else
            {
                _logger.Warning("Impossible de créer SettingsWindow via DI");
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Erreur lors de l'ouverture de la fenêtre de paramètres");
        }
    }

    /// <summary>
    /// Commande pour ouvrir la fenêtre "À propos".
    /// </summary>
    [RelayCommand]
    private void OpenAbout()
    {
        try
        {
            var aboutWindow = new Views.AboutWindow
            {
                Owner = System.Windows.Application.Current.MainWindow
            };
            aboutWindow.ShowDialog();
            _logger.Information("Fenêtre À propos ouverte depuis MainWindow");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Erreur lors de l'ouverture de la fenêtre À propos");
        }
    }

    #endregion

    #region Méthodes privées

    /// <summary>
    /// Calcule une valeur dB(A) lissée via moyenne glissante.
    /// Réduit les variations brutales dues aux micro-silences dans la musique.
    /// </summary>
    /// <param name="newValue">Nouvelle valeur dB(A) à ajouter au buffer</param>
    /// <returns>Moyenne des N dernières valeurs (fenêtre de lissage)</returns>
    private float CalculateSmoothDbA(float newValue)
    {
        // Ajouter nouvelle valeur au buffer
        _smoothingBuffer.Enqueue(newValue);

        // Supprimer les anciennes valeurs si buffer plein
        while (_smoothingBuffer.Count > SMOOTHING_WINDOW_SIZE)
        {
            _smoothingBuffer.Dequeue();
        }

        // Retourner la moyenne des valeurs dans le buffer
        return _smoothingBuffer.Average();
    }

    #endregion

    #region Gestionnaires d'événements

    private void OnAudioDataAvailable(object? sender, AudioDataEventArgs e)
    {
        try
        {
            // Créer une copie du buffer pour ne pas modifier l'original
            float[] buffer = (float[])e.Buffer.Clone();

            // ÉTAPE 1 : Appliquer le filtre de pondération A
            // Cela modifie le buffer en place pour simuler la sensibilité de l'oreille humaine
            _aWeightingFilter.ApplyFilter(buffer);

            // ÉTAPE 2 : Calculer RMS et dBFS sur le signal pondéré
            var dspResult = _dspEngine.ProcessBuffer(buffer);

            // ÉTAPE 3 : Ajouter l'échantillon au calculateur Leq
            _leqCalculator.AddSample(dspResult.DbFs);

            // ÉTAPE 4 : Récupérer Leq et Pic
            float leq = _leqCalculator.GetLeq();
            float peak = _leqCalculator.GetPeak();

            // ÉTAPE 5 : Estimer le SPL selon le mode actif (Phase 5)
            float estimatedSpl = _estimationModeManager.EstimateSpl(dspResult.DbFs);

            // ÉTAPE 5.5 : Calculer le SPL lissé via moyenne glissante
            float smoothedSpl = CalculateSmoothDbA(estimatedSpl);

            // ÉTAPE 6 : Catégoriser l'exposition basée sur le SPL lissé (pas sur dBFS)
            // Utiliser les 3 seuils personnalisables depuis les settings
            var category = _exposureCategorizer.CategorizeExposure(
                smoothedSpl,
                _settingsService.Settings.ThresholdWarning,
                _settingsService.Settings.ThresholdDanger,
                _settingsService.Settings.ThresholdCritical);

            // ÉTAPE 6.5 : Vérifier le seuil critique et notifier si nécessaire
            _notificationManager.CheckThreshold(smoothedSpl);

            // ÉTAPE 6.6 : Protection automatique du volume
            _autoVolumeProtectionService.MonitorAndProtect(smoothedSpl);

            // Incrémenter le compteur de throttling
            _displayThrottleCounter++;

            // ÉTAPE 7 : Mettre à jour les propriétés UI avec throttling (thread-safe via dispatcher)
            // Afficher seulement tous les N buffers (ex: 3 × 125ms = 375ms)
            if (_displayThrottleCounter % DISPLAY_THROTTLE_INTERVAL == 0)
            {
                // Utiliser BeginInvoke (asynchrone, non-bloquant) au lieu de Invoke
                App.Current.Dispatcher.BeginInvoke(() =>
                {
                    CurrentDbfs = dspResult.DbFs;
                    CurrentDbA = smoothedSpl; // Valeur lissée pour affichage confortable
                    Leq1Min = leq;
                    Peak = peak;
                    ExposureCategory = category; // Safe/Moderate/Hazardous basé sur SPL lissé

                    // Mettre à jour le tooltip du tray en temps réel
                    _trayController.UpdateTooltip(smoothedSpl, category);
                }, DispatcherPriority.Background); // Priorité basse pour ne pas bloquer UI
            }

            // ÉTAPE 8 : Ajouter point au graphe historique (Phase 6) + export CSV (Phase 9)
            // Throttling : ajouter un point au graphe tous les 500ms (4 buffers) pour meilleures performances
            _chartThrottleCounter++;
            if (_chartThrottleCounter % CHART_THROTTLE_INTERVAL == 0)
            {
                // Utiliser BeginInvoke (asynchrone, non-bloquant) au lieu de Invoke
                App.Current.Dispatcher.BeginInvoke(() =>
                {
                    AddDataPoint(smoothedSpl, dspResult.DbFs, leq, peak);
                }, DispatcherPriority.Background); // Priorité basse pour ne pas bloquer UI
            }

            // Log périodique (toutes les 2 secondes ≈ 16 buffers)
            if (_leqCalculator.GetSampleCount() % 16 == 0)
            {
                _logger.Debug("DSP - dBFS: {DbFs:F1}, Leq: {Leq:F1}, Pic: {Peak:F1}",
                    dspResult.DbFs, leq, peak);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Erreur lors du traitement DSP du buffer audio");
        }
    }

    private void OnErrorOccurred(object? sender, ErrorEventArgs e)
    {
        _logger.Error(e.Exception, "Erreur de capture audio");
        IsCapturing = false;
    }

    /// <summary>
    /// Gestionnaire de l'événement de changement de périphérique audio par défaut.
    /// Redémarre automatiquement la capture avec le nouveau périphérique.
    /// </summary>
    private async void OnDefaultDeviceChanged(object? sender, DefaultDeviceChangedEventArgs e)
    {
        try
        {
            _logger.Information("Changement de périphérique audio détecté : {DeviceName}", e.DeviceName);

            // Afficher notification tray
            _trayController.ShowBalloonTip(
                "Périphérique audio changé",
                $"Redémarrage de la capture avec : {e.DeviceName}",
                timeout: 3000
            );

            // Redémarrer la capture avec le nouveau périphérique
            await _audioCaptureService.RestartAsync();

            _logger.Information("Capture redémarrée avec succès après changement de périphérique");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Erreur lors du changement de périphérique audio");

            _trayController.ShowBalloonTip(
                "Erreur",
                "Impossible de redémarrer la capture. Redémarrez l'application.",
                timeout: 5000
            );
        }
    }

    /// <summary>
    /// Handler du timer UI pour refresh fluide (Phase 6 - Tâche 13).
    /// Appelé toutes les ~33ms (30 Hz).
    /// </summary>
    private void OnUiRefreshTick(object? sender, EventArgs e)
    {
        // Le timer sert principalement à garantir un refresh UI fluide.
        // Les valeurs sont mises à jour dans OnAudioDataAvailable (thread audio).
        // Ici on pourrait ajouter des animations ou interpolations si nécessaire.
    }

    /// <summary>
    /// Ajoute un point au graphe historique avec buffer circulaire (Phase 6 - Tâche 13 & 15).
    /// Phase 9 - Tâche 21 : Stocke également les données complètes pour l'export CSV.
    /// </summary>
    private void AddDataPoint(float dbAValue, float dbfsValue, float leqValue, float peakValue)
    {
        if (_captureStartTime == DateTime.MinValue)
            return;

        // Calculer le temps écoulé en secondes
        double elapsedSeconds = (DateTime.Now - _captureStartTime).TotalSeconds;

        // Ajouter le point à l'historique interne
        HistoryData.Add(new DataPoint(elapsedSeconds, dbAValue));

        // Ajouter le point complet pour l'export CSV
        var exportDataPoint = new ExportDataPoint(
            timestamp: DateTime.Now,
            dbFs: dbfsValue,
            dbA: dbAValue,
            leq1Min: leqValue,
            peak: peakValue);

        ExportHistoryData.Add(exportDataPoint);

        // Ajouter le point au graphe LiveCharts2 (Phase 6 - Tâche 15)
        _chartValues.Add(new ObservablePoint(elapsedSeconds, dbAValue));

        // Log périodique pour debugging (tous les 20 points ≈ toutes les 10 secondes)
        if (_chartValues.Count % 20 == 0)
        {
            _logger.Debug("Graphe LiveCharts2: {Count} points, dernier point: ({X:F1}s, {Y:F1} dB(A))",
                _chartValues.Count, elapsedSeconds, dbAValue);
        }

        // Buffer circulaire : supprimer les points les plus anciens si dépassement
        while (_chartValues.Count > MAX_HISTORY_POINTS)
        {
            _chartValues.RemoveAt(0);
        }

        // Buffer circulaire pour l'historique interne et export
        while (HistoryData.Count > MAX_HISTORY_POINTS)
        {
            HistoryData.RemoveAt(0);
        }
        while (ExportHistoryData.Count > MAX_HISTORY_POINTS)
        {
            ExportHistoryData.RemoveAt(0);
        }

        // Auto-scroll de l'axe X si le temps dépasse 3 minutes (scroll automatique)
        // Permet de suivre les données en temps réel au-delà de 180s
        if (elapsedSeconds > 180)
        {
            XAxes = new Axis[]
            {
                new Axis
                {
                    Name = "Temps (s)",
                    NameTextSize = 12,
                    TextSize = 10,
                    MinLimit = elapsedSeconds - 180,
                    MaxLimit = elapsedSeconds,
                    ForceStepToMin = false,
                    MinStep = 10
                }
            };
        }

        // Notifier que l'état de CanExportCsv peut avoir changé
        ExportCsvCommand.NotifyCanExecuteChanged();
    }

    #endregion
}
