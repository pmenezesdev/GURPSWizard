namespace GurpsWizard.Core.Models;

/// <summary>Vantagem ou desvantagem selecionada para o personagem.</summary>
public record TraitEntry(string DefinitionId, string Name, int Cost, int Level = 1, string? Reference = null, bool IsCustom = false);
