using System.Text.Json.Serialization;

namespace ApplAudition.Models;

/// <summary>
/// Profil heuristique d'un casque/écouteurs.
/// Contient les paramètres pour estimer le SPL (dB(A)) à partir du dBFS.
/// </summary>
public class Profile
{
    /// <summary>
    /// Identifiant unique du profil (ex: "over-ear-anc").
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Nom descriptif du profil (ex: "Over-ear ANC (fermés)").
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Patterns regex pour matcher le nom du périphérique.
    /// Ex: ["WH-1000XM", "QC35", "Bose.*700"]
    /// </summary>
    [JsonPropertyName("patterns")]
    public List<string> Patterns { get; set; } = new();

    /// <summary>
    /// Sensibilité du casque (dB/mW).
    /// Ex: 103 dB/mW pour Sony WH-1000XM4.
    /// Optionnel, à des fins documentaires.
    /// </summary>
    [JsonPropertyName("sensitivity_db_mw")]
    public double? SensitivityDbMw { get; set; }

    /// <summary>
    /// Impédance du casque (Ω).
    /// Ex: 47Ω pour Sony WH-1000XM4.
    /// Optionnel, à des fins documentaires.
    /// </summary>
    [JsonPropertyName("impedance_ohm")]
    public double? ImpedanceOhm { get; set; }

    /// <summary>
    /// Constante C pour convertir dBFS → SPL estimé.
    /// Formule : SPL_est (dB(A)) = dBFS + C
    /// Typiquement entre -20 et -5 dB (dépend du casque et volume système).
    /// </summary>
    [JsonPropertyName("constant_c")]
    public double ConstantC { get; set; }

    /// <summary>
    /// Marge d'erreur estimée (dB).
    /// Ex: 6 dB = précision ±6 dB.
    /// </summary>
    [JsonPropertyName("margin_db")]
    public double MarginDb { get; set; }

    /// <summary>
    /// Indique si ce profil est un fallback générique (non basé sur un match exact).
    /// </summary>
    [JsonIgnore]
    public bool IsFallback { get; set; }

    public override string ToString()
    {
        return $"{Name} (C={ConstantC:+0.0;-0.0} dB, marge ±{MarginDb} dB)";
    }
}
