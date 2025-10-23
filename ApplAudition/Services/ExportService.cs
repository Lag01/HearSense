using System.Globalization;
using System.IO;
using System.Text;
using ApplAudition.Models;
using Serilog;

namespace ApplAudition.Services;

/// <summary>
/// Service d'export des données vers CSV (Phase 9 - Tâche 21).
/// Encode en UTF-8 avec BOM pour compatibilité Excel.
/// </summary>
public class ExportService : IExportService
{
    private readonly ILogger _logger;

    public ExportService(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Exporte les données historiques vers un fichier CSV.
    /// Format : Timestamp,dBFS,dB(A),Leq_1min,Peak
    /// </summary>
    /// <param name="data">Collection de points de données à exporter.</param>
    /// <param name="filePath">Chemin complet du fichier CSV de destination.</param>
    /// <returns>True si l'export a réussi, false sinon.</returns>
    public async Task<bool> ExportToCsvAsync(IEnumerable<ExportDataPoint> data, string filePath)
    {
        try
        {
            _logger.Information("Début de l'export CSV vers {FilePath}", filePath);

            var dataList = data.ToList();

            if (dataList.Count == 0)
            {
                _logger.Warning("Aucune donnée à exporter");
                return false;
            }

            // Créer le contenu CSV
            var csvContent = new StringBuilder();

            // En-tête CSV
            csvContent.AppendLine("Timestamp,dBFS,dB(A),Leq_1min,Peak");

            // Lignes de données
            foreach (var point in dataList)
            {
                // Formater timestamp au format ISO 8601 (Excel compatible)
                string timestamp = point.Timestamp.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                // Formater les valeurs avec point décimal (culture invariante)
                string dbfs = point.DbFs.ToString("F1", CultureInfo.InvariantCulture);
                string dba = point.DbA.ToString("F1", CultureInfo.InvariantCulture);
                string leq = point.Leq1Min.ToString("F1", CultureInfo.InvariantCulture);
                string peak = point.Peak.ToString("F1", CultureInfo.InvariantCulture);

                // Construire ligne CSV
                csvContent.AppendLine($"{timestamp},{dbfs},{dba},{leq},{peak}");
            }

            // Écrire dans le fichier avec encodage UTF-8 BOM (pour Excel)
            await File.WriteAllTextAsync(filePath, csvContent.ToString(), new UTF8Encoding(true));

            _logger.Information(
                "Export CSV réussi : {Count} lignes exportées vers {FilePath}",
                dataList.Count,
                filePath);

            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Erreur lors de l'export CSV vers {FilePath}", filePath);
            return false;
        }
    }
}
