using ApplAudition.Models;

namespace ApplAudition.Services;

/// <summary>
/// Service de détection du périphérique audio actif.
/// Fournit le nom et le type du périphérique de sortie utilisé pour la capture loopback.
/// </summary>
public interface IAudioDeviceService
{
    /// <summary>
    /// Nom du périphérique audio actif (ex: "Sony WH-1000XM4").
    /// </summary>
    string? DeviceName { get; }

    /// <summary>
    /// Type du périphérique audio (Bluetooth, USB, WDM, Unknown).
    /// </summary>
    DeviceType DeviceType { get; }

    /// <summary>
    /// Indique si le périphérique détecté est une enceinte/haut-parleur.
    /// True si c'est une enceinte, False si c'est un casque/écouteur.
    /// </summary>
    bool IsSpeaker { get; }

    /// <summary>
    /// Événement déclenché lorsque le périphérique audio change.
    /// </summary>
    event EventHandler? DeviceChanged;

    /// <summary>
    /// Initialise la détection et récupère le périphérique actif.
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Rafraîchit les informations du périphérique actif.
    /// </summary>
    void RefreshDevice();
}
