using System.Text.Json;
using System.Text.Json.Serialization;

namespace GurpsWizard.Data.Gcs;

/// <summary>
/// Aceita tanto número (20) quanto string ("20") no JSON para campos decimal.
/// Necessário para arquivos v2 (GURPS Magia) que serializam value como string.
/// </summary>
internal sealed class StringOrNumberDecimalConverter : JsonConverter<decimal>
{
    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType == JsonTokenType.String
            ? decimal.Parse(reader.GetString()!, System.Globalization.CultureInfo.InvariantCulture)
            : reader.GetDecimal();

    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value);
}

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

    [JsonPropertyName("prereqs")]
    public GcsPrereqNode? Prereqs { get; set; }

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

    [JsonPropertyName("prereqs")]
    public GcsPrereqNode? Prereqs { get; set; }

    /// <summary>Técnicas: perícia-pai com modificador (ex: Briga-2).</summary>
    [JsonPropertyName("default")]
    public GcsDefaultEntry? Default { get; set; }

    /// <summary>Técnicas: nível máximo acima do predefinido (0 = sem limite explícito).</summary>
    [JsonPropertyName("limit")]
    public int? Limit { get; set; }

    // Campos ignorados: defaults, tech_level, features, etc.
}

// ─────────────────────────────────────────────────────────────────────────────
// default entry (técnicas)
// ─────────────────────────────────────────────────────────────────────────────

public class GcsDefaultEntry
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("modifier")]
    public int Modifier { get; set; }
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
    [JsonConverter(typeof(StringOrNumberDecimalConverter))]
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

// ─────────────────────────────────────────────────────────────────────────────
// .spl — Mágicas
// ─────────────────────────────────────────────────────────────────────────────

public class GcsSpellRow
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    /// <summary>Formato: "iq/h" ou "iq/vh".</summary>
    [JsonPropertyName("difficulty")]
    public string Difficulty { get; set; } = "";

    /// <summary>Escolas da mágica (ex: ["Ar"], ["Ar ou Clima"]).</summary>
    [JsonPropertyName("college")]
    public List<string> College { get; set; } = [];

    [JsonPropertyName("power_source")]
    public string? PowerSource { get; set; }

    [JsonPropertyName("spell_class")]
    public string? SpellClass { get; set; }

    [JsonPropertyName("resist")]
    public string? Resist { get; set; }

    [JsonPropertyName("casting_cost")]
    public string? CastingCost { get; set; }

    [JsonPropertyName("maintenance_cost")]
    public string? MaintenanceCost { get; set; }

    [JsonPropertyName("casting_time")]
    public string? CastingTime { get; set; }

    [JsonPropertyName("duration")]
    public string? Duration { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    [JsonPropertyName("reference")]
    public string? Reference { get; set; }

    [JsonPropertyName("prereqs")]
    public GcsPrereqNode? Prereqs { get; set; }

    [JsonPropertyName("categories")]
    public List<string> Categories { get; set; } = [];

    // Campos ignorados: points, weapons, calc, etc.
}

// ─────────────────────────────────────────────────────────────────────────────
// Prerequisite node (shared by traits and skills)
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Generic deserialization node for the GCS prereqs JSON tree.
/// Handles prereq_list, trait_prereq, skill_prereq, and attribute_prereq types.
/// </summary>
public class GcsPrereqNode
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    // prereq_list fields
    [JsonPropertyName("all")]
    public bool All { get; set; }

    [JsonPropertyName("prereqs")]
    public List<GcsPrereqNode>? Prereqs { get; set; }

    // trait_prereq / skill_prereq fields
    [JsonPropertyName("has")]
    public bool Has { get; set; } = true;

    [JsonPropertyName("name")]
    public GcsCompareQualifier? Name { get; set; }

    // skill_prereq specialization
    [JsonPropertyName("specialization")]
    public GcsCompareQualifier? Specialization { get; set; }

    // attribute_prereq fields
    [JsonPropertyName("which")]
    public string? Which { get; set; }

    [JsonPropertyName("qualifier")]
    public JsonElement? Qualifier { get; set; }
}

public class GcsCompareQualifier
{
    [JsonPropertyName("compare")]
    public string Compare { get; set; } = "";

    [JsonPropertyName("qualifier")]
    public JsonElement QualifierValue { get; set; }
}
