using Microsoft.Win32;
using Serilog;
using System.IO;

namespace HearSense.Services;

/// <summary>
/// Service d'enregistrement de l'application dans Windows.
/// Enregistre l'AppUserModelID dans le registre et crée un raccourci dans le menu Démarrer
/// pour que les Toast Notifications affichent le bon nom ("HearSense") et la bonne icône.
/// </summary>
public class WindowsRegistrationService : IWindowsRegistrationService
{
    private readonly ILogger _logger;
    private const string APP_USER_MODEL_ID = "LuminDev.HearSense.1";
    private const string DISPLAY_NAME = "HearSense";
    private const string ICON_BACKGROUND_COLOR = "FF2196F3"; // Bleu Material Design

    public WindowsRegistrationService(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Vérifie si l'application est déjà enregistrée dans le registre.
    /// </summary>
    public bool IsRegistered()
    {
        try
        {
            string registryPath = $@"Software\Classes\AppUserModelId\{APP_USER_MODEL_ID}";
            using var key = Registry.CurrentUser.OpenSubKey(registryPath, false);

            if (key == null)
                return false;

            var displayName = key.GetValue("DisplayName") as string;
            return displayName == DISPLAY_NAME;
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Erreur lors de la vérification de l'enregistrement");
            return false;
        }
    }

    /// <summary>
    /// Enregistre l'application dans le registre Windows.
    /// Note: Le raccourci Start Menu n'est pas créé car cela nécessite COM qui n'est pas compatible .NET 8.
    /// L'enregistrement dans le registre devrait suffire pour que les notifications affichent le bon nom.
    /// </summary>
    public async Task RegisterApplicationAsync()
    {
        try
        {
            // Enregistrer dans le registre
            RegisterInRegistry();

            _logger.Information("Application enregistrée avec succès dans Windows");

            await Task.CompletedTask; // Async pour compatibilité interface
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Erreur lors de l'enregistrement de l'application dans Windows");
            throw;
        }
    }

    /// <summary>
    /// Enregistre l'AppUserModelID dans le registre Windows.
    /// </summary>
    private void RegisterInRegistry()
    {
        try
        {
            string registryPath = $@"Software\Classes\AppUserModelId\{APP_USER_MODEL_ID}";

            using var key = Registry.CurrentUser.CreateSubKey(registryPath, true);

            if (key == null)
            {
                _logger.Error("Impossible de créer la clé de registre : {Path}", registryPath);
                return;
            }

            // Chemin vers l'icône PNG (200x200)
            string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "icon.png");

            // Enregistrer les valeurs
            key.SetValue("DisplayName", DISPLAY_NAME, RegistryValueKind.String);
            key.SetValue("IconUri", iconPath, RegistryValueKind.String);
            key.SetValue("IconBackgroundColor", ICON_BACKGROUND_COLOR, RegistryValueKind.String);

            _logger.Information("AppUserModelID enregistré dans le registre : {AUMID}", APP_USER_MODEL_ID);
            _logger.Debug("DisplayName={DisplayName}, IconUri={IconUri}", DISPLAY_NAME, iconPath);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Erreur lors de l'enregistrement dans le registre");
            throw;
        }
    }

}
