using System.Text.RegularExpressions;
using ApplAudition.Models;
using Serilog;

namespace ApplAudition.Services;

/// <summary>
/// Service de matching entre périphérique audio et profil heuristique.
/// Utilise des patterns regex pour associer un périphérique à un profil.
/// </summary>
public class ProfileMatcher : IProfileMatcher
{
    private readonly ProfileDatabase _profileDatabase;
    private readonly ILogger _logger;

    public ProfileMatcher(ProfileDatabase profileDatabase, ILogger logger)
    {
        _profileDatabase = profileDatabase;
        _logger = logger;
    }

    /// <summary>
    /// Tente de trouver un profil correspondant au périphérique.
    /// </summary>
    /// <param name="deviceName">Nom du périphérique (ex: "Sony WH-1000XM4").</param>
    /// <param name="deviceType">Type du périphérique (Bluetooth, USB, WDM, Unknown).</param>
    /// <returns>
    /// Profil correspondant, ou null si aucun match trouvé.
    /// Si fallback Bluetooth activé, retourne profil générique "over-ear-anc" avec IsFallback=true.
    /// </returns>
    public Profile? MatchProfile(string? deviceName, DeviceType deviceType)
    {
        if (string.IsNullOrWhiteSpace(deviceName))
        {
            _logger.Warning("Nom de périphérique vide ou null, aucun matching possible");
            return ApplyFallback(deviceType);
        }

        _logger.Debug("Tentative de matching pour périphérique : {DeviceName} (Type: {DeviceType})",
            deviceName, deviceType);

        // Parcourir tous les profils et tenter de matcher via regex
        foreach (var profile in _profileDatabase.Profiles)
        {
            foreach (var pattern in profile.Patterns)
            {
                try
                {
                    var regex = new Regex(pattern, RegexOptions.IgnoreCase);

                    if (regex.IsMatch(deviceName))
                    {
                        _logger.Information(
                            "Match trouvé : {DeviceName} → Profil '{ProfileName}' (pattern: '{Pattern}')",
                            deviceName, profile.Name, pattern);

                        // Retourner une copie du profil (pas de fallback)
                        return new Profile
                        {
                            Id = profile.Id,
                            Name = profile.Name,
                            Patterns = profile.Patterns,
                            SensitivityDbMw = profile.SensitivityDbMw,
                            ImpedanceOhm = profile.ImpedanceOhm,
                            ConstantC = profile.ConstantC,
                            MarginDb = profile.MarginDb,
                            IsFallback = false
                        };
                    }
                }
                catch (ArgumentException ex)
                {
                    _logger.Warning(ex, "Pattern regex invalide : {Pattern}", pattern);
                }
            }
        }

        // Aucun match trouvé : tenter fallback
        _logger.Debug("Aucun match trouvé pour {DeviceName}, tentative de fallback", deviceName);
        return ApplyFallback(deviceType);
    }

    /// <summary>
    /// Applique la logique de fallback basée sur le type de périphérique.
    /// </summary>
    /// <param name="deviceType">Type du périphérique.</param>
    /// <returns>Profil générique ou null.</returns>
    private Profile? ApplyFallback(DeviceType deviceType)
    {
        // Fallback Bluetooth : utiliser profil "over-ear-anc" générique (conservateur)
        if (deviceType == DeviceType.Bluetooth)
        {
            var fallbackProfile = _profileDatabase.GetProfileById("over-ear-anc");

            if (fallbackProfile != null)
            {
                _logger.Information(
                    "Fallback Bluetooth activé : utilisation du profil générique '{ProfileName}' (conservateur)",
                    fallbackProfile.Name);

                // Retourner une copie avec IsFallback=true et constante C plus conservatrice
                return new Profile
                {
                    Id = fallbackProfile.Id,
                    Name = $"{fallbackProfile.Name} (Générique Bluetooth)",
                    Patterns = fallbackProfile.Patterns,
                    SensitivityDbMw = fallbackProfile.SensitivityDbMw,
                    ImpedanceOhm = fallbackProfile.ImpedanceOhm,
                    ConstantC = -12.0, // Plus conservateur que -15.0 (sur-estime le niveau)
                    MarginDb = 8, // Marge plus large (8 dB au lieu de 6)
                    IsFallback = true
                };
            }

            _logger.Warning("Profil 'over-ear-anc' introuvable pour fallback Bluetooth");
        }

        // Aucun fallback applicable
        _logger.Information("Aucun profil trouvé, retour en Mode A (zero-input conservateur)");
        return null;
    }
}
