using Microsoft.Toolkit.Uwp.Notifications;
using Serilog;
using System.IO;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using HearSense.Helpers;

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
    /// <param name="iconPath">Chemin absolu vers l'icône à afficher (préférer PNG)</param>
    public void ShowToast(string title, string message, string? iconPath = null)
    {
        try
        {
            // Créer le contenu de la notification Toast
            var toastContent = new ToastContentBuilder()
                .AddText(title)
                .AddText(message);

            // Ajouter l'icône personnalisée si fournie
            if (!string.IsNullOrEmpty(iconPath))
            {
                try
                {
                    string fullPath = Path.GetFullPath(iconPath);

                    // Vérifier d'abord si une version PNG existe (préféré pour Toast)
                    string pngPath = Path.ChangeExtension(fullPath, ".png");
                    string effectivePath = File.Exists(pngPath) ? pngPath : fullPath;

                    if (File.Exists(effectivePath))
                    {
                        // Créer une URI file:// pour l'icône
                        var uri = new Uri("file:///" + effectivePath.Replace("\\", "/"), UriKind.Absolute);
                        toastContent.AddAppLogoOverride(uri, ToastGenericAppLogoCrop.Default);
                        _logger.Debug("Icône personnalisée ajoutée à la notification : {IconPath} (format: {Ext})",
                            effectivePath, Path.GetExtension(effectivePath));
                    }
                    else
                    {
                        _logger.Warning("Fichier d'icône introuvable : {IconPath}. Notification affichée sans icône.", fullPath);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex, "Erreur lors de l'ajout de l'icône : {IconPath}. Notification affichée sans icône.", iconPath);
                }
            }

            // Obtenir le contenu XML
            var toastXml = toastContent.GetToastContent().GetXml();

            // Log du XML pour debugging
            _logger.Debug("Toast XML : {Xml}", toastXml.GetXml());

            // Créer la notification
            var toast = new ToastNotification(toastXml);

            // Afficher la notification via ToastNotificationManager avec l'AUMID explicite
            // Cela permet à Windows de lier correctement la notification à l'application enregistrée
            ToastNotificationManager.CreateToastNotifier(AppUserModelHelper.APP_USER_MODEL_ID).Show(toast);

            _logger.Information("Notification Toast affichée avec AUMID : {Title} ({AUMID})",
                title, AppUserModelHelper.APP_USER_MODEL_ID);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Erreur lors de l'affichage de la notification Toast");
        }
    }
}
