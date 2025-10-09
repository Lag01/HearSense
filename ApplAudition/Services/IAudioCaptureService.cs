namespace ApplAudition.Services;

/// <summary>
/// Interface pour le service de capture audio système (WASAPI loopback).
/// </summary>
public interface IAudioCaptureService : IDisposable
{
    /// <summary>
    /// Événement déclenché lorsque de nouvelles données audio sont disponibles.
    /// </summary>
    event EventHandler<AudioDataEventArgs>? DataAvailable;

    /// <summary>
    /// Événement déclenché en cas d'erreur de capture.
    /// </summary>
    event EventHandler<ErrorEventArgs>? ErrorOccurred;

    /// <summary>
    /// Démarre la capture audio.
    /// </summary>
    Task StartAsync();

    /// <summary>
    /// Arrête la capture audio.
    /// </summary>
    Task StopAsync();

    /// <summary>
    /// Indique si la capture est active.
    /// </summary>
    bool IsCapturing { get; }
}

/// <summary>
/// Données audio capturées.
/// </summary>
public class AudioDataEventArgs : EventArgs
{
    /// <summary>
    /// Buffer audio (mono, 32-bit float, normalisé [-1.0, 1.0]).
    /// </summary>
    public float[] Buffer { get; }

    /// <summary>
    /// Taux d'échantillonnage (Hz).
    /// </summary>
    public int SampleRate { get; }

    public AudioDataEventArgs(float[] buffer, int sampleRate)
    {
        Buffer = buffer;
        SampleRate = sampleRate;
    }
}

/// <summary>
/// Erreur de capture audio.
/// </summary>
public class ErrorEventArgs : EventArgs
{
    public Exception Exception { get; }

    public ErrorEventArgs(Exception exception)
    {
        Exception = exception;
    }
}
