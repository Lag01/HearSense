using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ApplAudition.Models;
using ApplAudition.Services;
using Serilog;

namespace ApplAudition.ViewModels;

/// <summary>
/// ViewModel pour la calibration personnalisée (Phase 8 - Tâche 19).
/// Permet à l'utilisateur d'ajuster la constante C avec un sonomètre de référence.
/// </summary>
public partial class CalibrationViewModel : BaseViewModel
{
    private readonly IEstimationModeManager _estimationModeManager;
    private readonly ILogger _logger;

    #region Propriétés observables

    /// <summary>
    /// SPL mesuré au sonomètre de référence (en dB(A)).
    /// Saisi par l'utilisateur.
    /// </summary>
    [ObservableProperty]
    private float _measuredSpl = 85.0f;

    /// <summary>
    /// SPL estimé actuel par l'application (en dB(A)).
    /// Valeur en lecture seule, calculée par EstimationModeManager.
    /// </summary>
    [ObservableProperty]
    private float _currentEstimatedSpl;

    /// <summary>
    /// Indique si une calibration personnalisée est active.
    /// </summary>
    [ObservableProperty]
    private bool _isCalibrated;

    /// <summary>
    /// Constante de calibration actuelle (C_calibrated).
    /// Null si pas de calibration personnalisée.
    /// </summary>
    [ObservableProperty]
    private float? _calibrationConstantC;

    /// <summary>
    /// Constante de calibration du profil heuristique (C_profil).
    /// Null si en Mode A.
    /// </summary>
    [ObservableProperty]
    private float? _profileConstantC;

    /// <summary>
    /// Message d'information pour l'utilisateur.
    /// </summary>
    [ObservableProperty]
    private string _statusMessage = "Prêt à calibrer";

    /// <summary>
    /// Texte d'information sur la constante C calibrée.
    /// </summary>
    [ObservableProperty]
    private string _calibrationInfo = "Aucune calibration active";

    /// <summary>
    /// Indique si la calibration est disponible (Mode B uniquement).
    /// </summary>
    [ObservableProperty]
    private bool _canCalibrate;

    #endregion

    public CalibrationViewModel(
        IEstimationModeManager estimationModeManager,
        ILogger logger)
    {
        _estimationModeManager = estimationModeManager;
        _logger = logger;

        // S'abonner aux changements de mode
        _estimationModeManager.ModeChanged += OnModeChanged;

        // Initialiser les valeurs
        UpdateCalibrationInfo();
    }

    #region Commandes

    /// <summary>
    /// Commande de calibration (Phase 8 - Tâche 19).
    /// Formule : C_new = C_old + (SPL_mesuré - SPL_estimé).
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanCalibrate))]
    private async Task CalibrateAsync()
    {
        try
        {
            _logger.Information(
                "Calibration démarrée - SPL mesuré: {MeasuredSpl}, SPL estimé: {EstimatedSpl}",
                MeasuredSpl,
                CurrentEstimatedSpl);

            // Récupérer la constante C actuelle (calibrée si existe, sinon profil)
            float currentConstantC = _estimationModeManager.CalibrationConstantC
                                     ?? (float)(_estimationModeManager.CurrentProfile?.ConstantC ?? 0);

            // Calculer la nouvelle constante C
            // Formule : C_new = C_old + (SPL_mesuré - SPL_estimé)
            float newConstantC = currentConstantC + (MeasuredSpl - CurrentEstimatedSpl);

            _logger.Information(
                "Calcul calibration - C_old: {OldC}, C_new: {NewC}",
                currentConstantC,
                newConstantC);

            // Appliquer la nouvelle constante
            await _estimationModeManager.SetCalibrationConstantAsync(newConstantC);

            StatusMessage = $"Calibration appliquée ! C = {newConstantC:F1} dB";

            _logger.Information("Calibration appliquée avec succès");

            // Mettre à jour les informations
            UpdateCalibrationInfo();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Erreur lors de la calibration");
            StatusMessage = $"Erreur : {ex.Message}";
        }
    }

    /// <summary>
    /// Commande de réinitialisation de la calibration (Phase 8 - Tâche 19).
    /// Retour à la constante du profil heuristique.
    /// </summary>
    [RelayCommand]
    private async Task ResetCalibrationAsync()
    {
        try
        {
            _logger.Information("Réinitialisation de la calibration");

            await _estimationModeManager.ResetCalibrationAsync();

            StatusMessage = "Calibration réinitialisée, retour au profil heuristique";

            _logger.Information("Calibration réinitialisée avec succès");

            // Mettre à jour les informations
            UpdateCalibrationInfo();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Erreur lors de la réinitialisation de la calibration");
            StatusMessage = $"Erreur : {ex.Message}";
        }
    }

    #endregion

    #region Méthodes privées

    /// <summary>
    /// Met à jour les informations de calibration depuis EstimationModeManager.
    /// </summary>
    public void UpdateCalibrationInfo()
    {
        IsCalibrated = _estimationModeManager.IsCalibrated;
        CalibrationConstantC = _estimationModeManager.CalibrationConstantC;

        // Récupérer la constante du profil si Mode B
        ProfileConstantC = (float?)_estimationModeManager.CurrentProfile?.ConstantC;

        // Vérifier si la calibration est disponible (Mode B uniquement)
        CanCalibrate = _estimationModeManager.CurrentMode == EstimationMode.ModeB &&
                       _estimationModeManager.CurrentProfile != null;

        // Mettre à jour le texte d'information
        if (IsCalibrated)
        {
            CalibrationInfo = $"Calibré : C = {CalibrationConstantC:F1} dB (profil : {ProfileConstantC:F1} dB)";
        }
        else if (CanCalibrate)
        {
            CalibrationInfo = $"Profil heuristique : C = {ProfileConstantC:F1} dB";
        }
        else
        {
            CalibrationInfo = "Aucune calibration active (Mode A)";
        }

        _logger.Debug(
            "Infos calibration mises à jour - IsCalibrated: {IsCalibrated}, C: {C}",
            IsCalibrated,
            CalibrationConstantC);

        // Rafraîchir la commande Calibrate
        CalibrateCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Met à jour le SPL estimé actuel (appelé depuis MainViewModel).
    /// </summary>
    /// <param name="estimatedSpl">SPL estimé par l'application</param>
    public void UpdateCurrentEstimatedSpl(float estimatedSpl)
    {
        CurrentEstimatedSpl = estimatedSpl;
    }

    /// <summary>
    /// Gestionnaire d'événement : le mode d'estimation a changé.
    /// </summary>
    private void OnModeChanged(object? sender, EventArgs e)
    {
        _logger.Debug("Mode d'estimation changé, mise à jour des infos de calibration");

        // Mettre à jour les informations de calibration
        UpdateCalibrationInfo();
    }

    #endregion
}
