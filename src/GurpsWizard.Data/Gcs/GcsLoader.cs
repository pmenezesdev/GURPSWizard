using System.Text.Json;
using GurpsWizard.Core.Models;
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
                GcsId              = r.Id,
                Name               = r.Name,
                BasePoints         = r.BasePoints,
                CanLevel           = r.CanLevel,
                PointsPerLevel     = r.PointsPerLevel ?? 0,
                Tags               = string.Join(",", r.Tags),
                Notes              = r.Notes,
                Reference          = r.Reference,
                PrerequisitesJson  = SerializePrereqs(ParsePrereqs(r.Prereqs)),
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
            .Select(r => (parsed: ParseDifficulty(r.Difficulty), row: r))
            .Where(t => !string.IsNullOrEmpty(t.parsed.BaseAttr)) // exclui técnicas
            .Select(t => new LibrarySkill
            {
                GcsId              = t.row.Id,
                Name               = t.row.Name,
                BaseAttribute      = t.parsed.BaseAttr,
                Difficulty         = t.parsed.Difficulty,
                Specialization     = t.row.Specialization,
                Tags               = string.Join(",", t.row.Tags),
                Notes              = t.row.Notes,
                Reference          = t.row.Reference,
                PrerequisitesJson  = SerializePrereqs(ParsePrereqs(t.row.Prereqs)),
            });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Técnicas (.skl — linhas sem atributo base, dificuldade sem barra)
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<IEnumerable<LibraryTechnique>> LoadTechniquesAsync(string path)
    {
        await using var stream = File.OpenRead(path);
        var file = await JsonSerializer.DeserializeAsync<GcsFileRoot<GcsSkillRow>>(stream, JsonOpts)
                   ?? throw new InvalidDataException($"Falha ao deserializar {path}");

        return file.Rows
            .Where(r => !string.IsNullOrWhiteSpace(r.Id) && !string.IsNullOrWhiteSpace(r.Name))
            .Select(r => (parsed: ParseDifficulty(r.Difficulty), row: r))
            .Where(t => string.IsNullOrEmpty(t.parsed.BaseAttr)) // apenas técnicas
            .Select(t => new LibraryTechnique
            {
                GcsId            = t.row.Id,
                Name             = t.row.Name,
                Difficulty       = t.parsed.Difficulty,
                ParentSkillName  = t.row.Default?.Name ?? "",
                DefaultModifier  = t.row.Default?.Modifier ?? 0,
                MaxAboveDefault  = t.row.Limit is > 0 ? t.row.Limit : null,
                Tags             = string.Join(",", t.row.Tags),
                Notes            = t.row.Notes,
                Reference        = t.row.Reference,
            });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Mágicas (.spl)
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<IEnumerable<LibrarySpell>> LoadSpellsAsync(string path)
    {
        await using var stream = File.OpenRead(path);
        var file = await JsonSerializer.DeserializeAsync<GcsFileRoot<GcsSpellRow>>(stream, JsonOpts)
                   ?? throw new InvalidDataException($"Falha ao deserializar {path}");

        return file.Rows
            .Where(r => !string.IsNullOrWhiteSpace(r.Id) && !string.IsNullOrWhiteSpace(r.Name))
            .Select(r =>
            {
                var (_, diff) = ParseDifficulty(r.Difficulty);
                return new LibrarySpell
                {
                    GcsId            = r.Id,
                    Name             = r.Name,
                    College          = r.College.FirstOrDefault() ?? "",
                    PowerSource      = r.PowerSource,
                    SpellClass       = r.SpellClass,
                    Difficulty       = diff,
                    Resist           = r.Resist,
                    CastingCost      = r.CastingCost,
                    MaintenanceCost  = r.MaintenanceCost,
                    CastingTime      = r.CastingTime,
                    Duration         = r.Duration,
                    Notes            = r.Notes,
                    Reference        = r.Reference,
                    Tags             = string.Join(",", r.Categories),
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

    // ─────────────────────────────────────────────────────────────────────────
    // Prerequisite parsing
    // ─────────────────────────────────────────────────────────────────────────

    private static readonly JsonSerializerOptions PrereqSerializeOpts = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>
    /// Parses a GCS prereq node tree into our domain Prerequisite model.
    /// Returns null when there are no prerequisites.
    /// </summary>
    internal static Prerequisite? ParsePrereqs(GcsPrereqNode? node)
    {
        if (node is null) return null;

        return node.Type switch
        {
            "prereq_list" => ParsePrereqList(node),
            "trait_prereq" => ParseTraitPrereq(node),
            "skill_prereq" => ParseSkillPrereq(node),
            "attribute_prereq" => ParseAttributePrereq(node),
            _ => null, // unknown prereq types are silently ignored
        };
    }

    private static PrerequisiteGroup? ParsePrereqList(GcsPrereqNode node)
    {
        if (node.Prereqs is null or { Count: 0 }) return null;

        var items = new List<Prerequisite>();
        foreach (var child in node.Prereqs)
        {
            var parsed = ParsePrereqs(child);
            if (parsed is not null) items.Add(parsed);
        }

        return items.Count > 0 ? new PrerequisiteGroup(node.All, items) : null;
    }

    private static TraitPrerequisite ParseTraitPrereq(GcsPrereqNode node)
    {
        var name = node.Name?.QualifierValue.GetString() ?? "";
        return new TraitPrerequisite(name, node.Has);
    }

    private static SkillPrerequisite ParseSkillPrereq(GcsPrereqNode node)
    {
        var name = node.Name?.QualifierValue.GetString() ?? "";
        var spec = node.Specialization?.QualifierValue.GetString();
        return new SkillPrerequisite(name, spec, node.Has);
    }

    private static AttributePrerequisite? ParseAttributePrereq(GcsPrereqNode node)
    {
        if (node.Which is null) return null;

        // attribute_prereq qualifier is an object: { "compare": "at_least", "qualifier": 15 }
        var compare = "at_least";
        var qualValue = 0;

        if (node.Qualifier.HasValue && node.Qualifier.Value.ValueKind == JsonValueKind.Object)
        {
            if (node.Qualifier.Value.TryGetProperty("compare", out var cmp))
                compare = cmp.GetString() ?? "at_least";
            if (node.Qualifier.Value.TryGetProperty("qualifier", out var q))
                qualValue = q.TryGetInt32(out var n) ? n : 0;
        }

        return new AttributePrerequisite(node.Which.ToUpperInvariant(), qualValue, compare, node.Has);
    }

    /// <summary>Serializes a Prerequisite tree to JSON, or returns null if prereq is null.</summary>
    internal static string? SerializePrereqs(Prerequisite? prereq)
    {
        if (prereq is null) return null;
        return JsonSerializer.Serialize<Prerequisite>(prereq, PrereqSerializeOpts);
    }
}
