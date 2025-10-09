using ApplAudition.Models;
using ApplAudition.Services;
using FluentAssertions;
using Moq;
using Serilog;
using Xunit;

namespace ApplAudition.Tests.Services;

/// <summary>
/// Tests unitaires pour EstimationModeManager (version simplifiée - Mode A uniquement).
/// Vérifie le calcul SPL estimé avec la formule : SPL_est = dBFS + volume_système_dB + 120.
/// </summary>
public class EstimationModeManagerTests
{
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<ISystemVolumeService> _systemVolumeServiceMock;

    public EstimationModeManagerTests()
    {
        _loggerMock = new Mock<ILogger>();
        _systemVolumeServiceMock = new Mock<ISystemVolumeService>();

        // Configuration par défaut du volume système : 100% (0 dB)
        _systemVolumeServiceMock.Setup(s => s.GetCurrentVolume()).Returns(1.0f);
        _systemVolumeServiceMock.Setup(s => s.GetVolumeDb()).Returns(0.0f);
    }

    #region Tests initialisation

    [Fact]
    public void Initialize_SetsCorrectDefaults()
    {
        // Arrange
        var manager = new EstimationModeManager(
            _systemVolumeServiceMock.Object,
            _loggerMock.Object);

        // Act
        manager.Initialize();

        // Assert
        manager.CurrentMode.Should().Be(EstimationMode.ModeA, "Devrait toujours être en Mode A");
        manager.CurrentProfile.Should().BeNull("Pas de profils dans la version simplifiée");
        manager.IsForcedModeA.Should().BeFalse("Pas de mode forcé dans la version simplifiée");
        manager.IsCalibrated.Should().BeFalse("Pas de calibration dans la version simplifiée");
        manager.CalibrationConstantC.Should().BeNull("Pas de constante de calibration");
    }

    #endregion

    #region Tests EstimateSpl

    [Fact]
    public void EstimateSpl_WithVolumeMax_CalculatesCorrectly()
    {
        // Arrange : Volume système à 100% (0 dB)
        _systemVolumeServiceMock.Setup(s => s.GetVolumeDb()).Returns(0.0f);

        var manager = new EstimationModeManager(
            _systemVolumeServiceMock.Object,
            _loggerMock.Object);

        manager.Initialize();

        // Act : estimer SPL avec dBFS = -20
        float dbfs = -20.0f;
        float spl = manager.EstimateSpl(dbfs);

        // Assert : SPL = dBFS + volume_système_dB + 120
        // = -20 + 0 + 120 = 100 dB(A)
        float expectedSpl = dbfs + 0.0f + 120.0f;
        spl.Should().Be(expectedSpl, "EstimateSpl devrait retourner dBFS + volumeDb + 120");
    }

    [Fact]
    public void EstimateSpl_WithVolumeHalf_CalculatesCorrectly()
    {
        // Arrange : Volume système à 50% (environ -6 dB)
        _systemVolumeServiceMock.Setup(s => s.GetVolumeDb()).Returns(-6.0f);

        var manager = new EstimationModeManager(
            _systemVolumeServiceMock.Object,
            _loggerMock.Object);

        manager.Initialize();

        // Act : estimer SPL avec dBFS = -20
        float dbfs = -20.0f;
        float spl = manager.EstimateSpl(dbfs);

        // Assert : SPL = dBFS + volume_système_dB + 120
        // = -20 + (-6) + 120 = 94 dB(A)
        float expectedSpl = dbfs + (-6.0f) + 120.0f;
        spl.Should().Be(expectedSpl, "EstimateSpl devrait tenir compte du volume système");
    }

    [Theory]
    [InlineData(-10.0f, 0.0f, 110.0f)]   // dBFS=-10, volumeDb=0 → SPL=110
    [InlineData(-20.0f, 0.0f, 100.0f)]   // dBFS=-20, volumeDb=0 → SPL=100
    [InlineData(-30.0f, 0.0f, 90.0f)]    // dBFS=-30, volumeDb=0 → SPL=90
    [InlineData(-20.0f, -6.0f, 94.0f)]   // dBFS=-20, volumeDb=-6 → SPL=94
    [InlineData(-20.0f, -12.0f, 88.0f)]  // dBFS=-20, volumeDb=-12 → SPL=88
    public void EstimateSpl_VariousValues_CalculatesCorrectly(float dbfs, float volumeDb, float expectedSpl)
    {
        // Arrange
        _systemVolumeServiceMock.Setup(s => s.GetVolumeDb()).Returns(volumeDb);

        var manager = new EstimationModeManager(
            _systemVolumeServiceMock.Object,
            _loggerMock.Object);

        manager.Initialize();

        // Act
        float spl = manager.EstimateSpl(dbfs);

        // Assert : SPL = dBFS + volumeDb + 120
        spl.Should().Be(expectedSpl);
    }

    #endregion

    #region Tests méthodes obsolètes

    [Fact]
    public void SetForceModeA_DoesNothing()
    {
        // Arrange
        var manager = new EstimationModeManager(
            _systemVolumeServiceMock.Object,
            _loggerMock.Object);

        manager.Initialize();

        // Act : appeler SetForceModeA (devrait être ignoré)
        manager.SetForceModeA(true);

        // Assert : devrait toujours être en Mode A
        manager.CurrentMode.Should().Be(EstimationMode.ModeA);
        manager.IsForcedModeA.Should().BeFalse();
    }

    [Fact]
    public async Task SetCalibrationConstantAsync_DoesNothing()
    {
        // Arrange
        var manager = new EstimationModeManager(
            _systemVolumeServiceMock.Object,
            _loggerMock.Object);

        manager.Initialize();

        // Act : appeler SetCalibrationConstantAsync (devrait être ignoré)
        await manager.SetCalibrationConstantAsync(-18.0f);

        // Assert : devrait toujours être non calibré
        manager.IsCalibrated.Should().BeFalse();
        manager.CalibrationConstantC.Should().BeNull();
    }

    [Fact]
    public async Task ResetCalibrationAsync_DoesNothing()
    {
        // Arrange
        var manager = new EstimationModeManager(
            _systemVolumeServiceMock.Object,
            _loggerMock.Object);

        manager.Initialize();

        // Act : appeler ResetCalibrationAsync (devrait être ignoré)
        await manager.ResetCalibrationAsync();

        // Assert : devrait toujours être non calibré
        manager.IsCalibrated.Should().BeFalse();
    }

    #endregion
}
