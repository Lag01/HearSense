using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using ApplAudition.Models;
using Serilog;

namespace ApplAudition.Services;

/// <summary>
/// Service de détection du périphérique audio actif.
/// Utilise NAudio MMDeviceEnumerator pour récupérer le périphérique de sortie par défaut.
/// </summary>
public class AudioDeviceService : IAudioDeviceService, IDisposable
{
    private readonly ILogger _logger;
    private MMDeviceEnumerator? _enumerator;
    private MMDevice? _currentDevice;
    private bool _isDisposed;

    public string? DeviceName { get; private set; }
    public DeviceType DeviceType { get; private set; }
    public bool IsSpeaker { get; private set; }

    public event EventHandler? DeviceChanged;

    public AudioDeviceService(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Initialise la détection et récupère le périphérique actif.
    /// </summary>
    public Task InitializeAsync()
    {
        try
        {
            _enumerator = new MMDeviceEnumerator();

            // Récupérer le périphérique de sortie par défaut
            RefreshDevice();

            // S'abonner aux notifications de changement de périphérique
            _enumerator.RegisterEndpointNotificationCallback(new DeviceNotificationClient(this));

            _logger.Information("AudioDeviceService initialisé avec succès");
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Erreur lors de l'initialisation d'AudioDeviceService");
            throw;
        }
    }

    /// <summary>
    /// Rafraîchit les informations du périphérique actif.
    /// </summary>
    public void RefreshDevice()
    {
        try
        {
            if (_enumerator == null)
            {
                _enumerator = new MMDeviceEnumerator();
            }

            // Récupérer le périphérique de sortie par défaut
            _currentDevice = _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

            if (_currentDevice == null)
            {
                _logger.Warning("Aucun périphérique de sortie détecté");
                DeviceName = null;
                DeviceType = Models.DeviceType.Unknown;
                IsSpeaker = false;
                return;
            }

            // Récupérer le nom du périphérique
            DeviceName = _currentDevice.FriendlyName;

            // Détecter le type de périphérique
            DeviceType = DetectDeviceType(_currentDevice);

            // Détecter si c'est une enceinte/haut-parleur
            IsSpeaker = IsSpeakerDevice(_currentDevice);

            _logger.Information("Périphérique détecté : {DeviceName} (Type: {DeviceType}, Enceinte: {IsSpeaker})",
                DeviceName, DeviceType, IsSpeaker);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Erreur lors de la détection du périphérique");
            DeviceName = null;
            DeviceType = Models.DeviceType.Unknown;
            IsSpeaker = false;
        }
    }

    /// <summary>
    /// Détecte le type de périphérique (Bluetooth, USB, WDM, Unknown).
    /// Analyse les propriétés du périphérique et le nom friendly.
    /// </summary>
    private DeviceType DetectDeviceType(MMDevice device)
    {
        try
        {
            string friendlyName = device.FriendlyName.ToLowerInvariant();

            // Détecter Bluetooth via le nom
            if (friendlyName.Contains("bluetooth") ||
                friendlyName.Contains("bt") ||
                friendlyName.Contains("wireless") ||
                friendlyName.Contains("airpods") ||
                friendlyName.Contains("buds"))
            {
                _logger.Debug("Type détecté : Bluetooth (via nom)");
                return Models.DeviceType.Bluetooth;
            }

            // Détecter USB via le nom ou les propriétés
            if (friendlyName.Contains("usb"))
            {
                _logger.Debug("Type détecté : USB (via nom)");
                return Models.DeviceType.USB;
            }

            // Tenter de détecter via les propriétés du périphérique
            // Note : NAudio ne fournit pas d'accès direct au bus type,
            // mais on peut inférer via le DeviceInterface ou l'ID
            string deviceId = device.ID.ToLowerInvariant();

            if (deviceId.Contains("usb"))
            {
                _logger.Debug("Type détecté : USB (via ID)");
                return Models.DeviceType.USB;
            }

            if (deviceId.Contains("bth") || deviceId.Contains("bluetooth"))
            {
                _logger.Debug("Type détecté : Bluetooth (via ID)");
                return Models.DeviceType.Bluetooth;
            }

            // Par défaut : WDM (Windows Driver Model - typiquement jack 3.5mm, HDMI)
            _logger.Debug("Type détecté : WDM (par défaut)");
            return Models.DeviceType.WDM;
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Erreur lors de la détection du type de périphérique");
            return Models.DeviceType.Unknown;
        }
    }

    /// <summary>
    /// Détecte si le périphérique est une enceinte/haut-parleur.
    /// Retourne true si c'est une enceinte, false si c'est probablement un casque/écouteur.
    /// </summary>
    private bool IsSpeakerDevice(MMDevice device)
    {
        try
        {
            string friendlyName = device.FriendlyName.ToLowerInvariant();

            // Patterns typiques des enceintes/haut-parleurs
            string[] speakerPatterns = new[]
            {
                "speakers",
                "speaker",
                "haut-parleurs",
                "haut-parleur",
                "enceinte",
                "monitor",
                "realtek", // Souvent des sorties intégrées carte mère (enceintes)
                "hdmi",    // HDMI = généralement TV/moniteur avec enceintes
                "displayport",
                "display audio",
                "tv",
                "desktop speaker"
            };

            // Vérifier si le nom contient un des patterns d'enceintes
            bool isSpeaker = speakerPatterns.Any(pattern => friendlyName.Contains(pattern));

            if (isSpeaker)
            {
                _logger.Debug("Périphérique identifié comme enceinte : {DeviceName}", device.FriendlyName);
            }
            else
            {
                _logger.Debug("Périphérique identifié comme casque/écouteur : {DeviceName}", device.FriendlyName);
            }

            return isSpeaker;
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Erreur lors de la détection enceinte/casque");
            // En cas d'erreur, on considère par défaut que ce n'est PAS une enceinte
            // (pour permettre à l'app de fonctionner)
            return false;
        }
    }

    /// <summary>
    /// Notifie les abonnés d'un changement de périphérique.
    /// </summary>
    private void OnDeviceChanged()
    {
        RefreshDevice();
        DeviceChanged?.Invoke(this, EventArgs.Empty);
        _logger.Information("Changement de périphérique détecté");
    }

    /// <summary>
    /// Client de notification pour les changements de périphérique.
    /// </summary>
    private class DeviceNotificationClient : IMMNotificationClient
    {
        private readonly AudioDeviceService _service;

        public DeviceNotificationClient(AudioDeviceService service)
        {
            _service = service;
        }

        public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
        {
            // Notifier uniquement pour les changements de sortie audio
            if (flow == DataFlow.Render && role == Role.Multimedia)
            {
                _service.OnDeviceChanged();
            }
        }

        public void OnDeviceAdded(string pwstrDeviceId) { }
        public void OnDeviceRemoved(string pwstrDeviceId) { }
        public void OnDeviceStateChanged(string deviceId, DeviceState newState) { }
        public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key) { }
    }

    /// <summary>
    /// Libère les ressources.
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed)
            return;

        _currentDevice?.Dispose();
        _enumerator?.Dispose();

        _isDisposed = true;
        _logger.Information("AudioDeviceService disposé");
    }
}
