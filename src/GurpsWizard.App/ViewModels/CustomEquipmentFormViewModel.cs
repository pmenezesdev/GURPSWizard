using GurpsWizard.Core.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace GurpsWizard.App.ViewModels;

/// <summary>
/// Formulário para criação de equipamento personalizado.
/// </summary>
public class CustomEquipmentFormViewModel : ReactiveObject
{
    [Reactive] public string Name { get; set; } = "";
    [Reactive] public decimal Value { get; set; }
    [Reactive] public string Weight { get; set; } = "0 kg";
    [Reactive] public string? TechLevel { get; set; }
    [Reactive] public int Quantity { get; set; } = 1;
    [Reactive] public string? Reference { get; set; }

    public ReactiveCommand<System.Reactive.Unit, EquipmentEntry?> CreateCommand { get; }

    public CustomEquipmentFormViewModel()
    {
        var canCreate = this.WhenAnyValue(x => x.Name)
            .Select(n => !string.IsNullOrWhiteSpace(n));

        CreateCommand = ReactiveCommand.Create<EquipmentEntry?>(() =>
        {
            var entry = new EquipmentEntry(
                DefinitionId: $"custom-{Guid.NewGuid()}",
                Name: Name.Trim(),
                Value: Value,
                Weight: string.IsNullOrWhiteSpace(Weight) ? "0 kg" : Weight.Trim(),
                TechLevel: string.IsNullOrWhiteSpace(TechLevel) ? null : TechLevel.Trim(),
                Quantity: Quantity < 1 ? 1 : Quantity,
                Reference: string.IsNullOrWhiteSpace(Reference) ? null : Reference.Trim(),
                IsCustom: true
            );

            // Reset form
            Name = "";
            Value = 0;
            Weight = "0 kg";
            TechLevel = null;
            Quantity = 1;
            Reference = null;

            return entry;
        }, canCreate);
    }
}
