using System.Text.Json.Serialization;

namespace GurpsWizard.Core.Models;

/// <summary>Base type for all prerequisite nodes (discriminated union via inheritance).</summary>
[JsonDerivedType(typeof(AttributePrerequisite), "attribute")]
[JsonDerivedType(typeof(SkillPrerequisite), "skill")]
[JsonDerivedType(typeof(TraitPrerequisite), "trait")]
[JsonDerivedType(typeof(PrerequisiteGroup), "group")]
public abstract record Prerequisite;

/// <summary>Requires (or forbids) a specific attribute threshold.</summary>
public record AttributePrerequisite(
    string Attribute,
    int Qualifier,
    string Compare,
    bool Has = true
) : Prerequisite;

/// <summary>Requires (or forbids) a specific skill.</summary>
public record SkillPrerequisite(
    string SkillName,
    string? Specialization = null,
    bool Has = true
) : Prerequisite;

/// <summary>Requires (or forbids) a specific trait/advantage/disadvantage.</summary>
public record TraitPrerequisite(
    string TraitName,
    bool Has = true
) : Prerequisite;

/// <summary>A group of prerequisites combined with AND (RequireAll=true) or OR (RequireAll=false).</summary>
public record PrerequisiteGroup(
    bool RequireAll,
    IReadOnlyList<Prerequisite> Items
) : Prerequisite;
