using System.Text.Json;
using GurpsWizard.Data.Entities;

namespace GurpsWizard.Data.Gcs;

/// <summary>
/// Lê os arquivos JSON do GCS-PTBR e converte para as entidades de biblioteca
/// prontas para persistência no SQLite.
/// </summary>
public class GcsLoader
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    // ─────────────────────────────────────────────────────────────────────────
    // Vantagens e Desvantagens (.adq)
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<IEnumerable<LibraryTrait>> LoadTraitsAsync(string path)
    {
        await using var stream = File.OpenRead(path);
        var file = await JsonSerializer.DeserializeAsync<GcsFileRoot<GcsTraitRow>>(stream, JsonOpts)
                   ?? throw new InvalidDataException($"Falha ao deserializar {path}");

        return file.Rows
            .Where(r => !string.IsNullOrWhiteSpace(r.Id) && !string.IsNullOrWhiteSpace(r.Name))
            .Select(r => new LibraryTrait
            {
                GcsId         = r.Id,
                Name          = r.Name,
                BasePoints    = r.BasePoints,
                CanLevel      = r.CanLevel,
                PointsPerLevel = r.PointsPerLevel ?? 0,
                Tags          = string.Join(",", r.Tags),
                Notes         = r.Notes,
                Reference     = r.Reference,
            });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Perícias (.skl)
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<IEnumerable<LibrarySkill>> LoadSkillsAsync(string path)
    {
        await using var stream = File.OpenRead(path);
        var file = await JsonSerializer.DeserializeAsync<GcsFileRoot<GcsSkillRow>>(stream, JsonOpts)
                   ?? throw new InvalidDataException($"Falha ao deserializar {path}");

        return file.Rows
            .Where(r => !string.IsNullOrWhiteSpace(r.Id) && !string.IsNullOrWhiteSpace(r.Name))
            .Select(r =>
            {
                var (baseAttr, diff) = ParseDifficulty(r.Difficulty);
                return new LibrarySkill
                {
                    GcsId          = r.Id,
                    Name           = r.Name,
                    BaseAttribute  = baseAttr,
                    Difficulty     = diff,
                    Specialization = r.Specialization,
                    Tags           = string.Join(",", r.Tags),
                    Notes          = r.Notes,
                    Reference      = r.Reference,
                };
            });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Equipamentos (.eqp)
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<IEnumerable<LibraryEquipment>> LoadEquipmentAsync(string path)
    {
        await using var stream = File.OpenRead(path);
        var file = await JsonSerializer.DeserializeAsync<GcsFileRoot<GcsEquipmentRow>>(stream, JsonOpts)
                   ?? throw new InvalidDataException($"Falha ao deserializar {path}");

        return file.Rows
            .Where(r => !string.IsNullOrWhiteSpace(r.Id) && !string.IsNullOrWhiteSpace(r.Description))
            .Select(r => new LibraryEquipment
            {
                GcsId     = r.Id,
                Name      = r.Description,
                Value     = r.Value,
                Weight    = r.Weight ?? "",
                Tags      = string.Join(",", r.Tags),
                Notes     = r.Notes,
                Reference = r.Reference,
                TechLevel = r.TechLevel,
            });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Analisa o campo "difficulty" do JSON.
    /// Formatos: "iq/h", "dx/a", "st/vh", "ht/e" → (attr, diff)
    /// "a" (técnica, sem barra) → ("", "A")
    /// </summary>
    private static (string BaseAttr, string Difficulty) ParseDifficulty(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return ("", "");

        var slash = raw.IndexOf('/');
        if (slash < 0)
            // Técnica — sem atributo base próprio
            return ("", NormalizeDiff(raw));

        var attr = raw[..slash].ToUpperInvariant();
        var diff = NormalizeDiff(raw[(slash + 1)..]);
        return (attr, diff);
    }

    private static string NormalizeDiff(string d) => d.ToUpperInvariant() switch
    {
        "E"  => "E",
        "A"  => "A",
        "H"  => "H",
        "VH" => "VH",
        _    => d.ToUpperInvariant(),
    };
}
