using ApplAudition.Models;

namespace ApplAudition.Services;

/// <summary>
/// Gestionnaire des modes d'estimation SPL (Mode A vs Mode B).
/// Responsable de la sélection automatique du mode et du calcul SPL estimé.
/// </summary>
public interface IEstimationModeManager
{
    /// <summary>
    /// Mode d'estimation actuellement actif (ModeA ou ModeB).
    /// </summary>
    EstimationMode CurrentMode { get; }

    /// <summary>
    /// Profil heuristique actuellement utilisé (null en Mode A ou si aucun profil détecté).
    /// </summary>
    Profile? CurrentProfile { get; }

    /// <summary>
    /// Indique si le Mode A est forcé manuellement par l'utilisateur.
    /// </summary>
    bool IsForcedModeA { get; }

    /// <summary>
    /// Indique si une calibration personnalisée est active (Phase 8 - Tâche 20).
    /// </summary>
    bool IsCalibrated { get; }

    /// <summary>
    /// Constante de calibration personnalisée (null si pas calibré) (Phase 8 - Tâche 20).
    /// </summary>
    float? CalibrationConstantC { get; }

    /// <summary>
    /// Événement déclenché lorsque le mode d'estimation change.
    /// </summary>
    event EventHandler? ModeChanged;

    /// <summary>
    /// Initialise le gestionnaire : détecte le périphérique, matche le profil, et définit le mode.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Estime le niveau SPL à partir du dBFS selon le mode actif.
    /// - Mode A : retourne dBFS (relatif)
    /// - Mode B : retourne dBFS + C (SPL estimé absolu)
    /// </summary>
    /// <param name="dbfs">Niveau en dBFS (calculé par DSP)</param>
    /// <returns>
    /// Mode A : dBFS relatif (même valeur)
    /// Mode B : SPL estimé en dB(A) (dbfs + constante C du profil)
    /// </returns>
    float EstimateSpl(float dbfs);

    /// <summary>
    /// Force ou libère le Mode A manuellement.
    /// </summary>
    /// <param name="force">
    /// True : forcer Mode A (ignorer profil détecté)
    /// False : laisser le mode automatique
    /// </param>
    void SetForceModeA(bool force);

    /// <summary>
    /// Définit une constante de calibration personnalisée (Phase 8 - Tâche 20).
    /// Priorité : C_calibrated > C_profil.
    /// </summary>
    /// <param name="constantC">Nouvelle constante C (null pour désactiver calibration)</param>
    Task SetCalibrationConstantAsync(float? constantC);

    /// <summary>
    /// Réinitialise la calibration (retour à la constante du profil heuristique) (Phase 8 - Tâche 20).
    /// </summary>
    Task ResetCalibrationAsync();
}
