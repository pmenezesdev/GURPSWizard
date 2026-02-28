using System.Text.Json;
using System.Text.Json.Nodes;
using GurpsWizard.Core.Models;

namespace GurpsWizard.App;

/// <summary>Converte um <see cref="CharacterDraft"/> para o formato .gcs (GURPS Character Sheet v5).</summary>
public static class GcsExportService
{
    private static string NewId(char prefix)
    {
        Span<byte> bytes = stackalloc byte[12];
        Random.Shared.NextBytes(bytes);
        return prefix + Convert.ToBase64String(bytes)
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }

    private static (string name, string? spec) SplitDisplayName(string displayName)
    {
        var idx = displayName.LastIndexOf(" (", StringComparison.Ordinal);
        if (idx >= 0 && displayName.EndsWith(')'))
            return (displayName[..idx], displayName[(idx + 2)..^1]);
        return (displayName, null);
    }

    public static string ToGcs(CharacterDraft draft)
    {
        var root = new JsonObject
        {
            ["version"]      = 5,
            ["id"]           = NewId('c'),
            ["total_points"] = draft.TotalPoints,
            ["profile"]      = new JsonObject { ["name"] = draft.Name },
            ["settings"]     = BuildSettings(),
            ["attributes"]   = BuildAttributes(draft),
            ["traits"]       = BuildTraits(draft),
            ["skills"]       = BuildSkills(draft),
            ["equipment"]    = BuildEquipment(draft),
        };

        return root.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
    }

    private static JsonObject BuildSettings() =>
        new()
        {
            ["page"]                     = JsonNode.Parse("""{"paper_size":"letter","orientation":"portrait","top_margin":"0.25 in","left_margin":"0.25 in","bottom_margin":"0.25 in","right_margin":"0.25 in"}"""),
            ["block_layout"]             = JsonNode.Parse("""["reactions conditional_modifiers","melee","ranged","traits skills","spells","equipment","other_equipment","notes"]"""),
            ["attributes"]               = JsonNode.Parse(PtbrAttributesJson),
            ["damage_progression"]       = "basic_set",
            ["default_length_units"]     = "ft_in",
            ["default_weight_units"]     = "lb",
            ["user_description_display"] = "tooltip",
            ["modifiers_display"]        = "inline",
            ["notes_display"]            = "inline",
            ["skill_level_adj_display"]  = "tooltip",
            ["show_spell_adj"]           = true,
        };

    private static JsonArray BuildAttributes(CharacterDraft d)
    {
        var a   = d.Attributes;
        var sec = d.SecondaryAttributes;
        var arr = new JsonArray();

        arr.Add(IntAttr("st",               a.ST - 10));
        arr.Add(IntAttr("dx",               a.DX - 10));
        arr.Add(IntAttr("iq",               a.IQ - 10));
        arr.Add(IntAttr("ht",               a.HT - 10));
        arr.Add(IntAttr("will",             sec.WillBonus));
        arr.Add(IntAttr("fright_check",     0));
        arr.Add(IntAttr("senses",           0));
        arr.Add(IntAttr("per",              sec.PerBonus));
        arr.Add(IntAttr("vision",           0));
        arr.Add(IntAttr("hearing",          0));
        arr.Add(IntAttr("taste_smell",      0));
        arr.Add(IntAttr("touch",            0));
        arr.Add(IntAttr("movement",         0));
        arr.Add(DecAttr("basic_speed",      sec.BasicSpeedBonus * 0.25));
        arr.Add(IntAttr("basic_move",       sec.BasicMoveBonus));
        arr.Add(IntAttr("highjump",         0));
        arr.Add(IntAttr("running_highjump", 0));
        arr.Add(IntAttr("broadjump",        0));
        arr.Add(IntAttr("running_broadjump",0));
        arr.Add(IntAttr("fp",               sec.FPBonus));
        arr.Add(IntAttr("hp",               sec.HPBonus));

        return arr;
    }

    private static JsonObject IntAttr(string id, int adj)  =>
        new() { ["attr_id"] = id, ["adj"] = adj };

    private static JsonObject DecAttr(string id, double adj) =>
        new() { ["attr_id"] = id, ["adj"] = adj };

    private static JsonArray BuildTraits(CharacterDraft d)
    {
        var arr = new JsonArray();
        foreach (var t in d.Advantages.Concat(d.Disadvantages))
        {
            var obj = new JsonObject
            {
                ["id"]          = NewId('t'),
                ["name"]        = t.Name,
                ["base_points"] = t.Cost,
                ["calc"]        = new JsonObject { ["points"] = t.Cost },
            };
            if (!t.IsCustom)
            {
                obj["source"] = new JsonObject
                {
                    ["library"] = "/gcs_user_library",
                    ["path"]    = "Módulo Básico/Módulo Básico Vantagens e Desvantagens.adq",
                    ["id"]      = t.DefinitionId,
                };
            }
            arr.Add(obj);
        }
        return arr;
    }

    private static JsonArray BuildSkills(CharacterDraft d)
    {
        var arr = new JsonArray();
        foreach (var s in d.Skills)
        {
            var (name, spec) = SplitDisplayName(s.Name);
            var diff = $"{s.BaseAttr.ToLower()}/{s.Difficulty.ToLower()}";
            var obj  = new JsonObject
            {
                ["id"]         = NewId('s'),
                ["name"]       = name,
                ["difficulty"] = diff,
                ["points"]     = s.Cost,
            };
            if (!s.IsCustom)
            {
                obj["source"] = new JsonObject
                {
                    ["library"] = "/gcs_user_library",
                    ["path"]    = "Módulo Básico/Módulo Básico Perícias.skl",
                    ["id"]      = s.DefinitionId,
                };
            }
            if (spec is not null) obj["specialization"] = spec;
            arr.Add(obj);
        }
        return arr;
    }

    private static JsonArray BuildEquipment(CharacterDraft d)
    {
        var arr = new JsonArray();
        foreach (var e in d.Equipment)
        {
            var obj = new JsonObject
            {
                ["type"]        = "equipment",
                ["id"]          = NewId('e'),
                ["description"] = e.Name,
                ["value"]       = e.Value,
                ["weight"]      = e.Weight,
                ["quantity"]    = e.Quantity,
                ["equipped"]    = true,
            };
            if (!string.IsNullOrEmpty(e.TechLevel))
                obj["tech_level"] = e.TechLevel;
            if (!string.IsNullOrEmpty(e.Reference))
                obj["reference"] = e.Reference;
            arr.Add(obj);
        }
        return arr;
    }

    private const string PtbrAttributesJson = """
        [
          {
            "id": "st",
            "type": "integer",
            "name": "ST",
            "full_name": "Força",
            "base": "10",
            "cost_per_point": 10,
            "cost_adj_percent_per_sm": 10
          },
          {
            "id": "dx",
            "type": "integer",
            "name": "DX",
            "full_name": "Destreza",
            "base": "10",
            "cost_per_point": 20
          },
          {
            "id": "iq",
            "type": "integer",
            "name": "IQ",
            "full_name": "Inteligência",
            "base": "10",
            "cost_per_point": 20
          },
          {
            "id": "ht",
            "type": "integer",
            "name": "HT",
            "full_name": "Vitalidade",
            "base": "10",
            "cost_per_point": 10
          },
          {
            "id": "will",
            "type": "integer",
            "name": "Vontade",
            "full_name": "Vontade",
            "base": "$iq",
            "cost_per_point": 5
          },
          {
            "id": "fright_check",
            "type": "integer",
            "name": "Verif. de Pânico",
            "base": "$will",
            "cost_per_point": 2
          },
          {
            "id": "senses",
            "type": "secondary_separator",
            "name": "Sentidos"
          },
          {
            "id": "per",
            "type": "integer",
            "name": "Percepção",
            "full_name": "Percepção",
            "base": "$iq",
            "cost_per_point": 5
          },
          {
            "id": "vision",
            "type": "integer",
            "name": "Visão",
            "full_name": "Visão",
            "base": "$per",
            "cost_per_point": 2
          },
          {
            "id": "hearing",
            "type": "integer",
            "name": "Audição",
            "full_name": "Audição",
            "base": "$per",
            "cost_per_point": 2
          },
          {
            "id": "taste_smell",
            "type": "integer",
            "name": "Paladar/Olfato",
            "full_name": "Paladar/Olfato",
            "base": "$per",
            "cost_per_point": 2
          },
          {
            "id": "touch",
            "type": "integer",
            "name": "Tato",
            "full_name": "Tato",
            "base": "$per",
            "cost_per_point": 2
          },
          {
            "id": "movement",
            "type": "secondary_separator",
            "name": "Deslocamento"
          },
          {
            "id": "basic_speed",
            "type": "decimal",
            "name": "Velocidade Básica",
            "full_name": "Velocidade Básica",
            "base": "($dx + $ht) / 4",
            "cost_per_point": 20
          },
          {
            "id": "basic_move",
            "type": "integer",
            "name": "Deslocamento Básico",
            "full_name": "Deslocamento Básico",
            "base": "Math.floor($basic_speed)",
            "cost_per_point": 5
          },
          {
            "id": "highjump",
            "type": "integer_ref",
            "name": "Alt. Salto (cm)",
            "base": "((15 * Math.max(Math.max($basic_move, Math.floor(entity.skillLevel(\"jumping\") / 2)), $st / 4)) - 25) * entity.currentEncumbrance(false, true) * (2 ** Math.max(0, entity.traitLevel(\"super jump\")))"
          },
          {
            "id": "running_highjump",
            "type": "integer_ref",
            "name": "Alt. Salto ao correr",
            "base": "(((15 * Math.max(Math.max($basic_move, Math.floor(entity.skillLevel(\"jumping\") / 2)), $st / 4)) * (1 + Math.max(0, entity.traitLevel(\"enhanced move (ground)\")))) - 25) * entity.currentEncumbrance(false, true) * iff(entity.traitLevel(\"enhanced move (ground)\") < 1, 2, 1) * (2 ** Math.max(0, entity.traitLevel(\"super jump\")))"
          },
          {
            "id": "broadjump",
            "type": "integer_ref",
            "name": "Dist. Salto (cm)",
            "base": "((60 * Math.max(Math.max($basic_move, Math.floor(entity.skillLevel(\"jumping\") / 2)), $st / 4)) - 90) * entity.currentEncumbrance(false, true) * (2 ** Math.max(0, entity.traitLevel(\"super jump\")))"
          },
          {
            "id": "running_broadjump",
            "type": "integer_ref",
            "name": "Dist. Salto ao correr",
            "base": "(((60 * Math.max(Math.max($basic_move, Math.floor(entity.skillLevel(\"jumping\") / 2)), $st / 4)) * (1 + Math.max(0, entity.traitLevel(\"enhanced move (ground)\")))) - 90) * entity.currentEncumbrance(false, true) * iff(entity.traitLevel(\"enhanced move (ground)\") < 1, 2, 1) * (2 ** Math.max(0, entity.traitLevel(\"super jump\")))"
          },
          {
            "id": "fp",
            "type": "pool",
            "name": "PF",
            "full_name": "Pontos de Fadiga",
            "base": "$ht",
            "cost_per_point": 3,
            "thresholds": [
              {
                "state": "Inconsciente",
                "value": "-$fp",
                "ops": [ "halve_move", "halve_dodge", "halve_st" ]
              },
              {
                "state": "Colapsando",
                "value": "0",
                "explanation": "Roll vs. Will to do anything besides talk or rest; failure causes unconsciousness\nEach FP you lose below 0 also causes 1 HP of injury\nMove, Dodge and ST are halved (B426)",
                "ops": [ "halve_move", "halve_dodge", "halve_st" ]
              },
              {
                "state": "Cansado",
                "value": "Math.round($fp / 3)",
                "explanation": "Move, Dodge and ST are halved (B426)",
                "ops": [ "halve_move", "halve_dodge", "halve_st" ]
              },
              {
                "state": "Cansando",
                "value": "$fp - 1"
              },
              {
                "state": "Descansado",
                "value": "$fp"
              }
            ]
          },
          {
            "id": "hp",
            "type": "pool",
            "name": "PV",
            "full_name": "Pontos de Vida",
            "base": "$st",
            "cost_per_point": 2,
            "cost_adj_percent_per_sm": 10,
            "thresholds": [
              {
                "state": "Morto",
                "value": "Math.round(-$hp * 5)",
                "ops": [ "halve_move", "halve_dodge" ]
              },
              {
                "state": "Morrendo #4",
                "value": "Math.round(-$hp * 4)",
                "explanation": "Roll vs. HT to avoid death\nRoll vs. HT-4 every second to avoid falling unconscious\nMove and Dodge are halved (B419)",
                "ops": [ "halve_move", "halve_dodge" ]
              },
              {
                "state": "Morrendo #3",
                "value": "Math.round(-$hp * 3)",
                "explanation": "Roll vs. HT to avoid death\nRoll vs. HT-3 every second to avoid falling unconscious\nMove and Dodge are halved (B419)",
                "ops": [ "halve_move", "halve_dodge" ]
              },
              {
                "state": "Morrendo #2",
                "value": "Math.round(-$hp * 2)",
                "explanation": "Roll vs. HT to avoid death\nRoll vs. HT-2 every second to avoid falling unconscious\nMove and Dodge are halved (B419)",
                "ops": [ "halve_move", "halve_dodge" ]
              },
              {
                "state": "Morrendo #1",
                "value": "-$hp",
                "explanation": "Roll vs. HT to avoid death\nRoll vs. HT-1 every second to avoid falling unconscious\nMove and Dodge are halved (B419)",
                "ops": [ "halve_move", "halve_dodge" ]
              },
              {
                "state": "Colapsando",
                "value": "0",
                "explanation": "Roll vs. HT every second to avoid falling unconscious\nMove and Dodge are halved (B419)",
                "ops": [ "halve_move", "halve_dodge" ]
              },
              {
                "state": "Muito Ferido",
                "value": "Math.round($hp / 3)",
                "explanation": "Move and Dodge are halved (B419)",
                "ops": [ "halve_move", "halve_dodge" ]
              },
              {
                "state": "Machucado",
                "value": "$hp - 1"
              },
              {
                "state": "Saudável",
                "value": "$hp"
              }
            ]
          }
        ]
        """;
}
