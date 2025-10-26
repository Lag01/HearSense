using System.IO;

namespace ApplAudition.Constants;

/// <summary>
/// Constantes de configuration de l'application ApplAudition.
/// Centralise les chemins, noms et paramètres système.
/// </summary>
public static class AppConstants
{
    public const string APP_NAME = "ApplAudition";
    public const string APP_DISPLAY_NAME = "Appli Audition";

    // Chemins
    public static readonly string AppDataFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        APP_NAME);

    public static readonly string LogsFolder = Path.Combine(AppDataFolder, "logs");
    public const string LOG_FILE_PATTERN = "app-.log";

    public static readonly string SettingsFilePath = Path.Combine(AppDataFolder, "settings.json");

    public static readonly string ResourcesFolder = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "Resources");

    public static readonly string IconPath = Path.Combine(ResourcesFolder, "icon.ico");

    // Registre Windows
    public const string REGISTRY_RUN_KEY = @"Software\Microsoft\Windows\CurrentVersion\Run";
    public const string STARTUP_ARG_MINIMIZED = "--minimized";

    // Logging
    public const int LOG_FILE_SIZE_LIMIT_MB = 10;
    public const int LOG_FILE_RETENTION_COUNT = 10;

    // UI - Tooltip
    public const int MAX_TOOLTIP_LENGTH = 63; // Windows limite les tooltips NotifyIcon à 63 caractères
}
