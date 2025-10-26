namespace HearSense.Models;

/// <summary>
/// Type de périphérique audio détecté.
/// </summary>
public enum DeviceType
{
    /// <summary>
    /// Périphérique Bluetooth (casques sans fil).
    /// </summary>
    Bluetooth,

    /// <summary>
    /// Périphérique USB (casques USB, DAC).
    /// </summary>
    USB,

    /// <summary>
    /// Périphérique WDM générique (jack 3.5mm, HDMI, etc.).
    /// </summary>
    WDM,

    /// <summary>
    /// Type inconnu ou non déterminé.
    /// </summary>
    Unknown
}
