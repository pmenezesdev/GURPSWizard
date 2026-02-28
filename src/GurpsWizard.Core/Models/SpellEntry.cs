namespace GurpsWizard.Core.Models;

/// <summary>
/// Mágica selecionada para o personagem.
/// <see cref="Level"/> é o nível relativo ao IQ (mesmo cálculo de custo que perícias).
/// </summary>
public record SpellEntry(
    string DefinitionId,
    string Name,
    string College,
    string Difficulty,
    int Level,
    int Cost,
    string? Reference = null,
    bool IsCustom = false
);
