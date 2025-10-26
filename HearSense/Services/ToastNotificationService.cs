using Microsoft.Toolkit.Uwp.Notifications;
using Serilog;
using System.IO;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace HearSense.Services;

/// <summary>
/// Service pour afficher des notifications Toast natives Windows 10/11.
/// Utilise Microsoft.Toolkit.Uwp.Notifications pour un contrôle total sur l'apparence.
/// </summary>
public class ToastNotificationService : IToastNotificationService
{
    private readonly ILogger _logger;

    public ToastNotificationService(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Affiche une notification Toast native Windows avec icône personnalisée.
    /// </summary>
    /// <param name="title">Titre de la notification</param>
    /// <param name="message">Message de la notification</param>
    /// <param name="iconPath">Chemin absolu vers l'icône à afficher</param>
    public void ShowToast(string title, string message, string? iconPath = null)
    {
        try
        {
            // Créer le contenu de la notification Toast
            var toastContent = new ToastContentBuilder()
                .AddText(title)
                .AddText(message);

            // Ajouter l'icône personnalisée si fournie avec validation de sécurité
            if (!string.IsNullOrEmpty(iconPath))
            {
                try
                {
                    // Valider que le chemin est absolu et dans un répertoire autorisé
                    var fullPath = Path.GetFullPath(iconPath);
                    var allowedDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources"));

                    if (!fullPath.StartsWith(allowedDirectory, StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.Warning("Tentative d'accès à un chemin non autorisé : {IconPath}", iconPath);
                        return;
                    }

                    if (File.Exists(fullPath))
                    {
                        var uri = new Uri(fullPath, UriKind.Absolute);
                        toastContent.AddAppLogoOverride(uri, ToastGenericAppLogoCrop.Default);
                        _logger.Debug("Icône personnalisée ajoutée à la notification : {IconPath}", fullPath);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex, "Erreur lors de la validation du chemin de l'icône : {IconPath}", iconPath);
                }
            }

            // Obtenir le contenu XML
            var toastXml = toastContent.GetToastContent().GetXml();

            // Créer la notification
            var toast = new ToastNotification(toastXml);

            // Afficher la notification via ToastNotificationManager
            ToastNotificationManager.CreateToastNotifier().Show(toast);

            _logger.Information("Notification Toast affichée : {Title}", title);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Erreur lors de l'affichage de la notification Toast");
        }
    }
}
