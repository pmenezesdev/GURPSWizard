namespace GurpsWizard.Core.Models;

/// <summary>Estado completo do personagem sendo criado no wizard.</summary>
public record CharacterDraft(
    string Name,
    string Description,
    int TotalPoints,
    Attributes Attributes,
    SecondaryAttributes SecondaryAttributes,
    List<TraitEntry> Advantages,
    List<TraitEntry> Disadvantages,
    List<SkillEntry> Skills,
    List<EquipmentEntry> Equipment
)
{
    public static CharacterDraft Empty() => new(
        Name: "",
        Description: "",
        TotalPoints: 100,
        Attributes: Attributes.Default,
        SecondaryAttributes: SecondaryAttributes.Default,
        Advantages: [],
        Disadvantages: [],
        Skills: [],
        Equipment: []
    );
}
