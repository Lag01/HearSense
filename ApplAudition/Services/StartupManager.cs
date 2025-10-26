using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using Serilog;

namespace ApplAudition.Services;

/// <summary>
/// Gestionnaire du démarrage automatique de l'application avec Windows.
/// Utilise la clé de registre HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run.
/// </summary>
public class StartupManager : IStartupManager
{
    private const string REGISTRY_KEY_PATH = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string APP_NAME = "ApplAudition";
    private const string MINIMIZED_ARG = "--minimized";

    private readonly ILogger _logger;

    public StartupManager(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Obtient le chemin complet de l'exécutable actuel avec validation de sécurité.
    /// </summary>
    private string GetExecutablePath()
    {
        var process = Process.GetCurrentProcess();
        var mainModule = process.MainModule;

        if (mainModule == null || string.IsNullOrWhiteSpace(mainModule.FileName))
        {
            _logger.Error("Impossible de récupérer le module principal");
            throw new UnauthorizedAccessException("Accès au module refusé");
        }

        var exePath = mainModule.FileName;

        // Validation : doit être un chemin absolu valide
        if (!Path.IsPathFullyQualified(exePath))
        {
            _logger.Error("Chemin invalide (non absolu) : {ExePath}", exePath);
            throw new InvalidOperationException($"Chemin invalide : {exePath}");
        }

        // Vérifier que le fichier existe
        if (!File.Exists(exePath))
        {
            _logger.Error("Exécutable introuvable : {ExePath}", exePath);
            throw new FileNotFoundException($"Exécutable introuvable : {exePath}");
        }

        return exePath;
    }

    /// <summary>
    /// Ajoute l'application au démarrage automatique Windows.
    /// </summary>
    public bool AddToStartup(bool startMinimized = true)
    {
        try
        {
            var exePath = GetExecutablePath();
            var commandLine = startMinimized
                ? $"\"{exePath}\" {MINIMIZED_ARG}"
                : $"\"{exePath}\"";

            using var key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY_PATH, writable: true);

            if (key == null)
            {
                _logger.Error("Impossible d'ouvrir la clé de registre Run");
                return false;
            }

            key.SetValue(APP_NAME, commandLine, RegistryValueKind.String);

            _logger.Information("Application ajoutée au démarrage automatique : {CommandLine}", commandLine);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Erreur lors de l'ajout au démarrage automatique");
            return false;
        }
    }

    /// <summary>
    /// Supprime l'application du démarrage automatique Windows.
    /// </summary>
    public bool RemoveFromStartup()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY_PATH, writable: true);

            if (key == null)
            {
                _logger.Error("Impossible d'ouvrir la clé de registre Run");
                return false;
            }

            // Vérifier si la valeur existe avant de la supprimer
            if (key.GetValue(APP_NAME) != null)
            {
                key.DeleteValue(APP_NAME, throwOnMissingValue: false);
                _logger.Information("Application supprimée du démarrage automatique");
            }
            else
            {
                _logger.Debug("Application n'était pas dans le démarrage automatique");
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Erreur lors de la suppression du démarrage automatique");
            return false;
        }
    }

    /// <summary>
    /// Vérifie si l'application est configurée pour démarrer automatiquement.
    /// </summary>
    public bool IsInStartup()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY_PATH, writable: false);

            if (key == null)
            {
                _logger.Warning("Impossible d'ouvrir la clé de registre Run en lecture");
                return false;
            }

            var value = key.GetValue(APP_NAME) as string;
            bool isPresent = !string.IsNullOrEmpty(value);

            _logger.Debug("IsInStartup : {IsPresent} (valeur: {Value})", isPresent, value);

            return isPresent;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Erreur lors de la vérification du démarrage automatique");
            return false;
        }
    }

    /// <summary>
    /// Active ou désactive le démarrage automatique.
    /// </summary>
    public bool SetStartup(bool enable, bool startMinimized = true)
    {
        if (enable)
        {
            return AddToStartup(startMinimized);
        }
        else
        {
            return RemoveFromStartup();
        }
    }
}
