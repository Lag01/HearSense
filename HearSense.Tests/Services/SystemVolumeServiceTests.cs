using HearSense.Services;
using FluentAssertions;
using Moq;
using Serilog;
using Xunit;

namespace HearSense.Tests.Services;

/// <summary>
/// Tests unitaires pour SystemVolumeService (récupération du volume système Windows).
/// IMPORTANT : Ces tests sont des tests d'intégration car ils interagissent avec le système audio Windows réel.
/// </summary>
public class SystemVolumeServiceTests : IDisposable
{
    private readonly Mock<ILogger> _loggerMock;
    private SystemVolumeService? _service;

    public SystemVolumeServiceTests()
    {
        _loggerMock = new Mock<ILogger>();
    }

    public void Dispose()
    {
        _service?.Dispose();
    }

    #region Tests initialisation

    [Fact]
    public async Task InitializeAsync_SucceedsWithDefaultDevice()
    {
        // Arrange
        _service = new SystemVolumeService(_loggerMock.Object);

        // Act
        await _service.InitializeAsync();

        // Assert : GetCurrentVolume devrait retourner une valeur valide
        float volume = _service.GetCurrentVolume();
        volume.Should().BeInRange(0.0f, 1.0f, "Le volume devrait être entre 0.0 et 1.0");
    }

    [Fact]
    public async Task InitializeAsync_LogsVolumeInformation()
    {
        // Arrange
        _service = new SystemVolumeService(_loggerMock.Object);

        // Act
        await _service.InitializeAsync();

        // Assert : un log Information devrait être émis
        _loggerMock.Verify(
            x => x.Information(
                It.Is<string>(s => s.Contains("SystemVolumeService initialisé")),
                It.IsAny<float>(),
                It.IsAny<float>()),
            Times.Once,
            "Un log Information devrait être émis après l'initialisation");
    }

    #endregion

    #region Tests GetCurrentVolume

    [Fact]
    public async Task GetCurrentVolume_ReturnsValueBetween0And1()
    {
        // Arrange
        _service = new SystemVolumeService(_loggerMock.Object);
        await _service.InitializeAsync();

        // Act
        float volume = _service.GetCurrentVolume();

        // Assert
        volume.Should().BeInRange(0.0f, 1.0f, "Le volume système devrait être entre 0.0 (muet) et 1.0 (100%)");
    }

    [Fact]
    public void GetCurrentVolume_WhenNotInitialized_ReturnsFallback()
    {
        // Arrange : service non initialisé
        _service = new SystemVolumeService(_loggerMock.Object);

        // Act
        float volume = _service.GetCurrentVolume();

        // Assert : devrait retourner 1.0 (fallback)
        volume.Should().Be(1.0f, "Sans initialisation, le fallback devrait être 1.0 (100%)");

        // Vérifier qu'un warning a été loggé
        _loggerMock.Verify(
            x => x.Warning(It.Is<string>(s => s.Contains("non initialisé"))),
            Times.Once,
            "Un warning devrait être loggé si le service n'est pas initialisé");
    }

    #endregion

    #region Tests GetVolumeDb

    [Fact]
    public async Task GetVolumeDb_ReturnsValueBetweenMinus96And0()
    {
        // Arrange
        _service = new SystemVolumeService(_loggerMock.Object);
        await _service.InitializeAsync();

        // Act
        float volumeDb = _service.GetVolumeDb();

        // Assert
        volumeDb.Should().BeInRange(-96.0f, 0.0f,
            "Le volume en dB devrait être entre -96 dB (muet) et 0 dB (100%)");
    }

    [Fact]
    public void GetVolumeDb_WhenNotInitialized_ReturnsFallback()
    {
        // Arrange : service non initialisé
        _service = new SystemVolumeService(_loggerMock.Object);

        // Act
        float volumeDb = _service.GetVolumeDb();

        // Assert : devrait retourner 0.0 dB (fallback)
        volumeDb.Should().Be(0.0f, "Sans initialisation, le fallback devrait être 0.0 dB (100%)");

        // Vérifier qu'un warning a été loggé
        _loggerMock.Verify(
            x => x.Warning(It.Is<string>(s => s.Contains("non initialisé"))),
            Times.Once,
            "Un warning devrait être loggé si le service n'est pas initialisé");
    }

    [Fact]
    public async Task GetVolumeDb_ConsistentWithScalarVolume()
    {
        // Arrange
        _service = new SystemVolumeService(_loggerMock.Object);
        await _service.InitializeAsync();

        // Act
        float volumeScalar = _service.GetCurrentVolume();
        float volumeDb = _service.GetVolumeDb();

        // Assert : vérifier la cohérence entre volumeScalar et volumeDb
        // À 100% (scalar = 1.0), volumeDb devrait être proche de 0 dB
        // À 50% (scalar = 0.5), volumeDb devrait être négatif
        if (volumeScalar >= 0.99f)
        {
            volumeDb.Should().BeInRange(-3.0f, 0.0f,
                "À volume quasi maximal (>99%), volumeDb devrait être proche de 0 dB");
        }
        else if (volumeScalar <= 0.01f)
        {
            volumeDb.Should().BeLessThan(-40.0f,
                "À volume quasi minimal (<1%), volumeDb devrait être très négatif");
        }
    }

    #endregion

    #region Tests événements

    [Fact(Skip = "Test manuel car nécessite une modification du volume système pendant l'exécution")]
    public async Task VolumeChanged_Event_TriggeredWhenVolumeChanges()
    {
        // Arrange
        _service = new SystemVolumeService(_loggerMock.Object);
        await _service.InitializeAsync();

        bool eventFired = false;
        float? newVolume = null;
        float? newVolumeDb = null;

        _service.VolumeChanged += (sender, args) =>
        {
            eventFired = true;
            newVolume = args.Volume;
            newVolumeDb = args.VolumeDb;
        };

        // Act : (Manuel) Changer le volume système Windows

        // Assert
        // NOTE : Ce test doit être exécuté manuellement en changeant le volume Windows
        // pendant son exécution. Il est marqué Skip pour ne pas bloquer la suite automatique.
        eventFired.Should().BeTrue("L'événement VolumeChanged devrait être déclenché");
        newVolume.Should().NotBeNull();
        newVolumeDb.Should().NotBeNull();
    }

    #endregion

    #region Tests Dispose

    [Fact]
    public async Task Dispose_ReleasesResources()
    {
        // Arrange
        _service = new SystemVolumeService(_loggerMock.Object);
        await _service.InitializeAsync();

        // Act
        _service.Dispose();

        // Assert : après Dispose, GetCurrentVolume devrait retourner le fallback
        float volume = _service.GetCurrentVolume();
        volume.Should().Be(1.0f, "Après Dispose, le service devrait retourner le fallback");

        // Vérifier qu'un log "disposé" a été émis
        _loggerMock.Verify(
            x => x.Information(It.Is<string>(s => s.Contains("disposé"))),
            Times.Once,
            "Un log devrait confirmer que le service a été disposé");
    }

    [Fact]
    public void Dispose_Multiple_DoesNotThrow()
    {
        // Arrange
        _service = new SystemVolumeService(_loggerMock.Object);

        // Act & Assert : appeler Dispose plusieurs fois ne devrait pas planter
        Action act = () =>
        {
            _service.Dispose();
            _service.Dispose();
            _service.Dispose();
        };

        act.Should().NotThrow("Dispose devrait être idempotent");
    }

    #endregion
}
