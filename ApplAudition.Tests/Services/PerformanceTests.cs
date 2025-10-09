using ApplAudition.Services;
using ApplAudition.Tests.Helpers;
using FluentAssertions;
using Moq;
using Serilog;
using System.Diagnostics;
using Xunit;

namespace ApplAudition.Tests.Services;

/// <summary>
/// Tests de performance pour vérifier CPU et mémoire.
/// Objectif : CPU moyen < 10%, pas de memory leak.
/// </summary>
public class PerformanceTests
{
    private readonly Mock<ILogger> _loggerMock;
    private const int SAMPLE_RATE = 48000;
    private const int BUFFER_SIZE_MS = 125; // 125 ms
    private const int BUFFER_SIZE_SAMPLES = (SAMPLE_RATE * BUFFER_SIZE_MS) / 1000; // 6000 échantillons

    public PerformanceTests()
    {
        _loggerMock = new Mock<ILogger>();
    }

    #region Tests CPU

    [Fact]
    public void DspPipeline_CpuUsage_BelowThreshold()
    {
        // Arrange : simuler pipeline DSP complet
        var dspEngine = new DspEngine(_loggerMock.Object);
        var aWeightingFilter = new AWeightingFilter(_loggerMock.Object, SAMPLE_RATE);
        var leqCalculator = new LeqCalculator(_loggerMock.Object);

        // Durée du test : 5 minutes (300 secondes) pour test automatisé
        // (Au lieu de 30 min, pour ne pas ralentir les tests)
        int durationSeconds = 5; // Réduit pour tests rapides
        int iterations = (durationSeconds * 1000) / BUFFER_SIZE_MS;

        var process = Process.GetCurrentProcess();
        var startTime = process.TotalProcessorTime;
        var stopwatch = Stopwatch.StartNew();

        // Act : traiter flux audio simulé pendant 5 secondes
        for (int i = 0; i < iterations; i++)
        {
            // Générer buffer audio (bruit blanc simulé)
            float[] buffer = SignalGenerator.GenerateWhiteNoise(BUFFER_SIZE_SAMPLES, amplitude: 0.1, seed: i);

            // Pipeline DSP complet
            aWeightingFilter.ApplyFilter(buffer); // Pondération A
            var dspResult = dspEngine.ProcessBuffer(buffer); // RMS + dBFS
            leqCalculator.AddSample(dspResult.DbFs); // Leq

            // Simuler intervalle 125 ms (en réalité, ne pas attendre pour accélérer le test)
            // Thread.Sleep(BUFFER_SIZE_MS); // Commenté pour accélérer
        }

        stopwatch.Stop();

        var endTime = process.TotalProcessorTime;
        var cpuTimeUsed = (endTime - startTime).TotalMilliseconds;
        var totalRealTime = stopwatch.Elapsed.TotalMilliseconds;

        // Calculer % CPU : (CPU time / Real time) * 100 / nombre de cores
        int processorCount = Environment.ProcessorCount;
        double cpuUsagePercent = (cpuTimeUsed / totalRealTime) * 100.0 / processorCount;

        // Assert : CPU moyen devrait être < 10%
        cpuUsagePercent.Should().BeLessThan(10.0,
            "CPU usage du pipeline DSP devrait être < 10% en moyenne");

        // Output pour diagnostics
        Console.WriteLine($"CPU Usage: {cpuUsagePercent:F2}% (sur {processorCount} cores)");
        Console.WriteLine($"CPU Time: {cpuTimeUsed:F2} ms, Real Time: {totalRealTime:F2} ms");
    }

    [Fact]
    public void DspEngine_ProcessBuffer_PerformanceBenchmark()
    {
        // Arrange
        var dspEngine = new DspEngine(_loggerMock.Object);
        float[] buffer = SignalGenerator.GenerateSineWave(1000, SAMPLE_RATE, 0.125, 1.0);

        int iterations = 10000; // 10k itérations pour benchmark

        // Act : mesurer temps de traitement
        var stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < iterations; i++)
        {
            dspEngine.ProcessBuffer(buffer);
        }

        stopwatch.Stop();

        double averageTimeMs = stopwatch.Elapsed.TotalMilliseconds / iterations;

        // Assert : temps moyen devrait être < 1 ms par buffer (125 ms disponible en temps réel)
        averageTimeMs.Should().BeLessThan(1.0,
            "ProcessBuffer devrait traiter un buffer en < 1 ms (bien en dessous des 125 ms temps réel)");

        // Output pour diagnostics
        Console.WriteLine($"Average ProcessBuffer time: {averageTimeMs:F4} ms ({iterations} iterations)");
    }

    [Fact]
    public void AWeightingFilter_ApplyFilter_PerformanceBenchmark()
    {
        // Arrange
        var filter = new AWeightingFilter(_loggerMock.Object, SAMPLE_RATE);
        float[] buffer = SignalGenerator.GenerateSineWave(1000, SAMPLE_RATE, 0.125, 1.0);

        int iterations = 10000;

        // Act
        var stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < iterations; i++)
        {
            // Créer une copie pour éviter que le filtre ne modifie le buffer original
            float[] bufferCopy = (float[])buffer.Clone();
            filter.ApplyFilter(bufferCopy);
        }

        stopwatch.Stop();

        double averageTimeMs = stopwatch.Elapsed.TotalMilliseconds / iterations;

        // Assert : temps moyen devrait être < 1 ms
        averageTimeMs.Should().BeLessThan(1.0,
            "ApplyFilter devrait traiter un buffer en < 1 ms");

        Console.WriteLine($"Average ApplyFilter time: {averageTimeMs:F4} ms ({iterations} iterations)");
    }

    #endregion

    #region Tests mémoire

    [Fact]
    public void DspPipeline_NoMemoryLeak()
    {
        // Arrange
        var dspEngine = new DspEngine(_loggerMock.Object);
        var aWeightingFilter = new AWeightingFilter(_loggerMock.Object, SAMPLE_RATE);
        var leqCalculator = new LeqCalculator(_loggerMock.Object);

        // Forcer GC avant mesure
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        long memoryBefore = GC.GetTotalMemory(true);

        // Act : traiter 1000 buffers (environ 125 secondes simulées)
        int iterations = 1000;

        for (int i = 0; i < iterations; i++)
        {
            float[] buffer = SignalGenerator.GenerateWhiteNoise(BUFFER_SIZE_SAMPLES, amplitude: 0.1, seed: i);

            aWeightingFilter.ApplyFilter(buffer);
            var dspResult = dspEngine.ProcessBuffer(buffer);
            leqCalculator.AddSample(dspResult.DbFs);
        }

        // Forcer GC après traitement
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        long memoryAfter = GC.GetTotalMemory(true);

        long memoryDiff = memoryAfter - memoryBefore;

        // Assert : augmentation mémoire devrait être < 10 MB
        // (tolère buffer circulaire Leq qui est constant)
        const long maxMemoryIncreaseMB = 10;
        long maxMemoryIncreaseBytes = maxMemoryIncreaseMB * 1024 * 1024;

        memoryDiff.Should().BeLessThan(maxMemoryIncreaseBytes,
            "Pas de memory leak significatif détecté (< 10 MB après 1000 itérations)");

        // Output pour diagnostics
        Console.WriteLine($"Memory before: {memoryBefore / 1024.0 / 1024.0:F2} MB");
        Console.WriteLine($"Memory after: {memoryAfter / 1024.0 / 1024.0:F2} MB");
        Console.WriteLine($"Memory diff: {memoryDiff / 1024.0 / 1024.0:F2} MB");
    }

    [Fact]
    public void LeqCalculator_CircularBuffer_StableMemory()
    {
        // Arrange : buffer circulaire ne devrait pas croître indéfiniment
        var leqCalculator = new LeqCalculator(_loggerMock.Object, durationSeconds: 60, updateIntervalMs: 125);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        long memoryBefore = GC.GetTotalMemory(true);

        // Act : ajouter beaucoup d'échantillons (plus que la capacité du buffer)
        int bufferCapacity = (60 * 1000) / 125; // 480 échantillons
        int iterations = bufferCapacity * 10; // 10x la capacité

        for (int i = 0; i < iterations; i++)
        {
            leqCalculator.AddSample(-20.0f);
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        long memoryAfter = GC.GetTotalMemory(true);

        long memoryDiff = memoryAfter - memoryBefore;

        // Assert : mémoire devrait rester stable (buffer circulaire taille fixe)
        const long maxMemoryIncreaseMB = 5;
        long maxMemoryIncreaseBytes = maxMemoryIncreaseMB * 1024 * 1024;

        memoryDiff.Should().BeLessThan(maxMemoryIncreaseBytes,
            "Buffer circulaire ne devrait pas augmenter indéfiniment en mémoire");

        Console.WriteLine($"Memory diff (circular buffer): {memoryDiff / 1024.0 / 1024.0:F2} MB");
    }

    #endregion

    #region Tests stabilité (optionnel)

    [Fact]
    public void DspPipeline_LongRun_Stability()
    {
        // Arrange : test de stabilité sur longue durée (2 min simulées)
        var dspEngine = new DspEngine(_loggerMock.Object);
        var aWeightingFilter = new AWeightingFilter(_loggerMock.Object, SAMPLE_RATE);
        var leqCalculator = new LeqCalculator(_loggerMock.Object);

        int durationSeconds = 10; // 10 secondes pour test rapide
        int iterations = (durationSeconds * 1000) / BUFFER_SIZE_MS;

        // Act : traiter flux continu
        Exception? thrownException = null;

        try
        {
            for (int i = 0; i < iterations; i++)
            {
                float[] buffer = SignalGenerator.GenerateSineWave(
                    frequency: 1000 + (i % 100), // Varier légèrement la fréquence
                    sampleRate: SAMPLE_RATE,
                    durationSeconds: 0.125,
                    amplitude: 0.5);

                aWeightingFilter.ApplyFilter(buffer);
                var dspResult = dspEngine.ProcessBuffer(buffer);
                leqCalculator.AddSample(dspResult.DbFs);

                // Vérifier valeurs cohérentes
                dspResult.DbFs.Should().BeGreaterThan(-120.0f);
                dspResult.DbFs.Should().BeLessThanOrEqualTo(0.0f);
            }
        }
        catch (Exception ex)
        {
            thrownException = ex;
        }

        // Assert : aucune exception durant l'exécution
        thrownException.Should().BeNull("Pipeline devrait être stable sur longue durée");

        // Vérifier Leq final cohérent
        float leq = leqCalculator.GetLeq();
        leq.Should().BeGreaterThan(-120.0f, "Leq devrait être cohérent après longue exécution");
        leq.Should().BeLessThanOrEqualTo(0.0f);
    }

    #endregion

    #region Tests throughput

    [Fact]
    public void DspPipeline_Throughput_MeetsRealTimeRequirements()
    {
        // Arrange : vérifier que le pipeline peut traiter en temps réel (8 buffers/sec = 125 ms/buffer)
        var dspEngine = new DspEngine(_loggerMock.Object);
        var aWeightingFilter = new AWeightingFilter(_loggerMock.Object, SAMPLE_RATE);
        var leqCalculator = new LeqCalculator(_loggerMock.Object);

        int buffersPerSecond = 8; // 125 ms/buffer = 8 buffers/sec
        int testDurationSeconds = 5;
        int totalBuffers = buffersPerSecond * testDurationSeconds;

        var stopwatch = Stopwatch.StartNew();

        // Act : traiter buffers
        for (int i = 0; i < totalBuffers; i++)
        {
            float[] buffer = SignalGenerator.GenerateWhiteNoise(BUFFER_SIZE_SAMPLES, amplitude: 0.1, seed: i);

            aWeightingFilter.ApplyFilter(buffer);
            var dspResult = dspEngine.ProcessBuffer(buffer);
            leqCalculator.AddSample(dspResult.DbFs);
        }

        stopwatch.Stop();

        double totalTimeMs = stopwatch.Elapsed.TotalMilliseconds;
        double availableTimeMs = testDurationSeconds * 1000.0; // Temps réel disponible

        // Assert : temps de traitement devrait être << temps réel disponible
        // (margin confortable pour UI refresh, etc.)
        totalTimeMs.Should().BeLessThan(availableTimeMs * 0.1,
            "Pipeline devrait traiter bien plus rapidement que le temps réel (< 10% du temps disponible)");

        Console.WriteLine($"Processed {totalBuffers} buffers in {totalTimeMs:F2} ms (available: {availableTimeMs:F2} ms)");
        Console.WriteLine($"Throughput: {totalTimeMs / totalBuffers:F4} ms/buffer (max allowed: {1000.0 / buffersPerSecond:F2} ms/buffer)");
    }

    #endregion
}
