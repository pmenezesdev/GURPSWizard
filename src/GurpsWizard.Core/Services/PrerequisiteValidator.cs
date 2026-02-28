using GurpsWizard.Core.Models;

namespace GurpsWizard.Core.Services;

/// <summary>
/// Validates whether a prerequisite tree is satisfied by a given CharacterDraft.
/// Also produces human-readable descriptions of prerequisites.
/// </summary>
public static class PrerequisiteValidator
{
    /// <summary>Returns true if the prerequisite is met by the given draft.</summary>
    public static bool IsMet(Prerequisite prereq, CharacterDraft draft)
    {
        return prereq switch
        {
            PrerequisiteGroup group => group.RequireAll
                ? group.Items.All(p => IsMet(p, draft))
                : group.Items.Any(p => IsMet(p, draft)),

            TraitPrerequisite tp => EvalTrait(tp, draft),
            SkillPrerequisite sp => EvalSkill(sp, draft),
            AttributePrerequisite ap => EvalAttribute(ap, draft),
            _ => true,
        };
    }

    /// <summary>Returns a human-readable description of the prerequisite tree.</summary>
    public static string Describe(Prerequisite prereq)
    {
        return prereq switch
        {
            PrerequisiteGroup group => DescribeGroup(group),
            TraitPrerequisite tp => DescribeTrait(tp),
            SkillPrerequisite sp => DescribeSkill(sp),
            AttributePrerequisite ap => DescribeAttribute(ap),
            _ => "",
        };
    }

    // ── Evaluation ──────────────────────────────────────────────────────────

    private static bool EvalTrait(TraitPrerequisite tp, CharacterDraft draft)
    {
        var has = draft.Advantages.Any(t => NameMatch(t.Name, tp.TraitName))
               || draft.Disadvantages.Any(t => NameMatch(t.Name, tp.TraitName));
        return tp.Has ? has : !has;
    }

    private static bool EvalSkill(SkillPrerequisite sp, CharacterDraft draft)
    {
        bool has;
        if (sp.Specialization is not null)
        {
            // Match "Name (Spec)" pattern in SkillEntry.Name
            var fullName = $"{sp.SkillName} ({sp.Specialization})";
            has = draft.Skills.Any(s => NameMatch(s.Name, fullName));
        }
        else
        {
            has = draft.Skills.Any(s => NameMatch(s.Name, sp.SkillName));
        }

        return sp.Has ? has : !has;
    }

    private static bool EvalAttribute(AttributePrerequisite ap, CharacterDraft draft)
    {
        var attrValue = GetAttributeValue(ap.Attribute, draft);
        var result = ap.Compare switch
        {
            "at_least" => attrValue >= ap.Qualifier,
            "at_most" => attrValue <= ap.Qualifier,
            _ => true,
        };
        return ap.Has ? result : !result;
    }

    private static int GetAttributeValue(string attr, CharacterDraft draft)
    {
        return attr switch
        {
            "ST" => draft.Attributes.ST,
            "DX" => draft.Attributes.DX,
            "IQ" => draft.Attributes.IQ,
            "HT" => draft.Attributes.HT,
            _ => 10, // unknown attributes default to 10
        };
    }

    private static bool NameMatch(string actual, string expected)
        => string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);

    // ── Description ─────────────────────────────────────────────────────────

    private static string DescribeGroup(PrerequisiteGroup group)
    {
        var descriptions = group.Items
            .Select(Describe)
            .Where(d => !string.IsNullOrEmpty(d))
            .ToList();

        if (descriptions.Count == 0) return "";
        if (descriptions.Count == 1) return descriptions[0];

        var joiner = group.RequireAll ? " E " : " OU ";
        return string.Join(joiner, descriptions);
    }

    private static string DescribeTrait(TraitPrerequisite tp)
        => tp.Has
            ? $"Requer: {tp.TraitName}"
            : $"Incompatível com: {tp.TraitName}";

    private static string DescribeSkill(SkillPrerequisite sp)
    {
        var name = sp.Specialization is not null
            ? $"{sp.SkillName} ({sp.Specialization})"
            : sp.SkillName;

        return sp.Has
            ? $"Requer perícia: {name}"
            : $"Incompatível com perícia: {name}";
    }

    private static string DescribeAttribute(AttributePrerequisite ap)
    {
        var op = ap.Compare switch
        {
            "at_least" => "≥",
            "at_most" => "≤",
            _ => ap.Compare,
        };
        var prefix = ap.Has ? "Requer" : "Incompatível:";
        return $"{prefix} {ap.Attribute} {op} {ap.Qualifier}";
    }
}
