using ApplAudition.Services;
using FluentAssertions;
using Moq;
using Serilog;
using Xunit;

namespace ApplAudition.Tests.Services;

/// <summary>
/// Tests unitaires pour LeqCalculator (niveau équivalent continu).
/// Vérifie le calcul Leq (moyenne logarithmique), le pic et le buffer circulaire.
/// </summary>
public class LeqCalculatorTests
{
    private readonly Mock<ILogger> _loggerMock;
    private const float TOLERANCE_DB = 0.5f;
    private const int DURATION_SECONDS = 60;
    private const int UPDATE_INTERVAL_MS = 125;

    public LeqCalculatorTests()
    {
        _loggerMock = new Mock<ILogger>();
    }

    #region Tests Leq avec signal constant

    [Fact]
    public void GetLeq_WithConstantSignal_ReturnsConstantLevel()
    {
        // Arrange : ajouter plusieurs échantillons identiques
        var calculator = new LeqCalculator(_loggerMock.Object, DURATION_SECONDS, UPDATE_INTERVAL_MS);

        float constantDbfs = -20.0f;

        // Ajouter 100 échantillons identiques
        for (int i = 0; i < 100; i++)
        {
            calculator.AddSample(constantDbfs);
        }

        // Act
        float leq = calculator.GetLeq();

        // Assert : Leq devrait être égal au niveau constant
        leq.Should().BeApproximately(constantDbfs, TOLERANCE_DB,
            "Leq d'un signal constant devrait être égal au niveau constant");
    }

    [Theory]
    [InlineData(-10.0f)]
    [InlineData(-30.0f)]
    [InlineData(-60.0f)]
    public void GetLeq_WithVariousConstantLevels_ReturnsCorrectLeq(float constantDbfs)
    {
        // Arrange
        var calculator = new LeqCalculator(_loggerMock.Object, DURATION_SECONDS, UPDATE_INTERVAL_MS);

        for (int i = 0; i < 50; i++)
        {
            calculator.AddSample(constantDbfs);
        }

        // Act
        float leq = calculator.GetLeq();

        // Assert
        leq.Should().BeApproximately(constantDbfs, TOLERANCE_DB);
    }

    #endregion

    #region Tests Leq avec signal variable

    [Fact]
    public void GetLeq_WithVariableSignal_ReturnsLogarithmicMean()
    {
        // Arrange : alterner entre deux niveaux
        var calculator = new LeqCalculator(_loggerMock.Object, DURATION_SECONDS, UPDATE_INTERVAL_MS);

        float level1 = -10.0f; // Niveau élevé
        float level2 = -40.0f; // Niveau faible

        // Ajouter 50% à -10 dB, 50% à -40 dB
        for (int i = 0; i < 50; i++)
        {
            calculator.AddSample(level1);
            calculator.AddSample(level2);
        }

        // Act
        float leq = calculator.GetLeq();

        // Calculer Leq théorique : 10 * log10(mean(10^(Li/10)))
        // mean(10^(-10/10), 10^(-40/10)) = mean(0.1, 0.0001) = 0.05005
        // Leq = 10 * log10(0.05005) ≈ -13.0 dB
        float expectedLeq = 10.0f * (float)Math.Log10(
            (Math.Pow(10, level1 / 10.0) + Math.Pow(10, level2 / 10.0)) / 2.0);

        // Assert
        leq.Should().BeApproximately(expectedLeq, TOLERANCE_DB,
            "Leq devrait être la moyenne logarithmique énergétique");
    }

    [Fact]
    public void GetLeq_WithIncreasingSignal_ReflectsIncrease()
    {
        // Arrange : signal qui augmente progressivement
        var calculator = new LeqCalculator(_loggerMock.Object, DURATION_SECONDS, UPDATE_INTERVAL_MS);

        // Ajouter échantillons croissants
        for (int i = 0; i < 100; i++)
        {
            float dbfs = -50.0f + (i * 0.3f); // De -50 dB à -20 dB
            calculator.AddSample(dbfs);
        }

        // Act
        float leq = calculator.GetLeq();

        // Assert : Leq devrait être entre -50 et -20
        leq.Should().BeInRange(-50.0f, -20.0f,
            "Leq d'un signal croissant devrait être dans la plage des valeurs");

        // Leq devrait être plus proche du milieu (biaisé vers les valeurs élevées car logarithmique)
        leq.Should().BeGreaterThan(-40.0f,
            "Leq devrait être biaisé vers les niveaux élevés (moyenne énergétique)");
    }

    #endregion

    #region Tests Peak

    [Fact]
    public void GetPeak_ReturnsMaximumValue()
    {
        // Arrange
        var calculator = new LeqCalculator(_loggerMock.Object, DURATION_SECONDS, UPDATE_INTERVAL_MS);

        float[] samples = { -30.0f, -20.0f, -50.0f, -10.0f, -40.0f };

        foreach (float sample in samples)
        {
            calculator.AddSample(sample);
        }

        // Act
        float peak = calculator.GetPeak();

        // Assert : pic devrait être -10.0 (valeur max)
        peak.Should().Be(-10.0f, "Peak devrait retourner la valeur maximale du buffer");
    }

    [Fact]
    public void GetPeak_WithOneSample_ReturnsThatSample()
    {
        // Arrange
        var calculator = new LeqCalculator(_loggerMock.Object, DURATION_SECONDS, UPDATE_INTERVAL_MS);

        calculator.AddSample(-25.0f);

        // Act
        float peak = calculator.GetPeak();

        // Assert
        peak.Should().Be(-25.0f);
    }

    #endregion

    #region Tests buffer circulaire

    [Fact]
    public void AddSample_FillsBufferCircularly()
    {
        // Arrange : buffer de 10 secondes, 100 échantillons max
        var calculator = new LeqCalculator(_loggerMock.Object, durationSeconds: 10, updateIntervalMs: 100);

        int bufferCapacity = (10 * 1000) / 100; // 100 échantillons

        // Act : ajouter plus d'échantillons que la capacité
        for (int i = 0; i < bufferCapacity + 50; i++)
        {
            calculator.AddSample(-20.0f);
        }

        // Assert : le compteur d'échantillons ne devrait pas dépasser la capacité
        int sampleCount = calculator.GetSampleCount();
        sampleCount.Should().Be(bufferCapacity,
            "Buffer circulaire ne devrait pas dépasser sa capacité");
    }

    [Fact]
    public void AddSample_OverwritesOldestSamples()
    {
        // Arrange : petit buffer (5 échantillons pour test)
        var calculator = new LeqCalculator(_loggerMock.Object, durationSeconds: 5, updateIntervalMs: 1000);

        // Ajouter 5 échantillons à -30 dB
        for (int i = 0; i < 5; i++)
        {
            calculator.AddSample(-30.0f);
        }

        float leqBefore = calculator.GetLeq();

        // Act : ajouter 5 nouveaux échantillons à -10 dB (écrase les anciens)
        for (int i = 0; i < 5; i++)
        {
            calculator.AddSample(-10.0f);
        }

        float leqAfter = calculator.GetLeq();

        // Assert : Leq devrait maintenant refléter uniquement les nouveaux échantillons (-10 dB)
        leqBefore.Should().BeApproximately(-30.0f, TOLERANCE_DB);
        leqAfter.Should().BeApproximately(-10.0f, TOLERANCE_DB,
            "Après écrasement du buffer, Leq devrait refléter les nouveaux échantillons");
    }

    #endregion

    #region Tests Reset

    [Fact]
    public void Reset_ClearsBuffer()
    {
        // Arrange : ajouter des échantillons
        var calculator = new LeqCalculator(_loggerMock.Object, DURATION_SECONDS, UPDATE_INTERVAL_MS);

        for (int i = 0; i < 100; i++)
        {
            calculator.AddSample(-20.0f);
        }

        // Act : reset
        calculator.Reset();

        // Assert
        calculator.GetSampleCount().Should().Be(0, "Après reset, le buffer devrait être vide");
        calculator.GetLeq().Should().Be(-120.0f, "Après reset, Leq devrait être au plancher");
        calculator.GetPeak().Should().Be(-120.0f, "Après reset, Peak devrait être au plancher");
    }

    [Fact]
    public void Reset_AllowsNewSamples()
    {
        // Arrange
        var calculator = new LeqCalculator(_loggerMock.Object, DURATION_SECONDS, UPDATE_INTERVAL_MS);

        for (int i = 0; i < 50; i++)
        {
            calculator.AddSample(-30.0f);
        }

        calculator.Reset();

        // Act : ajouter nouveaux échantillons après reset
        for (int i = 0; i < 50; i++)
        {
            calculator.AddSample(-15.0f);
        }

        // Assert
        calculator.GetSampleCount().Should().Be(50);
        calculator.GetLeq().Should().BeApproximately(-15.0f, TOLERANCE_DB,
            "Après reset, Leq devrait refléter uniquement les nouveaux échantillons");
    }

    #endregion

    #region Tests edge cases

    [Fact]
    public void GetLeq_WithEmptyBuffer_ReturnsFloor()
    {
        // Arrange : buffer vide
        var calculator = new LeqCalculator(_loggerMock.Object, DURATION_SECONDS, UPDATE_INTERVAL_MS);

        // Act
        float leq = calculator.GetLeq();

        // Assert
        leq.Should().Be(-120.0f, "Leq d'un buffer vide devrait être au plancher -120 dB");
    }

    [Fact]
    public void GetPeak_WithEmptyBuffer_ReturnsFloor()
    {
        // Arrange
        var calculator = new LeqCalculator(_loggerMock.Object, DURATION_SECONDS, UPDATE_INTERVAL_MS);

        // Act
        float peak = calculator.GetPeak();

        // Assert
        peak.Should().Be(-120.0f, "Peak d'un buffer vide devrait être au plancher");
    }

    [Fact]
    public void GetSampleCount_InitiallyZero()
    {
        // Arrange
        var calculator = new LeqCalculator(_loggerMock.Object, DURATION_SECONDS, UPDATE_INTERVAL_MS);

        // Act
        int count = calculator.GetSampleCount();

        // Assert
        count.Should().Be(0, "Compteur initial devrait être 0");
    }

    [Fact]
    public void GetSampleCount_IncrementsWithSamples()
    {
        // Arrange
        var calculator = new LeqCalculator(_loggerMock.Object, DURATION_SECONDS, UPDATE_INTERVAL_MS);

        // Act
        for (int i = 0; i < 42; i++)
        {
            calculator.AddSample(-20.0f);
        }

        // Assert
        calculator.GetSampleCount().Should().Be(42, "Compteur devrait incrémenter avec chaque échantillon");
    }

    #endregion

    #region Tests thread-safety (optionnel)

    [Fact]
    public void AddSample_FromMultipleThreads_IsThreadSafe()
    {
        // Arrange
        var calculator = new LeqCalculator(_loggerMock.Object, DURATION_SECONDS, UPDATE_INTERVAL_MS);

        int numThreads = 10;
        int samplesPerThread = 100;

        // Act : ajouter échantillons depuis plusieurs threads simultanément
        var tasks = new List<Task>();

        for (int t = 0; t < numThreads; t++)
        {
            tasks.Add(Task.Run(() =>
            {
                for (int i = 0; i < samplesPerThread; i++)
                {
                    calculator.AddSample(-20.0f);
                }
            }));
        }

        Task.WaitAll(tasks.ToArray());

        // Assert : tous les échantillons devraient être ajoutés
        // (limité à la capacité du buffer)
        int totalSamples = numThreads * samplesPerThread;
        int bufferCapacity = (DURATION_SECONDS * 1000) / UPDATE_INTERVAL_MS;

        int expectedCount = Math.Min(totalSamples, bufferCapacity);

        calculator.GetSampleCount().Should().Be(expectedCount,
            "Tous les échantillons devraient être correctement ajoutés (thread-safe)");
    }

    #endregion

    #region Tests durée et intervalle configurables

    [Fact]
    public void Constructor_WithCustomDuration_CreatesCorrectBufferSize()
    {
        // Arrange & Act : buffer de 30 secondes
        var calculator = new LeqCalculator(_loggerMock.Object, durationSeconds: 30, updateIntervalMs: 250);

        // Capacité attendue = 30s * 1000ms / 250ms = 120 échantillons
        int expectedCapacity = (30 * 1000) / 250;

        // Remplir le buffer
        for (int i = 0; i < expectedCapacity + 10; i++)
        {
            calculator.AddSample(-20.0f);
        }

        // Assert
        calculator.GetSampleCount().Should().Be(expectedCapacity,
            "Buffer devrait avoir la capacité configurée");
    }

    #endregion
}
