using NAudio.CoreAudioApi;
using Serilog;

namespace ApplAudition.Services;

/// <summary>
/// Service de récupération du volume système Windows.
/// Utilise NAudio.CoreAudioApi pour accéder au volume du périphérique de sortie actif.
/// IMPORTANT : Ce service résout le problème où WASAPI Loopback capture le signal
/// AVANT l'application du volume système.
/// </summary>
public class SystemVolumeService : ISystemVolumeService, IDisposable
{
    private readonly ILogger _logger;
    private MMDevice? _device;
    private AudioEndpointVolume? _volumeEndpoint;
    private bool _isDisposed;

    public event EventHandler<VolumeChangedEventArgs>? VolumeChanged;

    public SystemVolumeService(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Initialise le service et s'abonne aux changements de volume.
    /// </summary>
    public Task InitializeAsync()
    {
        try
        {
            // Obtenir le périphérique de sortie par défaut
            var enumerator = new MMDeviceEnumerator();
            _device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

            if (_device == null)
            {
                _logger.Warning("Impossible de récupérer le périphérique audio par défaut");
                return Task.CompletedTask;
            }

            // Obtenir l'interface de contrôle du volume
            _volumeEndpoint = _device.AudioEndpointVolume;

            // S'abonner aux notifications de changement de volume
            _volumeEndpoint.OnVolumeNotification += OnVolumeNotification;

            float currentVolume = GetCurrentVolume();
            float currentVolumeDb = GetVolumeDb();

            _logger.Information(
                "SystemVolumeService initialisé - Volume actuel : {Volume:P0} ({VolumeDb:F1} dB)",
                currentVolume,
                currentVolumeDb);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Erreur lors de l'initialisation de SystemVolumeService");
            throw;
        }
    }

    /// <summary>
    /// Obtient le niveau de volume système actuel (0.0 à 1.0).
    /// </summary>
    public float GetCurrentVolume()
    {
        try
        {
            if (_volumeEndpoint == null)
            {
                _logger.Warning("Volume endpoint non initialisé, retour de 1.0 (100%)");
                return 1.0f;
            }

            // Obtenir le volume master (scalar 0.0 à 1.0)
            float volume = _volumeEndpoint.MasterVolumeLevelScalar;
            return volume;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Erreur lors de la récupération du volume système");
            return 1.0f; // Fallback sécurisé
        }
    }

    /// <summary>
    /// Obtient le niveau de volume en dB (relatif au maximum).
    /// IMPORTANT : Cette valeur est utilisée pour corriger le calcul SPL.
    /// </summary>
    /// <returns>Volume en dB (typiquement -96 dB à 0 dB)</returns>
    public float GetVolumeDb()
    {
        try
        {
            if (_volumeEndpoint == null)
            {
                _logger.Warning("Volume endpoint non initialisé, retour de 0 dB");
                return 0.0f;
            }

            // Obtenir le volume en dB
            // Windows utilise une échelle logarithmique où 0 dB = volume max
            // et des valeurs négatives pour les volumes plus faibles (ex: -20 dB, -40 dB)
            float volumeDb = _volumeEndpoint.MasterVolumeLevel;
            return volumeDb;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Erreur lors de la récupération du volume en dB");
            return 0.0f; // Fallback sécurisé
        }
    }

    /// <summary>
    /// Gestionnaire d'événement : le volume système a changé.
    /// </summary>
    private void OnVolumeNotification(AudioVolumeNotificationData data)
    {
        try
        {
            float volume = data.MasterVolume;
            float volumeDb = GetVolumeDb();

            _logger.Debug(
                "Volume système changé : {Volume:P0} ({VolumeDb:F1} dB)",
                volume,
                volumeDb);

            // Notifier les abonnés
            VolumeChanged?.Invoke(this, new VolumeChangedEventArgs(volume, volumeDb));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Erreur lors du traitement de la notification de volume");
        }
    }

    /// <summary>
    /// Libère les ressources.
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed)
            return;

        if (_volumeEndpoint != null)
        {
            _volumeEndpoint.OnVolumeNotification -= OnVolumeNotification;
            _volumeEndpoint.Dispose();
            _volumeEndpoint = null;
        }

        if (_device != null)
        {
            _device.Dispose();
            _device = null;
        }

        _isDisposed = true;
        _logger.Information("SystemVolumeService disposé");
    }
}
