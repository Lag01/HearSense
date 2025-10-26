using Serilog;
using System.Runtime.InteropServices;

namespace ApplAudition.Helpers;

/// <summary>
/// Helper pour configurer l'AppUserModelID de l'application.
/// Nécessaire pour les Toast Notifications natives Windows 10/11.
/// </summary>
public static class AppUserModelHelper
{
    // AppUserModelID unique pour l'application
    private const string APP_USER_MODEL_ID = "LuminDev.ApplAudition.1";

    /// <summary>
    /// Définit l'AppUserModelID du processus actuel.
    /// Cela permet à Windows d'identifier correctement l'application
    /// et d'afficher les Toast Notifications avec le bon nom et l'icône.
    /// </summary>
    public static void SetAppUserModelId(ILogger? logger = null)
    {
        try
        {
            // Définir l'AppUserModelID pour le processus actuel
            int result = SetCurrentProcessExplicitAppUserModelID(APP_USER_MODEL_ID);

            if (result != 0)
            {
                logger?.Warning("Échec de la définition de l'AppUserModelID (HRESULT: {Result})", result);
            }
            else
            {
                logger?.Information("AppUserModelID défini : {AppUserModelId}", APP_USER_MODEL_ID);
            }
        }
        catch (Exception ex)
        {
            logger?.Error(ex, "Erreur lors de la définition de l'AppUserModelID");
        }
    }

    #region P/Invoke

    /// <summary>
    /// Définit l'AppUserModelID explicite pour le processus actuel.
    /// </summary>
    /// <param name="appId">L'AppUserModelID à définir</param>
    /// <returns>HRESULT (0 = succès)</returns>
    [DllImport("shell32.dll", SetLastError = true)]
    private static extern int SetCurrentProcessExplicitAppUserModelID(
        [MarshalAs(UnmanagedType.LPWStr)] string appId);

    #endregion
}
