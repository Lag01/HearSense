using CommunityToolkit.Mvvm.ComponentModel;

namespace ApplAudition.ViewModels;

/// <summary>
/// Classe de base pour tous les ViewModels avec support MVVM (INotifyPropertyChanged).
/// </summary>
public abstract class BaseViewModel : ObservableObject
{
    // Cette classe hérite de ObservableObject du CommunityToolkit.Mvvm
    // qui implémente INotifyPropertyChanged automatiquement.
    // Les propriétés des ViewModels dérivés peuvent utiliser [ObservableProperty]
    // pour générer automatiquement le code de notification.
}
