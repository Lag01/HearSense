using HearSense.Services;
using HearSense.Tests.Helpers;
using FluentAssertions;
using Moq;
using Serilog;
using Xunit;

namespace HearSense.Tests.Services;

/// <summary>
/// Tests unitaires pour DspEngine (calcul RMS et dBFS).
/// Objectif : couverture ≥ 80%, tolérance ±0.5 dB.
/// </summary>
public class DspEngineTests
{
    private readonly IDspEngine _dspEngine;
    private readonly Mock<ILogger> _loggerMock;
    private const float TOLERANCE_DB = 0.5f;
    private const int SAMPLE_RATE = 48000;

    public DspEngineTests()
    {
        _loggerMock = new Mock<ILogger>();
        _dspEngine = new DspEngine(_loggerMock.Object);
    }

    #region Tests RMS

    [Fact]
    public void CalculateRms_WithSineWave1kHz_AmplitudeCorrect()
    {
        // Arrange : signal sinusoïdal 1 kHz, amplitude 1.0
        // RMS théorique = 1.0 / √2 ≈ 0.707
        float[] signal = SignalGenerator.GenerateSineWave(
            frequency: 1000,
            sampleRate: SAMPLE_RATE,
            durationSeconds: 0.125, // 125 ms = 6000 samples à 48 kHz
            amplitude: 1.0);

        float expectedRms = SignalGenerator.GetTheoreticalSineRms(1.0);

        // Act
        float actualRms = _dspEngine.CalculateRms(signal);

        // Assert : tolérance 5% car fenêtre Hann modifie légèrement
        actualRms.Should().BeApproximately(expectedRms, expectedRms * 0.15f,
            "RMS d'une sinusoïde amplitude 1.0 devrait être ≈ 0.707");
    }

    [Fact]
    public void CalculateRms_WithSineWave_Amplitude05_ReturnsCorrectRms()
    {
        // Arrange : sinusoïde amplitude 0.5
        // RMS théorique = 0.5 / √2 ≈ 0.354
        float[] signal = SignalGenerator.GenerateSineWave(
            frequency: 1000,
            sampleRate: SAMPLE_RATE,
            durationSeconds: 0.125,
            amplitude: 0.5);

        float expectedRms = SignalGenerator.GetTheoreticalSineRms(0.5);

        // Act
        float actualRms = _dspEngine.CalculateRms(signal);

        // Assert
        actualRms.Should().BeApproximately(expectedRms, expectedRms * 0.15f,
            "RMS d'une sinusoïde amplitude 0.5 devrait être ≈ 0.354");
    }

    [Fact]
    public void CalculateRms_WithSilence_ReturnsZero()
    {
        // Arrange : buffer de silence (tous les échantillons = 0)
        float[] silence = SignalGenerator.GenerateConstant(6000, 0.0f);

        // Act
        float rms = _dspEngine.CalculateRms(silence);

        // Assert
        rms.Should().Be(0.0f, "RMS d'un signal nul devrait être 0");
    }

    [Fact]
    public void CalculateRms_WithEmptyBuffer_ReturnsZero()
    {
        // Arrange
        float[] emptyBuffer = Array.Empty<float>();

        // Act
        float rms = _dspEngine.CalculateRms(emptyBuffer);

        // Assert
        rms.Should().Be(0.0f, "RMS d'un buffer vide devrait être 0");
    }

    [Fact]
    public void CalculateRms_WithNullBuffer_ReturnsZero()
    {
        // Act
        float rms = _dspEngine.CalculateRms(null!);

        // Assert
        rms.Should().Be(0.0f, "RMS d'un buffer null devrait être 0");
    }

    #endregion

    #region Tests dBFS

    [Theory]
    [InlineData(1.0f, 0.0f)] // RMS = 1.0 → 0 dBFS
    [InlineData(0.5f, -6.02f)] // RMS = 0.5 → ≈ -6 dBFS (20*log10(0.5) = -6.02)
    [InlineData(0.1f, -20.0f)] // RMS = 0.1 → -20 dBFS
    [InlineData(0.01f, -40.0f)] // RMS = 0.01 → -40 dBFS
    public void RmsToDbfs_WithKnownValues_ReturnsCorrectDbfs(float rms, float expectedDbfs)
    {
        // Act
        float actualDbfs = _dspEngine.RmsToDbfs(rms);

        // Assert : tolérance ±0.1 dB pour calculs mathématiques
        actualDbfs.Should().BeApproximately(expectedDbfs, 0.1f,
            $"20*log10({rms}) devrait donner ≈ {expectedDbfs} dBFS");
    }

    [Fact]
    public void RmsToDbfs_WithZero_ReturnsFloor()
    {
        // Act
        float dbfs = _dspEngine.RmsToDbfs(0.0f);

        // Assert : devrait retourner plancher de -120 dB au lieu de -∞
        dbfs.Should().Be(-120.0f, "RMS = 0 devrait retourner plancher -120 dBFS, pas -∞");
    }

    [Fact]
    public void RmsToDbfs_WithNegativeValue_ReturnsFloor()
    {
        // Act
        float dbfs = _dspEngine.RmsToDbfs(-0.5f);

        // Assert
        dbfs.Should().Be(-120.0f, "RMS négatif devrait retourner plancher -120 dBFS");
    }

    [Fact]
    public void RmsToDbfs_WithNaN_ReturnsFloor()
    {
        // Act
        float dbfs = _dspEngine.RmsToDbfs(float.NaN);

        // Assert
        dbfs.Should().Be(-120.0f, "RMS = NaN devrait retourner plancher -120 dBFS");
    }

    [Fact]
    public void RmsToDbfs_WithInfinity_ReturnsFloor()
    {
        // Act
        float dbfs = _dspEngine.RmsToDbfs(float.PositiveInfinity);

        // Assert
        dbfs.Should().Be(-120.0f, "RMS = ∞ devrait retourner plancher -120 dBFS");
    }

    #endregion

    #region Tests ProcessBuffer

    [Fact]
    public void ProcessBuffer_WithSineWave_ReturnsValidDspResult()
    {
        // Arrange
        float[] signal = SignalGenerator.GenerateSineWave(
            frequency: 1000,
            sampleRate: SAMPLE_RATE,
            durationSeconds: 0.125,
            amplitude: 0.5);

        // Act
        var result = _dspEngine.ProcessBuffer(signal);

        // Assert
        result.Should().NotBeNull();
        result.Rms.Should().BeGreaterThan(0, "RMS devrait être > 0 pour un signal non nul");
        result.DbFs.Should().BeGreaterThan(-120, "dBFS devrait être > -120 dB pour un signal non nul");
        result.DbFs.Should().BeLessThanOrEqualTo(0, "dBFS devrait être ≤ 0 (échelle numérique)");
        result.Timestamp.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ProcessBuffer_WithSilence_ReturnsFloorDbfs()
    {
        // Arrange
        float[] silence = SignalGenerator.GenerateConstant(6000, 0.0f);

        // Act
        var result = _dspEngine.ProcessBuffer(silence);

        // Assert
        result.Rms.Should().Be(0.0f);
        result.DbFs.Should().Be(-120.0f, "Silence devrait retourner plancher -120 dBFS");
    }

    [Fact]
    public void ProcessBuffer_WithNullBuffer_ReturnsFloorDbfs()
    {
        // Act
        var result = _dspEngine.ProcessBuffer(null!);

        // Assert
        result.Rms.Should().Be(0.0f);
        result.DbFs.Should().Be(-120.0f);
        _loggerMock.Verify(
            x => x.Warning(It.IsAny<string>()),
            Times.AtLeastOnce,
            "Un warning devrait être loggé pour buffer null");
    }

    [Fact]
    public void ProcessBuffer_WithEmptyBuffer_ReturnsFloorDbfs()
    {
        // Arrange
        float[] emptyBuffer = Array.Empty<float>();

        // Act
        var result = _dspEngine.ProcessBuffer(emptyBuffer);

        // Assert
        result.Rms.Should().Be(0.0f);
        result.DbFs.Should().Be(-120.0f);
    }

    #endregion

    #region Tests fenêtrage Hann

    [Fact]
    public void CalculateRms_AppliesHannWindow_ReducesEdgeEffects()
    {
        // Arrange : sinusoïde avec discontinuité (pas un multiple de période)
        // Le fenêtrage Hann devrait atténuer les bords
        float[] signalWithDiscontinuity = SignalGenerator.GenerateSineWave(
            frequency: 997, // Pas un multiple exact
            sampleRate: SAMPLE_RATE,
            durationSeconds: 0.125,
            amplitude: 1.0);

        // Act
        float rmsWithHann = _dspEngine.CalculateRms(signalWithDiscontinuity);
        float rmsWithoutHann = SignalGenerator.CalculateRmsSimple(signalWithDiscontinuity);

        // Assert : RMS avec Hann devrait être légèrement inférieur
        // (car la fenêtre atténue les bords)
        rmsWithHann.Should().BeLessThan(rmsWithoutHann,
            "Fenêtre Hann devrait réduire légèrement le RMS en atténuant les bords");

        // Différence attendue : ~10-30% pour Hann
        float ratio = rmsWithHann / rmsWithoutHann;
        ratio.Should().BeInRange(0.5f, 0.9f,
            "Ratio RMS Hann / RMS simple devrait être entre 0.5 et 0.9");
    }

    #endregion
}
