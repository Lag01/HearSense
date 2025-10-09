using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Serilog;

namespace ApplAudition.Models;

/// <summary>
/// Base de données de profils heuristiques de casques/écouteurs.
/// Charge les profils depuis le fichier JSON embarqué.
/// </summary>
public class ProfileDatabase
{
    private readonly ILogger _logger;
    private List<Profile> _profiles = new();

    /// <summary>
    /// Liste des profils chargés.
    /// </summary>
    public IReadOnlyList<Profile> Profiles => _profiles.AsReadOnly();

    public ProfileDatabase(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Charge les profils depuis le fichier JSON embarqué.
    /// </summary>
    public async Task LoadProfilesAsync()
    {
        try
        {
            // Charger le fichier JSON embarqué
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "ApplAudition.Resources.profiles.json";

            await using var stream = assembly.GetManifestResourceStream(resourceName);

            if (stream == null)
            {
                _logger.Error("Fichier profiles.json introuvable dans les ressources embarquées");
                throw new FileNotFoundException("Fichier profiles.json introuvable", resourceName);
            }

            // Désérialiser le JSON
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };

            var data = await JsonSerializer.DeserializeAsync<ProfilesData>(stream, options);

            if (data?.Profiles == null || data.Profiles.Count == 0)
            {
                _logger.Warning("Aucun profil trouvé dans profiles.json");
                _profiles = new List<Profile>();
                return;
            }

            _profiles = data.Profiles;
            _logger.Information("Profils chargés avec succès : {Count} profils", _profiles.Count);

            // Logger les profils chargés
            foreach (var profile in _profiles)
            {
                _logger.Debug("Profil chargé : {Profile}", profile);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Erreur lors du chargement des profils");
            _profiles = new List<Profile>();
            throw;
        }
    }

    /// <summary>
    /// Récupère un profil par son ID.
    /// </summary>
    public Profile? GetProfileById(string id)
    {
        return _profiles.FirstOrDefault(p => p.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Classe interne pour la désérialisation JSON.
    /// </summary>
    private class ProfilesData
    {
        [JsonPropertyName("profiles")]
        public List<Profile> Profiles { get; set; } = new();
    }
}
