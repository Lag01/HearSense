using System.IO;
using System.Text.Json;
using ApplAudition.Models;
using Serilog;

namespace ApplAudition.Services;

/// <summary>
/// Service de gestion des paramètres de l'application (Phase 7 - Tâche 18).
/// Persistance dans %LOCALAPPDATA%\ApplAudition\settings.json.
/// </summary>
public class SettingsService : ISettingsService
{
    private readonly ILogger _logger;
    private readonly string _settingsFilePath;
    private AppSettings _settings;

    public event EventHandler? SettingsChanged;

    public AppSettings Settings => _settings;

    public SettingsService(ILogger logger)
    {
        _logger = logger;

        // Déterminer le chemin du fichier settings.json
        string appDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ApplAudition");

        Directory.CreateDirectory(appDataFolder);
        _settingsFilePath = Path.Combine(appDataFolder, "settings.json");

        _settings = new AppSettings();

        _logger.Information("SettingsService initialisé, chemin: {Path}", _settingsFilePath);
    }

    /// <summary>
    /// Charge les paramètres depuis le fichier JSON.
    /// </summary>
    public async Task LoadAsync()
    {
        try
        {
            if (!File.Exists(_settingsFilePath))
            {
                _logger.Information("Fichier settings.json introuvable, utilisation des valeurs par défaut");
                _settings = new AppSettings();
                await SaveAsync(); // Créer le fichier avec les valeurs par défaut
                return;
            }

            string json = await File.ReadAllTextAsync(_settingsFilePath);
            var loadedSettings = JsonSerializer.Deserialize<AppSettings>(json);

            if (loadedSettings != null)
            {
                _settings = loadedSettings;
                _logger.Information(
                    "Paramètres chargés - ThresholdWarning: {Warning} dB, ThresholdDanger: {Danger} dB",
                    _settings.ThresholdWarning, _settings.ThresholdDanger);
            }
            else
            {
                _logger.Warning("Échec de la désérialisation, utilisation des valeurs par défaut");
                _settings = new AppSettings();
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Erreur lors du chargement des paramètres");
            _settings = new AppSettings();
        }
    }

    /// <summary>
    /// Sauvegarde les paramètres dans le fichier JSON.
    /// </summary>
    public async Task SaveAsync()
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(_settings, options);
            await File.WriteAllTextAsync(_settingsFilePath, json);

            _logger.Debug("Paramètres sauvegardés: {Path}", _settingsFilePath);
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Erreur lors de la sauvegarde des paramètres");
        }
    }

}
