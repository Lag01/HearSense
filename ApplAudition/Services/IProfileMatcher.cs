using ApplAudition.Models;

namespace ApplAudition.Services;

/// <summary>
/// Service de matching entre périphérique audio et profil heuristique.
/// Utilise des patterns regex pour associer un périphérique à un profil.
/// </summary>
public interface IProfileMatcher
{
    /// <summary>
    /// Tente de trouver un profil correspondant au périphérique.
    /// </summary>
    /// <param name="deviceName">Nom du périphérique (ex: "Sony WH-1000XM4").</param>
    /// <param name="deviceType">Type du périphérique (Bluetooth, USB, WDM, Unknown).</param>
    /// <returns>
    /// Profil correspondant, ou null si aucun match trouvé.
    /// Si fallback Bluetooth activé, retourne profil générique "over-ear-anc" avec IsFallback=true.
    /// </returns>
    Profile? MatchProfile(string? deviceName, DeviceType deviceType);
}
