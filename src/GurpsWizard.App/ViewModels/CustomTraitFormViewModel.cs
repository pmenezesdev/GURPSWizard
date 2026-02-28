using GurpsWizard.Core.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace GurpsWizard.App.ViewModels;

/// <summary>
/// Formulário para criação de vantagem/desvantagem personalizada.
/// </summary>
public class CustomTraitFormViewModel : ReactiveObject
{
    [Reactive] public string Name { get; set; } = "";
    [Reactive] public int Cost { get; set; }
    [Reactive] public int Level { get; set; } = 1;
    [Reactive] public string? Reference { get; set; }

    public ReactiveCommand<System.Reactive.Unit, TraitEntry?> CreateCommand { get; }

    public CustomTraitFormViewModel()
    {
        var canCreate = this.WhenAnyValue(x => x.Name)
            .Select(n => !string.IsNullOrWhiteSpace(n));

        CreateCommand = ReactiveCommand.Create<TraitEntry?>(() =>
        {
            var entry = new TraitEntry(
                DefinitionId: $"custom-{Guid.NewGuid()}",
                Name: Name.Trim(),
                Cost: Cost,
                Level: Level,
                Reference: string.IsNullOrWhiteSpace(Reference) ? null : Reference.Trim(),
                IsCustom: true
            );

            // Reset form
            Name = "";
            Cost = 0;
            Level = 1;
            Reference = null;

            return entry;
        }, canCreate);
    }
}
