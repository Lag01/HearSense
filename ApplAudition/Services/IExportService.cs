using ApplAudition.Models;

namespace ApplAudition.Services;

/// <summary>
/// Interface du service d'export des données vers CSV (Phase 9 - Tâche 21).
/// </summary>
public interface IExportService
{
    /// <summary>
    /// Exporte les données historiques vers un fichier CSV.
    /// </summary>
    /// <param name="data">Collection de points de données à exporter.</param>
    /// <param name="filePath">Chemin complet du fichier CSV de destination.</param>
    /// <returns>True si l'export a réussi, false sinon.</returns>
    Task<bool> ExportToCsvAsync(IEnumerable<ExportDataPoint> data, string filePath);
}
