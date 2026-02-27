namespace GurpsWizard.Core.Models;

/// <summary>Item de equipamento selecionado para o personagem.</summary>
public record EquipmentEntry(
    string DefinitionId,
    string Name,
    decimal Value,
    string Weight,
    int Quantity = 1
);
