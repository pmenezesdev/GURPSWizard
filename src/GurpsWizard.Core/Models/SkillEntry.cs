namespace GurpsWizard.Core.Models;

/// <summary>
/// Perícia selecionada para o personagem.
/// <see cref="Level"/> é o nível relativo ao atributo base (pode ser negativo).
/// <see cref="Cost"/> é o custo em pontos calculado a partir de <see cref="Level"/>.
/// </summary>
public record SkillEntry(
    string DefinitionId,
    string Name,
    string BaseAttr,
    string Difficulty,
    int Level,
    int Cost,
    string? Reference = null,
    bool IsCustom = false
);
