namespace GurpsWizard.Core.Models;

/// <summary>Vantagem ou desvantagem selecionada para o personagem.</summary>
public record TraitEntry(string DefinitionId, string Name, int Cost, string? Reference = null);
