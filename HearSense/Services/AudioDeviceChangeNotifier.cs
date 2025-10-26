using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using Serilog;

namespace HearSense.Services;

/// <summary>
/// Notifie des changements de périphériques audio (changement de périphérique par défaut, etc.).
/// Implémente IMMNotificationClient de NAudio pour recevoir les notifications système.
/// </summary>
public class AudioDeviceChangeNotifier : IMMNotificationClient, IDisposable
{
    private readonly ILogger _logger;
    private readonly MMDeviceEnumerator _enumerator;
    private bool _isDisposed;

    /// <summary>
    /// Événement déclenché quand le périphérique audio par défaut change.
    /// </summary>
    public event EventHandler<DefaultDeviceChangedEventArgs>? DefaultDeviceChanged;

    public AudioDeviceChangeNotifier(ILogger logger)
    {
        _logger = logger;
        _enumerator = new MMDeviceEnumerator();
    }

    /// <summary>
    /// Démarre l'écoute des changements de périphériques audio.
    /// </summary>
    public void Start()
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(AudioDeviceChangeNotifier));

        _enumerator.RegisterEndpointNotificationCallback(this);
        _logger.Information("AudioDeviceChangeNotifier démarré - Écoute des changements de périphériques");
    }

    /// <summary>
    /// Arrête l'écoute des changements de périphériques audio.
    /// </summary>
    public void Stop()
    {
        if (_isDisposed)
            return;

        _enumerator.UnregisterEndpointNotificationCallback(this);
        _logger.Information("AudioDeviceChangeNotifier arrêté");
    }

    /// <summary>
    /// Appelé quand le périphérique par défaut change.
    /// </summary>
    public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
    {
        try
        {
            // Nous sommes intéressés uniquement par les changements de périphérique de sortie (Render)
            if (flow != DataFlow.Render)
                return;

            _logger.Information("Changement de périphérique audio par défaut détecté : Flow={Flow}, Role={Role}, DeviceId={DeviceId}",
                flow, role, defaultDeviceId);

            // Obtenir le nom convivial du nouveau périphérique
            string deviceName = "Inconnu";
            try
            {
                var device = _enumerator.GetDevice(defaultDeviceId);
                deviceName = device.FriendlyName;
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Impossible de récupérer le nom du périphérique {DeviceId}", defaultDeviceId);
            }

            // Notifier les abonnés
            DefaultDeviceChanged?.Invoke(this, new DefaultDeviceChangedEventArgs(deviceName, defaultDeviceId));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Erreur lors du traitement du changement de périphérique par défaut");
        }
    }

    #region Méthodes IMMNotificationClient non utilisées

    public void OnDeviceStateChanged(string deviceId, DeviceState newState)
    {
        // Non utilisé pour le moment
    }

    public void OnDeviceAdded(string pwstrDeviceId)
    {
        // Non utilisé pour le moment
    }

    public void OnDeviceRemoved(string deviceId)
    {
        // Non utilisé pour le moment
    }

    public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key)
    {
        // Non utilisé pour le moment
    }

    #endregion

    public void Dispose()
    {
        if (_isDisposed)
            return;

        Stop();
        _enumerator.Dispose();
        _isDisposed = true;
    }
}

/// <summary>
/// Arguments de l'événement de changement de périphérique par défaut.
/// </summary>
public class DefaultDeviceChangedEventArgs : EventArgs
{
    public string DeviceName { get; }
    public string DeviceId { get; }

    public DefaultDeviceChangedEventArgs(string deviceName, string deviceId)
    {
        DeviceName = deviceName;
        DeviceId = deviceId;
    }
}
