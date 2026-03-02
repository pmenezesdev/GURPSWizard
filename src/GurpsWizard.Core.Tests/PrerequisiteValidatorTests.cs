using FluentAssertions;
using GurpsWizard.Core.Models;
using GurpsWizard.Core.Services;

namespace GurpsWizard.Core.Tests;

public class PrerequisiteValidatorTests
{
    // ─────────────────────────────────────────────────────────────────────────
    // IsMet — TraitPrerequisite
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void IsMet_TraitPrereq_TraitInAdvantages_ReturnsTrue()
    {
        var prereq = new TraitPrerequisite("Reflexos em Combate");
        var draft = CharacterDraft.Empty() with
        {
            Advantages = [new TraitEntry("id1", "Reflexos em Combate", 15)]
        };

        PrerequisiteValidator.IsMet(prereq, draft).Should().BeTrue();
    }

    [Fact]
    public void IsMet_TraitPrereq_TraitInDisadvantages_ReturnsTrue()
    {
        var prereq = new TraitPrerequisite("Acrofobia");
        var draft = CharacterDraft.Empty() with
        {
            Disadvantages = [new TraitEntry("id1", "Acrofobia", -10)]
        };

        PrerequisiteValidator.IsMet(prereq, draft).Should().BeTrue();
    }

    [Fact]
    public void IsMet_TraitPrereq_TraitAbsent_ReturnsFalse()
    {
        var prereq = new TraitPrerequisite("Reflexos em Combate");
        var draft = CharacterDraft.Empty();

        PrerequisiteValidator.IsMet(prereq, draft).Should().BeFalse();
    }

    [Fact]
    public void IsMet_TraitPrereq_HasFalse_TraitPresent_ReturnsFalse()
    {
        // Has=false significa "incompatível com" — presença invalida
        var prereq = new TraitPrerequisite("Acrofobia", Has: false);
        var draft = CharacterDraft.Empty() with
        {
            Disadvantages = [new TraitEntry("id1", "Acrofobia", -10)]
        };

        PrerequisiteValidator.IsMet(prereq, draft).Should().BeFalse();
    }

    [Fact]
    public void IsMet_TraitPrereq_HasFalse_TraitAbsent_ReturnsTrue()
    {
        var prereq = new TraitPrerequisite("Acrofobia", Has: false);
        var draft = CharacterDraft.Empty();

        PrerequisiteValidator.IsMet(prereq, draft).Should().BeTrue();
    }

    [Fact]
    public void IsMet_TraitPrereq_CaseInsensitive_ReturnsTrue()
    {
        var prereq = new TraitPrerequisite("reflexos em combate");
        var draft = CharacterDraft.Empty() with
        {
            Advantages = [new TraitEntry("id1", "Reflexos em Combate", 15)]
        };

        PrerequisiteValidator.IsMet(prereq, draft).Should().BeTrue();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // IsMet — SkillPrerequisite
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void IsMet_SkillPrereq_SkillPresentByName_ReturnsTrue()
    {
        var prereq = new SkillPrerequisite("Medicina");
        var draft = CharacterDraft.Empty() with
        {
            Skills = [new SkillEntry("sk1", "Medicina", "IQ", "H", Level: 0, Cost: 4)]
        };

        PrerequisiteValidator.IsMet(prereq, draft).Should().BeTrue();
    }

    [Fact]
    public void IsMet_SkillPrereq_SkillAbsent_ReturnsFalse()
    {
        var prereq = new SkillPrerequisite("Medicina");
        var draft = CharacterDraft.Empty();

        PrerequisiteValidator.IsMet(prereq, draft).Should().BeFalse();
    }

    [Fact]
    public void IsMet_SkillPrereq_WithSpecialization_FullNameMatch_ReturnsTrue()
    {
        // Skill armazenada como "Medicina (Cirurgia)" no draft
        var prereq = new SkillPrerequisite("Medicina", Specialization: "Cirurgia");
        var draft = CharacterDraft.Empty() with
        {
            Skills = [new SkillEntry("sk1", "Medicina (Cirurgia)", "IQ", "H", Level: 0, Cost: 4)]
        };

        PrerequisiteValidator.IsMet(prereq, draft).Should().BeTrue();
    }

    [Fact]
    public void IsMet_SkillPrereq_WithSpecialization_BaseNameOnly_ReturnsFalse()
    {
        // Pré-requisito exige especialização, mas draft só tem o nome base
        var prereq = new SkillPrerequisite("Medicina", Specialization: "Cirurgia");
        var draft = CharacterDraft.Empty() with
        {
            Skills = [new SkillEntry("sk1", "Medicina", "IQ", "H", Level: 0, Cost: 4)]
        };

        PrerequisiteValidator.IsMet(prereq, draft).Should().BeFalse();
    }

    [Fact]
    public void IsMet_SkillPrereq_HasFalse_SkillPresent_ReturnsFalse()
    {
        var prereq = new SkillPrerequisite("Medicina", Has: false);
        var draft = CharacterDraft.Empty() with
        {
            Skills = [new SkillEntry("sk1", "Medicina", "IQ", "H", Level: 0, Cost: 4)]
        };

        PrerequisiteValidator.IsMet(prereq, draft).Should().BeFalse();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // IsMet — AttributePrerequisite
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void IsMet_AttributePrereq_ST_AtLeast12_ST12_ReturnsTrue()
    {
        var prereq = new AttributePrerequisite("ST", 12, "at_least");
        var draft = CharacterDraft.Empty() with
        {
            Attributes = Attributes.Default with { ST = 12 }
        };

        PrerequisiteValidator.IsMet(prereq, draft).Should().BeTrue();
    }

    [Fact]
    public void IsMet_AttributePrereq_ST_AtLeast12_ST11_ReturnsFalse()
    {
        var prereq = new AttributePrerequisite("ST", 12, "at_least");
        var draft = CharacterDraft.Empty() with
        {
            Attributes = Attributes.Default with { ST = 11 }
        };

        PrerequisiteValidator.IsMet(prereq, draft).Should().BeFalse();
    }

    [Fact]
    public void IsMet_AttributePrereq_IQ_AtMost14_IQ14_ReturnsTrue()
    {
        var prereq = new AttributePrerequisite("IQ", 14, "at_most");
        var draft = CharacterDraft.Empty() with
        {
            Attributes = Attributes.Default with { IQ = 14 }
        };

        PrerequisiteValidator.IsMet(prereq, draft).Should().BeTrue();
    }

    [Fact]
    public void IsMet_AttributePrereq_IQ_AtMost14_IQ15_ReturnsFalse()
    {
        var prereq = new AttributePrerequisite("IQ", 14, "at_most");
        var draft = CharacterDraft.Empty() with
        {
            Attributes = Attributes.Default with { IQ = 15 }
        };

        PrerequisiteValidator.IsMet(prereq, draft).Should().BeFalse();
    }

    [Fact]
    public void IsMet_AttributePrereq_UnknownAttribute_DefaultsTo10_ReturnsTrue()
    {
        // Atributo desconhecido tem valor padrão 10
        var prereq = new AttributePrerequisite("MANA", 5, "at_least");
        var draft = CharacterDraft.Empty();

        // Valor padrão 10 >= 5 → true
        PrerequisiteValidator.IsMet(prereq, draft).Should().BeTrue();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // IsMet — PrerequisiteGroup (AND / OR)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void IsMet_AndGroup_AllConditionsMet_ReturnsTrue()
    {
        var group = new PrerequisiteGroup(RequireAll: true, Items: [
            new TraitPrerequisite("Reflexos em Combate"),
            new SkillPrerequisite("Espadas"),
        ]);

        var draft = CharacterDraft.Empty() with
        {
            Advantages = [new TraitEntry("v1", "Reflexos em Combate", 15)],
            Skills = [new SkillEntry("sk1", "Espadas", "DX", "A", Level: 0, Cost: 2)],
        };

        PrerequisiteValidator.IsMet(group, draft).Should().BeTrue();
    }

    [Fact]
    public void IsMet_AndGroup_OneConditionMissing_ReturnsFalse()
    {
        var group = new PrerequisiteGroup(RequireAll: true, Items: [
            new TraitPrerequisite("Reflexos em Combate"),
            new SkillPrerequisite("Espadas"),
        ]);

        // Só tem a vantagem, não tem a perícia
        var draft = CharacterDraft.Empty() with
        {
            Advantages = [new TraitEntry("v1", "Reflexos em Combate", 15)],
        };

        PrerequisiteValidator.IsMet(group, draft).Should().BeFalse();
    }

    [Fact]
    public void IsMet_OrGroup_AtLeastOneMet_ReturnsTrue()
    {
        var group = new PrerequisiteGroup(RequireAll: false, Items: [
            new TraitPrerequisite("Reflexos em Combate"),
            new SkillPrerequisite("Espadas"),
        ]);

        // Só tem a perícia, não tem a vantagem
        var draft = CharacterDraft.Empty() with
        {
            Skills = [new SkillEntry("sk1", "Espadas", "DX", "A", Level: 0, Cost: 2)],
        };

        PrerequisiteValidator.IsMet(group, draft).Should().BeTrue();
    }

    [Fact]
    public void IsMet_OrGroup_NoneMetm_ReturnsFalse()
    {
        var group = new PrerequisiteGroup(RequireAll: false, Items: [
            new TraitPrerequisite("Reflexos em Combate"),
            new SkillPrerequisite("Espadas"),
        ]);

        var draft = CharacterDraft.Empty();

        PrerequisiteValidator.IsMet(group, draft).Should().BeFalse();
    }

    [Fact]
    public void IsMet_NestedGroup_AndInsideOr_EvaluatesCorrectly()
    {
        // OR de [ (AND de [TraitA, TraitB]), TraitC ]
        // Draft tem TraitA e TraitB → inner AND = true → outer OR = true
        var innerAnd = new PrerequisiteGroup(RequireAll: true, Items: [
            new TraitPrerequisite("TraitA"),
            new TraitPrerequisite("TraitB"),
        ]);
        var outerOr = new PrerequisiteGroup(RequireAll: false, Items: [
            innerAnd,
            new TraitPrerequisite("TraitC"),
        ]);

        var draft = CharacterDraft.Empty() with
        {
            Advantages = [
                new TraitEntry("a1", "TraitA", 5),
                new TraitEntry("a2", "TraitB", 5),
            ]
        };

        PrerequisiteValidator.IsMet(outerOr, draft).Should().BeTrue();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Describe
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Describe_TraitPrereq_Has_FormatRequer()
    {
        var prereq = new TraitPrerequisite("Reflexos em Combate");
        PrerequisiteValidator.Describe(prereq).Should().Be("Requer: Reflexos em Combate");
    }

    [Fact]
    public void Describe_TraitPrereq_HasFalse_FormatIncompativel()
    {
        var prereq = new TraitPrerequisite("Acrofobia", Has: false);
        PrerequisiteValidator.Describe(prereq).Should().Be("Incompatível com: Acrofobia");
    }

    [Fact]
    public void Describe_SkillPrereq_WithSpecialization_IncludesParens()
    {
        var prereq = new SkillPrerequisite("Medicina", Specialization: "Cirurgia");
        PrerequisiteValidator.Describe(prereq).Should().Be("Requer perícia: Medicina (Cirurgia)");
    }

    [Fact]
    public void Describe_SkillPrereq_WithoutSpecialization_NoParens()
    {
        var prereq = new SkillPrerequisite("Espadas");
        PrerequisiteValidator.Describe(prereq).Should().Be("Requer perícia: Espadas");
    }

    [Fact]
    public void Describe_AttributePrereq_AtLeast_UsesGeSymbol()
    {
        var prereq = new AttributePrerequisite("IQ", 14, "at_least");
        PrerequisiteValidator.Describe(prereq).Should().Be("Requer IQ ≥ 14");
    }

    [Fact]
    public void Describe_AttributePrereq_AtMost_UsesLeSymbol()
    {
        var prereq = new AttributePrerequisite("ST", 8, "at_most");
        PrerequisiteValidator.Describe(prereq).Should().Be("Requer ST ≤ 8");
    }

    [Fact]
    public void Describe_AndGroup_JoinsWithE()
    {
        var group = new PrerequisiteGroup(RequireAll: true, Items: [
            new TraitPrerequisite("TraitA"),
            new TraitPrerequisite("TraitB"),
        ]);

        PrerequisiteValidator.Describe(group).Should().Contain(" E ");
    }

    [Fact]
    public void Describe_OrGroup_JoinsWithOU()
    {
        var group = new PrerequisiteGroup(RequireAll: false, Items: [
            new TraitPrerequisite("TraitA"),
            new TraitPrerequisite("TraitB"),
        ]);

        PrerequisiteValidator.Describe(group).Should().Contain(" OU ");
    }
}
