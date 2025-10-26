using NAudio.CoreAudioApi;
using NAudio.Wave;
using Serilog;

namespace ApplAudition.Services;

/// <summary>
/// Service de capture audio système via WASAPI loopback.
/// Capture l'audio système à 48 kHz, 32-bit float, convertit stéréo en mono.
/// </summary>
public class AudioCaptureService : IAudioCaptureService
{
    private readonly ILogger _logger;
    private WasapiLoopbackCapture? _capture;
    private readonly object _lock = new();
    private bool _isDisposed;

    public event EventHandler<AudioDataEventArgs>? DataAvailable;
    public event EventHandler<ErrorEventArgs>? ErrorOccurred;

    public bool IsCapturing { get; private set; }

    public AudioCaptureService(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Démarre la capture audio système.
    /// </summary>
    public Task StartAsync()
    {
        lock (_lock)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(AudioCaptureService));

            if (IsCapturing)
            {
                _logger.Warning("La capture est déjà active");
                return Task.CompletedTask;
            }

            try
            {
                // Obtenir le périphérique de sortie par défaut
                var enumerator = new MMDeviceEnumerator();
                var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

                _logger.Information("Périphérique audio détecté : {DeviceName}", device.FriendlyName);

                // Créer capture loopback
                _capture = new WasapiLoopbackCapture(device);

                // Souscrire aux événements
                _capture.DataAvailable += OnDataAvailableInternal;
                _capture.RecordingStopped += OnRecordingStopped;

                // Démarrer la capture
                _capture.StartRecording();
                IsCapturing = true;

                _logger.Information("Capture audio démarrée : {SampleRate} Hz, {Channels} canaux, {BitsPerSample} bits",
                    _capture.WaveFormat.SampleRate,
                    _capture.WaveFormat.Channels,
                    _capture.WaveFormat.BitsPerSample);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Erreur lors du démarrage de la capture");
                IsCapturing = false;
                throw new InvalidOperationException("Impossible de démarrer la capture audio", ex);
            }
        }
    }

    /// <summary>
    /// Arrête la capture audio.
    /// </summary>
    public Task StopAsync()
    {
        lock (_lock)
        {
            if (!IsCapturing || _capture == null)
            {
                _logger.Warning("Aucune capture active à arrêter");
                return Task.CompletedTask;
            }

            try
            {
                _capture.StopRecording();
                IsCapturing = false;
                _logger.Information("Capture audio arrêtée");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Erreur lors de l'arrêt de la capture");
                throw;
            }

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Redémarre la capture audio avec le périphérique par défaut actuel.
    /// Utilisé lors d'un changement de périphérique audio détecté.
    /// </summary>
    public async Task RestartAsync()
    {
        _logger.Information("Redémarrage de la capture audio suite à un changement de périphérique");

        try
        {
            // Arrêter la capture actuelle
            await StopAsync();

            // Disposer l'ancienne capture
            lock (_lock)
            {
                if (_capture != null)
                {
                    _capture.DataAvailable -= OnDataAvailableInternal;
                    _capture.RecordingStopped -= OnRecordingStopped;
                    _capture.Dispose();
                    _capture = null;
                }
            }

            // Petite pause pour laisser le système libérer les ressources
            await Task.Delay(500);

            // Redémarrer avec le nouveau périphérique
            await StartAsync();

            _logger.Information("Capture audio redémarrée avec succès");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Erreur lors du redémarrage de la capture audio");
            throw;
        }
    }

    /// <summary>
    /// Gestionnaire interne des données audio (appelé par NAudio).
    /// Convertit stéréo → mono et notifie les abonnés.
    /// </summary>
    private void OnDataAvailableInternal(object? sender, WaveInEventArgs e)
    {
        try
        {
            if (_capture == null || e.BytesRecorded == 0)
                return;

            var waveFormat = _capture.WaveFormat;

            // Convertir bytes → float[] (32-bit IEEE float)
            var floatBuffer = new float[e.BytesRecorded / 4]; // 4 bytes par sample
            Buffer.BlockCopy(e.Buffer, 0, floatBuffer, 0, e.BytesRecorded);

            // Convertir stéréo → mono si nécessaire
            float[] monoBuffer;
            if (waveFormat.Channels == 2)
            {
                monoBuffer = ConvertStereoToMono(floatBuffer);
            }
            else
            {
                monoBuffer = floatBuffer;
            }

            // Notifier les abonnés
            DataAvailable?.Invoke(this, new AudioDataEventArgs(monoBuffer, waveFormat.SampleRate));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Erreur lors du traitement des données audio");
            ErrorOccurred?.Invoke(this, new ErrorEventArgs(ex));
        }
    }

    /// <summary>
    /// Convertit un buffer stéréo en mono (moyenne des canaux L+R).
    /// </summary>
    private float[] ConvertStereoToMono(float[] stereoBuffer)
    {
        var monoBuffer = new float[stereoBuffer.Length / 2];

        for (int i = 0; i < monoBuffer.Length; i++)
        {
            var left = stereoBuffer[i * 2];
            var right = stereoBuffer[i * 2 + 1];
            monoBuffer[i] = (left + right) / 2.0f;
        }

        return monoBuffer;
    }

    /// <summary>
    /// Gestionnaire d'arrêt de l'enregistrement.
    /// </summary>
    private void OnRecordingStopped(object? sender, StoppedEventArgs e)
    {
        if (e.Exception != null)
        {
            _logger.Error(e.Exception, "Enregistrement arrêté à cause d'une erreur");
            ErrorOccurred?.Invoke(this, new ErrorEventArgs(e.Exception));
        }
        else
        {
            _logger.Information("Enregistrement arrêté normalement");
        }

        IsCapturing = false;
    }

    /// <summary>
    /// Libère les ressources.
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed)
            return;

        lock (_lock)
        {
            if (_capture != null)
            {
                if (IsCapturing)
                {
                    _capture.StopRecording();
                }

                _capture.DataAvailable -= OnDataAvailableInternal;
                _capture.RecordingStopped -= OnRecordingStopped;
                _capture.Dispose();
                _capture = null;
            }

            _isDisposed = true;
            _logger.Information("AudioCaptureService disposé");
        }
    }
}
