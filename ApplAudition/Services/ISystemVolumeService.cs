namespace ApplAudition.Services;

/// <summary>
/// Interface pour le service de récupération du volume système Windows.
/// Permet de monitorer le niveau de volume en temps réel.
/// </summary>
public interface ISystemVolumeService
{
    /// <summary>
    /// Obtient le niveau de volume système actuel (0.0 à 1.0).
    /// </summary>
    float GetCurrentVolume();

    /// <summary>
    /// Obtient le niveau de volume en dB (relatif au maximum).
    /// </summary>
    /// <returns>Volume en dB (typiquement -96 dB à 0 dB)</returns>
    float GetVolumeDb();

    /// <summary>
    /// Événement déclenché lorsque le volume système change.
    /// </summary>
    event EventHandler<VolumeChangedEventArgs>? VolumeChanged;

    /// <summary>
    /// Initialise le monitoring du volume système.
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Libère les ressources.
    /// </summary>
    void Dispose();
}

/// <summary>
/// Arguments d'événement pour les changements de volume.
/// </summary>
public class VolumeChangedEventArgs : EventArgs
{
    public float Volume { get; }
    public float VolumeDb { get; }

    public VolumeChangedEventArgs(float volume, float volumeDb)
    {
        Volume = volume;
        VolumeDb = volumeDb;
    }
}
