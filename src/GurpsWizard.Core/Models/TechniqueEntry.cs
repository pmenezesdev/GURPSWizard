namespace GurpsWizard.Core.Models;

/// <summary>
/// Técnica selecionada para o personagem.
/// <see cref="LevelsAboveDefault"/> é quantos níveis acima do valor predefinido.
/// <see cref="Cost"/> é o custo em pontos calculado pela Tabela de Custo das Técnicas (MB p.230).
/// </summary>
public record TechniqueEntry(
    string DefinitionId,
    string Name,
    string Difficulty,
    int LevelsAboveDefault,
    int Cost,
    string? Reference = null,
    bool IsCustom = false
);
