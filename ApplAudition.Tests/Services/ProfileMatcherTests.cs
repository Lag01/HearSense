using ApplAudition.Models;
using ApplAudition.Services;
using FluentAssertions;
using Moq;
using Serilog;
using Xunit;

namespace ApplAudition.Tests.Services;

/// <summary>
/// Tests unitaires pour ProfileMatcher (association périphérique → profil).
/// Vérifie le matching par patterns regex, le fallback Bluetooth et les edge cases.
/// </summary>
public class ProfileMatcherTests
{
    private readonly Mock<ILogger> _loggerMock;
    private readonly ProfileDatabase _profileDatabase;
    private readonly IProfileMatcher _profileMatcher;

    public ProfileMatcherTests()
    {
        _loggerMock = new Mock<ILogger>();

        // Créer une ProfileDatabase de test avec profils fictifs
        _profileDatabase = CreateTestProfileDatabase();

        _profileMatcher = new ProfileMatcher(_profileDatabase, _loggerMock.Object);
    }

    #region Helpers

    /// <summary>
    /// Crée une base de profils de test (sans charger le JSON embarqué).
    /// </summary>
    private ProfileDatabase CreateTestProfileDatabase()
    {
        var db = new ProfileDatabase(_loggerMock.Object);

        // Ajouter des profils via réflexion (contourner le chargement JSON)
        var profilesField = typeof(ProfileDatabase)
            .GetField("_profiles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var testProfiles = new List<Profile>
        {
            // Profil 1 : Over-ear ANC
            new Profile
            {
                Id = "over-ear-anc",
                Name = "Over-ear ANC (fermés)",
                Patterns = new List<string> { "WH-1000XM", "XM4", "XM5", "QC35", "Bose.*700" },
                SensitivityDbMw = 103,
                ImpedanceOhm = 47,
                ConstantC = -15.0,
                MarginDb = 6,
                IsFallback = false
            },

            // Profil 2 : IEM (intra-auriculaires)
            new Profile
            {
                Id = "iem",
                Name = "IEM (intra-auriculaires)",
                Patterns = new List<string> { "AirPods", "Galaxy Buds", "IEM", "Earbuds" },
                SensitivityDbMw = 105,
                ImpedanceOhm = 16,
                ConstantC = -8.0,
                MarginDb = 8,
                IsFallback = false
            },

            // Profil 3 : On-ear
            new Profile
            {
                Id = "on-ear",
                Name = "On-ear",
                Patterns = new List<string> { "Beats Solo", "Sennheiser.*Momentum.*On" },
                SensitivityDbMw = 100,
                ImpedanceOhm = 32,
                ConstantC = -12.0,
                MarginDb = 7,
                IsFallback = false
            }
        };

        profilesField?.SetValue(db, testProfiles);

        return db;
    }

    #endregion

    #region Tests matching exact

    [Fact]
    public void MatchProfile_WithSonyWH1000XM4_ReturnsOverEarAncProfile()
    {
        // Arrange
        string deviceName = "Sony WH-1000XM4";

        // Act
        var profile = _profileMatcher.MatchProfile(deviceName, DeviceType.Bluetooth);

        // Assert
        profile.Should().NotBeNull("Un profil devrait être trouvé pour Sony WH-1000XM4");
        profile!.Id.Should().Be("over-ear-anc");
        profile.Name.Should().Be("Over-ear ANC (fermés)");
        profile.IsFallback.Should().BeFalse("Match exact n'est pas un fallback");
    }

    [Fact]
    public void MatchProfile_WithWH1000XM5_ReturnsOverEarAncProfile()
    {
        // Arrange : pattern "XM5" devrait matcher
        string deviceName = "Sony WH-1000XM5 Wireless Headphones";

        // Act
        var profile = _profileMatcher.MatchProfile(deviceName, DeviceType.Bluetooth);

        // Assert
        profile.Should().NotBeNull();
        profile!.Id.Should().Be("over-ear-anc");
    }

    [Fact]
    public void MatchProfile_WithAirPodsPro_ReturnsIemProfile()
    {
        // Arrange
        string deviceName = "AirPods Pro";

        // Act
        var profile = _profileMatcher.MatchProfile(deviceName, DeviceType.Bluetooth);

        // Assert
        profile.Should().NotBeNull();
        profile!.Id.Should().Be("iem");
        profile.Name.Should().Be("IEM (intra-auriculaires)");
    }

    [Fact]
    public void MatchProfile_WithGalaxyBuds_ReturnsIemProfile()
    {
        // Arrange
        string deviceName = "Samsung Galaxy Buds2 Pro";

        // Act
        var profile = _profileMatcher.MatchProfile(deviceName, DeviceType.Bluetooth);

        // Assert
        profile.Should().NotBeNull();
        profile!.Id.Should().Be("iem");
    }

    [Fact]
    public void MatchProfile_WithBose700_ReturnsOverEarAncProfile()
    {
        // Arrange : pattern regex "Bose.*700" devrait matcher
        string deviceName = "Bose Noise Cancelling Headphones 700";

        // Act
        var profile = _profileMatcher.MatchProfile(deviceName, DeviceType.Bluetooth);

        // Assert
        profile.Should().NotBeNull();
        profile!.Id.Should().Be("over-ear-anc");
    }

    #endregion

    #region Tests matching case-insensitive

    [Fact]
    public void MatchProfile_CaseInsensitive_Matches()
    {
        // Arrange : test case insensitive
        string deviceName = "airpods pro"; // minuscules

        // Act
        var profile = _profileMatcher.MatchProfile(deviceName, DeviceType.Bluetooth);

        // Assert
        profile.Should().NotBeNull("Matching devrait être case-insensitive");
        profile!.Id.Should().Be("iem");
    }

    #endregion

    #region Tests no match

    [Fact]
    public void MatchProfile_WithUnknownDevice_ReturnsNull_IfNotBluetooth()
    {
        // Arrange : périphérique inconnu, type non-Bluetooth
        string deviceName = "Unknown Generic Headphones";

        // Act
        var profile = _profileMatcher.MatchProfile(deviceName, DeviceType.USB);

        // Assert : aucun fallback pour USB/WDM
        profile.Should().BeNull("Aucun profil ne devrait être trouvé pour périphérique inconnu non-Bluetooth");
    }

    [Fact]
    public void MatchProfile_WithSpeakers_ReturnsNull()
    {
        // Arrange : haut-parleurs (pas de match)
        string deviceName = "Realtek Speakers";

        // Act
        var profile = _profileMatcher.MatchProfile(deviceName, DeviceType.WDM);

        // Assert
        profile.Should().BeNull("Aucun profil ne devrait matcher pour haut-parleurs");
    }

    #endregion

    #region Tests fallback Bluetooth

    [Fact]
    public void MatchProfile_WithUnknownBluetooth_ReturnsFallbackProfile()
    {
        // Arrange : périphérique Bluetooth inconnu → fallback générique
        string deviceName = "Generic Bluetooth Headset XYZ";

        // Act
        var profile = _profileMatcher.MatchProfile(deviceName, DeviceType.Bluetooth);

        // Assert
        profile.Should().NotBeNull("Fallback Bluetooth devrait retourner un profil générique");
        profile!.IsFallback.Should().BeTrue("Le profil devrait être marqué comme fallback");
        profile.Name.Should().Contain("Générique Bluetooth");

        // Constante C plus conservatrice pour fallback
        profile.ConstantC.Should().Be(-12.0, "Fallback devrait utiliser constante conservatrice");
        profile.MarginDb.Should().Be(8, "Fallback devrait avoir marge plus large");
    }

    [Fact]
    public void MatchProfile_WithEmptyNameBluetooth_ReturnsFallback()
    {
        // Arrange : nom vide mais type Bluetooth
        string deviceName = "";

        // Act
        var profile = _profileMatcher.MatchProfile(deviceName, DeviceType.Bluetooth);

        // Assert
        profile.Should().NotBeNull("Fallback Bluetooth devrait fonctionner même avec nom vide");
        profile!.IsFallback.Should().BeTrue();
    }

    #endregion

    #region Tests edge cases

    [Fact]
    public void MatchProfile_WithNullDeviceName_ReturnsNull_IfNotBluetooth()
    {
        // Act
        var profile = _profileMatcher.MatchProfile(null, DeviceType.USB);

        // Assert
        profile.Should().BeNull();

        // Vérifier qu'un warning a été loggé
        _loggerMock.Verify(
            x => x.Warning(It.IsAny<string>()),
            Times.AtLeastOnce,
            "Un warning devrait être loggé pour nom de périphérique null");
    }

    [Fact]
    public void MatchProfile_WithWhitespaceDeviceName_ReturnsNull_IfNotBluetooth()
    {
        // Arrange
        string deviceName = "   "; // espaces uniquement

        // Act
        var profile = _profileMatcher.MatchProfile(deviceName, DeviceType.WDM);

        // Assert
        profile.Should().BeNull();
    }

    #endregion

    #region Tests patterns regex invalides

    [Fact]
    public void MatchProfile_WithInvalidRegexPattern_DoesNotCrash()
    {
        // Arrange : créer un profil avec pattern regex invalide
        var dbWithInvalidPattern = new ProfileDatabase(_loggerMock.Object);

        var profilesField = typeof(ProfileDatabase)
            .GetField("_profiles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var testProfiles = new List<Profile>
        {
            new Profile
            {
                Id = "invalid-regex",
                Name = "Profil avec regex invalide",
                Patterns = new List<string> { "[invalid(regex" }, // Pattern invalide
                ConstantC = -10.0,
                MarginDb = 5
            }
        };

        profilesField?.SetValue(dbWithInvalidPattern, testProfiles);

        var matcher = new ProfileMatcher(dbWithInvalidPattern, _loggerMock.Object);

        // Act : ne devrait pas crasher
        Action act = () => matcher.MatchProfile("Test Device", DeviceType.USB);

        // Assert
        act.Should().NotThrow("Matcher devrait gérer patterns regex invalides sans crasher");

        // Vérifier qu'un warning a été loggé
        _loggerMock.Verify(
            x => x.Warning(It.IsAny<ArgumentException>(), It.IsAny<string>(), It.IsAny<object[]>()),
            Times.AtLeastOnce,
            "Un warning devrait être loggé pour pattern regex invalide");
    }

    #endregion

    #region Tests priorité des matches

    [Fact]
    public void MatchProfile_ReturnsFirstMatch_WhenMultiplePatternsMatch()
    {
        // Arrange : périphérique qui pourrait matcher plusieurs patterns
        // (ex: "AirPods" et "Earbuds" sont tous deux dans le profil IEM)
        string deviceName = "Apple AirPods with Earbuds";

        // Act
        var profile = _profileMatcher.MatchProfile(deviceName, DeviceType.Bluetooth);

        // Assert : devrait retourner IEM (premier profil dont un pattern matche)
        profile.Should().NotBeNull();
        profile!.Id.Should().Be("iem");
    }

    #endregion

    #region Tests constantes et marges

    [Fact]
    public void MatchProfile_ExactMatch_ReturnsOriginalConstants()
    {
        // Arrange
        string deviceName = "Sony WH-1000XM4";

        // Act
        var profile = _profileMatcher.MatchProfile(deviceName, DeviceType.Bluetooth);

        // Assert : constantes du profil exact (pas de fallback)
        profile.Should().NotBeNull();
        profile!.ConstantC.Should().Be(-15.0, "Match exact devrait utiliser constante du profil");
        profile.MarginDb.Should().Be(6, "Match exact devrait utiliser marge du profil");
    }

    [Fact]
    public void MatchProfile_FallbackMatch_UsesConservativeConstants()
    {
        // Arrange : fallback Bluetooth
        string deviceName = "Unknown BT Headset";

        // Act
        var profile = _profileMatcher.MatchProfile(deviceName, DeviceType.Bluetooth);

        // Assert : constantes conservatrices pour fallback
        profile.Should().NotBeNull();
        profile!.ConstantC.Should().Be(-12.0, "Fallback devrait utiliser C conservateur (-12 au lieu de -15)");
        profile.MarginDb.Should().Be(8, "Fallback devrait avoir marge plus large (8 au lieu de 6)");
    }

    #endregion

    #region Tests profils multiples

    [Fact]
    public void MatchProfile_WithMultipleProfilesInDatabase_FindsCorrectOne()
    {
        // Arrange : base avec plusieurs profils
        string[] deviceNames = {
            "Sony WH-1000XM4",        // → over-ear-anc
            "AirPods Pro",            // → iem
            "Beats Solo 3"            // → on-ear
        };

        string[] expectedProfileIds = { "over-ear-anc", "iem", "on-ear" };

        // Act & Assert
        for (int i = 0; i < deviceNames.Length; i++)
        {
            var profile = _profileMatcher.MatchProfile(deviceNames[i], DeviceType.Bluetooth);
            profile.Should().NotBeNull($"Profil devrait être trouvé pour {deviceNames[i]}");
            profile!.Id.Should().Be(expectedProfileIds[i],
                $"{deviceNames[i]} devrait matcher profil {expectedProfileIds[i]}");
        }
    }

    #endregion
}
