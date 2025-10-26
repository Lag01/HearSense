using HearSense.Services;
using HearSense.Tests.Helpers;
using FluentAssertions;
using Moq;
using Serilog;
using Xunit;

namespace HearSense.Tests.Services;

/// <summary>
/// Tests unitaires pour AWeightingFilter (filtre de pondération A IEC 61672:2003).
/// Vérifie l'atténuation correcte des fréquences selon la courbe de pondération A.
/// </summary>
public class AWeightingFilterTests
{
    private readonly Mock<ILogger> _loggerMock;
    private const int SAMPLE_RATE = 48000;
    private const float TOLERANCE_DB = 2.0f; // Tolérance ±2 dB (spec CLAUDE.md)

    public AWeightingFilterTests()
    {
        _loggerMock = new Mock<ILogger>();
    }

    #region Tests pondération A - Fréquences de référence

    [Fact]
    public void ApplyFilter_At1kHz_MinimalAttenuation()
    {
        // Arrange : 1 kHz est la fréquence de référence (0 dB d'atténuation théorique)
        var filter = new AWeightingFilter(_loggerMock.Object, SAMPLE_RATE);

        float[] signal = SignalGenerator.GenerateSineWave(
            frequency: 1000,
            sampleRate: SAMPLE_RATE,
            durationSeconds: 0.5, // 500 ms pour stabilisation du filtre
            amplitude: 1.0);

        // Calculer RMS avant filtrage
        float rmsBefore = SignalGenerator.CalculateRmsSimple(signal);

        // Act
        filter.ApplyFilter(signal);

        // Calculer RMS après filtrage
        float rmsAfter = SignalGenerator.CalculateRmsSimple(signal);

        // Calculer atténuation en dB : 20*log10(rmsAfter / rmsBefore)
        float attenuationDb = 20.0f * (float)Math.Log10(rmsAfter / rmsBefore);

        // Assert : à 1 kHz, atténuation devrait être proche de 0 dB
        attenuationDb.Should().BeInRange(-TOLERANCE_DB, TOLERANCE_DB,
            "À 1 kHz (référence), la pondération A devrait être ≈ 0 dB");
    }

    [Fact]
    public void ApplyFilter_At100Hz_StrongAttenuation()
    {
        // Arrange : 100 Hz devrait avoir forte atténuation (≈ -19 dB théorique)
        var filter = new AWeightingFilter(_loggerMock.Object, SAMPLE_RATE);

        float[] signal = SignalGenerator.GenerateSineWave(
            frequency: 100,
            sampleRate: SAMPLE_RATE,
            durationSeconds: 0.5,
            amplitude: 1.0);

        float rmsBefore = SignalGenerator.CalculateRmsSimple(signal);

        // Act
        filter.ApplyFilter(signal);

        float rmsAfter = SignalGenerator.CalculateRmsSimple(signal);
        float attenuationDb = 20.0f * (float)Math.Log10(rmsAfter / rmsBefore);

        // Assert : à 100 Hz, atténuation devrait être ≈ -19 dB (±2 dB)
        // Pondération A théorique à 100 Hz = -19.1 dB
        attenuationDb.Should().BeInRange(-21.0f, -17.0f,
            "À 100 Hz, la pondération A devrait être ≈ -19 dB (forte atténuation basses fréquences)");
    }

    [Fact]
    public void ApplyFilter_At10kHz_ModerateAttenuation()
    {
        // Arrange : 10 kHz devrait avoir atténuation modérée (≈ -4.3 dB théorique)
        var filter = new AWeightingFilter(_loggerMock.Object, SAMPLE_RATE);

        float[] signal = SignalGenerator.GenerateSineWave(
            frequency: 10000,
            sampleRate: SAMPLE_RATE,
            durationSeconds: 0.5,
            amplitude: 1.0);

        float rmsBefore = SignalGenerator.CalculateRmsSimple(signal);

        // Act
        filter.ApplyFilter(signal);

        float rmsAfter = SignalGenerator.CalculateRmsSimple(signal);
        float attenuationDb = 20.0f * (float)Math.Log10(rmsAfter / rmsBefore);

        // Assert : à 10 kHz, atténuation devrait être ≈ -4.3 dB (±2 dB)
        attenuationDb.Should().BeInRange(-6.3f, -2.3f,
            "À 10 kHz, la pondération A devrait être ≈ -4.3 dB (atténuation modérée hautes fréquences)");
    }

    [Fact]
    public void ApplyFilter_At50Hz_VeryStrongAttenuation()
    {
        // Arrange : 50 Hz devrait avoir très forte atténuation (≈ -30.2 dB théorique)
        var filter = new AWeightingFilter(_loggerMock.Object, SAMPLE_RATE);

        float[] signal = SignalGenerator.GenerateSineWave(
            frequency: 50,
            sampleRate: SAMPLE_RATE,
            durationSeconds: 0.5,
            amplitude: 1.0);

        float rmsBefore = SignalGenerator.CalculateRmsSimple(signal);

        // Act
        filter.ApplyFilter(signal);

        float rmsAfter = SignalGenerator.CalculateRmsSimple(signal);
        float attenuationDb = 20.0f * (float)Math.Log10(rmsAfter / rmsBefore);

        // Assert : à 50 Hz, atténuation devrait être ≈ -30 dB (±3 dB)
        attenuationDb.Should().BeInRange(-33.0f, -27.0f,
            "À 50 Hz, la pondération A devrait être ≈ -30 dB (très forte atténuation)");
    }

    [Fact]
    public void ApplyFilter_At4kHz_SlightAmplification()
    {
        // Arrange : 4 kHz est proche du pic de sensibilité (≈ +1 dB théorique)
        var filter = new AWeightingFilter(_loggerMock.Object, SAMPLE_RATE);

        float[] signal = SignalGenerator.GenerateSineWave(
            frequency: 4000,
            sampleRate: SAMPLE_RATE,
            durationSeconds: 0.5,
            amplitude: 1.0);

        float rmsBefore = SignalGenerator.CalculateRmsSimple(signal);

        // Act
        filter.ApplyFilter(signal);

        float rmsAfter = SignalGenerator.CalculateRmsSimple(signal);
        float attenuationDb = 20.0f * (float)Math.Log10(rmsAfter / rmsBefore);

        // Assert : à 4 kHz, légère amplification (≈ +1 dB)
        attenuationDb.Should().BeInRange(-1.0f, 3.0f,
            "À 4 kHz (pic de sensibilité), la pondération A devrait être ≈ +1 dB");
    }

    #endregion

    #region Tests Reset

    [Fact]
    public void Reset_ClearsFilterState()
    {
        // Arrange : filtrer un signal, puis reset
        var filter = new AWeightingFilter(_loggerMock.Object, SAMPLE_RATE);

        float[] signal1 = SignalGenerator.GenerateSineWave(1000, SAMPLE_RATE, 0.5, 1.0);
        filter.ApplyFilter(signal1);

        // Act : reset
        filter.Reset();

        // Filtrer un nouveau signal identique
        float[] signal2 = SignalGenerator.GenerateSineWave(1000, SAMPLE_RATE, 0.5, 1.0);
        filter.ApplyFilter(signal2);

        // Assert : les deux signaux devraient être identiques après reset
        // (pas d'état résiduel du filtre)
        float rms1 = SignalGenerator.CalculateRmsSimple(signal1);
        float rms2 = SignalGenerator.CalculateRmsSimple(signal2);

        rms2.Should().BeApproximately(rms1, rms1 * 0.01f,
            "Après reset, le filtre devrait produire le même résultat pour le même signal");
    }

    #endregion

    #region Tests edge cases

    [Fact]
    public void ApplyFilter_WithEmptyBuffer_DoesNotCrash()
    {
        // Arrange
        var filter = new AWeightingFilter(_loggerMock.Object, SAMPLE_RATE);
        float[] emptyBuffer = Array.Empty<float>();

        // Act & Assert : ne devrait pas crasher
        Action act = () => filter.ApplyFilter(emptyBuffer);
        act.Should().NotThrow("ApplyFilter devrait gérer un buffer vide sans crasher");
    }

    [Fact]
    public void ApplyFilter_WithNullBuffer_DoesNotCrash()
    {
        // Arrange
        var filter = new AWeightingFilter(_loggerMock.Object, SAMPLE_RATE);

        // Act & Assert
        Action act = () => filter.ApplyFilter(null!);
        act.Should().NotThrow("ApplyFilter devrait gérer un buffer null sans crasher");
    }

    [Fact]
    public void ApplyFilter_WithSilence_OutputIsSilence()
    {
        // Arrange : signal de silence
        var filter = new AWeightingFilter(_loggerMock.Object, SAMPLE_RATE);
        float[] silence = SignalGenerator.GenerateConstant(6000, 0.0f);

        // Act
        filter.ApplyFilter(silence);

        // Assert : sortie devrait toujours être silence
        float rms = SignalGenerator.CalculateRmsSimple(silence);
        rms.Should().Be(0.0f, "Filtrer du silence devrait donner du silence");
    }

    #endregion

    #region Tests multiples appels (continuité du filtre)

    [Fact]
    public void ApplyFilter_MultipleBuffers_MaintainsFilterState()
    {
        // Arrange : filtrer plusieurs buffers consécutifs
        // Le filtre doit maintenir son état entre les appels
        var filter = new AWeightingFilter(_loggerMock.Object, SAMPLE_RATE);

        // Créer un long signal et le diviser en chunks
        float[] longSignal = SignalGenerator.GenerateSineWave(1000, SAMPLE_RATE, 1.0, 1.0);

        int chunkSize = 6000; // 125 ms à 48 kHz
        int numChunks = longSignal.Length / chunkSize;

        List<float[]> filteredChunks = new();

        for (int i = 0; i < numChunks; i++)
        {
            float[] chunk = new float[chunkSize];
            Array.Copy(longSignal, i * chunkSize, chunk, 0, chunkSize);

            filter.ApplyFilter(chunk);
            filteredChunks.Add(chunk);
        }

        // Assert : les RMS des chunks devraient être similaires (filtre stabilisé)
        var rmsValues = filteredChunks.Select(SignalGenerator.CalculateRmsSimple).ToList();

        // Ignorer le premier chunk (transitoire de démarrage du filtre)
        var stableRms = rmsValues.Skip(1).ToList();

        float avgRms = stableRms.Average();
        float maxDeviation = stableRms.Max(r => Math.Abs(r - avgRms));

        // Déviation max devrait être < 10% de la moyenne
        maxDeviation.Should().BeLessThan(avgRms * 0.1f,
            "Les RMS des chunks devraient être stables après le transitoire initial");
    }

    #endregion

    #region Tests courbe complète (facultatif)

    [Theory]
    [InlineData(31.5, -39.4)] // 31.5 Hz → -39.4 dB
    [InlineData(63, -26.2)]   // 63 Hz → -26.2 dB
    [InlineData(125, -16.1)]  // 125 Hz → -16.1 dB
    [InlineData(250, -8.6)]   // 250 Hz → -8.6 dB
    [InlineData(500, -3.2)]   // 500 Hz → -3.2 dB
    [InlineData(2000, 1.2)]   // 2 kHz → +1.2 dB
    [InlineData(8000, 1.0)]   // 8 kHz → +1.0 dB
    [InlineData(16000, -6.6)] // 16 kHz → -6.6 dB
    public void ApplyFilter_VariousFrequencies_MatchesAWeightingCurve(double frequency, double expectedAttenuationDb)
    {
        // Arrange
        var filter = new AWeightingFilter(_loggerMock.Object, SAMPLE_RATE);

        float[] signal = SignalGenerator.GenerateSineWave(
            frequency: frequency,
            sampleRate: SAMPLE_RATE,
            durationSeconds: 0.5,
            amplitude: 1.0);

        float rmsBefore = SignalGenerator.CalculateRmsSimple(signal);

        // Act
        filter.ApplyFilter(signal);

        float rmsAfter = SignalGenerator.CalculateRmsSimple(signal);
        float actualAttenuationDb = 20.0f * (float)Math.Log10(rmsAfter / rmsBefore);

        // Assert : tolérance ±3 dB (généreuse pour test)
        actualAttenuationDb.Should().BeApproximately((float)expectedAttenuationDb, 3.0f,
            $"À {frequency} Hz, pondération A devrait être ≈ {expectedAttenuationDb} dB");
    }

    #endregion
}
