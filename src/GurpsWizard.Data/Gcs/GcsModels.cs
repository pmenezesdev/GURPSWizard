using System.Text.Json.Serialization;

namespace GurpsWizard.Data.Gcs;

// ─────────────────────────────────────────────────────────────────────────────
// Raiz genérica (suporta v5 sem type/id e v2 com type/id)
// ─────────────────────────────────────────────────────────────────────────────

public class GcsFileRoot<TRow>
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("version")]
    public int Version { get; set; }

    [JsonPropertyName("rows")]
    public List<TRow> Rows { get; set; } = [];
}

// ─────────────────────────────────────────────────────────────────────────────
// .adq — Vantagens e Desvantagens
// ─────────────────────────────────────────────────────────────────────────────

public class GcsTraitRow
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("base_points")]
    public int BasePoints { get; set; }

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = [];

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    [JsonPropertyName("reference")]
    public string? Reference { get; set; }

    [JsonPropertyName("can_level")]
    public bool CanLevel { get; set; }

    [JsonPropertyName("points_per_level")]
    public int? PointsPerLevel { get; set; }

    // Campos ignorados (modifiers, calc, levels, cr, cr_adj, etc.)
}

// ─────────────────────────────────────────────────────────────────────────────
// .skl — Perícias
// ─────────────────────────────────────────────────────────────────────────────

public class GcsSkillRow
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    /// <summary>
    /// Formato: "attr/dificuldade" (ex: "iq/h", "dx/a") ou apenas "a" para Técnicas.
    /// </summary>
    [JsonPropertyName("difficulty")]
    public string Difficulty { get; set; } = "";

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = [];

    [JsonPropertyName("specialization")]
    public string? Specialization { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    [JsonPropertyName("reference")]
    public string? Reference { get; set; }

    // Campos ignorados: defaults, default, limit, tech_level, prereqs, features, etc.
}

// ─────────────────────────────────────────────────────────────────────────────
// .eqp — Equipamentos
// ─────────────────────────────────────────────────────────────────────────────

public class GcsEquipmentRow
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    /// <summary>Campo "description" no JSON (não "name").</summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    [JsonPropertyName("value")]
    public decimal Value { get; set; }

    [JsonPropertyName("weight")]
    public string? Weight { get; set; }

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = [];

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    [JsonPropertyName("reference")]
    public string? Reference { get; set; }

    [JsonPropertyName("tech_level")]
    public string? TechLevel { get; set; }

    // Campos ignorados: quantity, calc, legality_class, equipped, weapons, features, etc.
}
