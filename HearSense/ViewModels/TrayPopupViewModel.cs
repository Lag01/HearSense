using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HearSense.Models;

namespace HearSense.ViewModels;

/// <summary>
/// ViewModel pour le popup tray miniature (style EarTrumpet).
/// Affiche la jauge dB(A) et des boutons d'action rapide.
/// </summary>
public partial class TrayPopupViewModel : BaseViewModel
{
    #region Propriétés observables

    [ObservableProperty]
    private float _currentDbA;

    [ObservableProperty]
    private ExposureCategory _exposureCategory = ExposureCategory.Safe;

    #endregion

    /// <summary>
    /// Met à jour les valeurs affichées dans le popup.
    /// </summary>
    public void UpdateValues(float dbA, ExposureCategory category)
    {
        CurrentDbA = dbA;
        ExposureCategory = category;
    }
}
