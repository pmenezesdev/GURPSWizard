using GurpsWizard.Core.Models;
using GurpsWizard.Core.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace GurpsWizard.App.ViewModels;

/// <summary>
/// Formulário para criação de perícia/técnica personalizada.
/// </summary>
public class CustomSkillFormViewModel : ReactiveObject
{
    [Reactive] public string Name { get; set; } = "";
    [Reactive] public string BaseAttribute { get; set; } = "DX";
    [Reactive] public string Difficulty { get; set; } = "A";
    [Reactive] public int RelativeLevel { get; set; }
    [Reactive] public int PreviewCost { get; private set; } = 1;

    public IReadOnlyList<string> BaseAttributes { get; } = ["ST", "DX", "IQ", "HT"];
    public IReadOnlyList<string> Difficulties { get; } = ["E", "A", "H", "VH"];

    public ReactiveCommand<System.Reactive.Unit, SkillEntry?> CreateCommand { get; }

    public CustomSkillFormViewModel()
    {
        // Recalculate preview cost when difficulty or level changes
        this.WhenAnyValue(x => x.Difficulty, x => x.RelativeLevel)
            .Subscribe(t => PreviewCost = PointCalculator.SkillCostFromDifficulty(t.Item1, t.Item2));

        var canCreate = this.WhenAnyValue(x => x.Name)
            .Select(n => !string.IsNullOrWhiteSpace(n));

        CreateCommand = ReactiveCommand.Create<SkillEntry?>(() =>
        {
            var cost = PointCalculator.SkillCostFromDifficulty(Difficulty, RelativeLevel);
            var entry = new SkillEntry(
                DefinitionId: $"custom-{Guid.NewGuid()}",
                Name: Name.Trim(),
                BaseAttr: BaseAttribute,
                Difficulty: Difficulty,
                Level: RelativeLevel,
                Cost: cost,
                Reference: null,
                IsCustom: true
            );

            // Reset form
            Name = "";
            BaseAttribute = "DX";
            Difficulty = "A";
            RelativeLevel = 0;

            return entry;
        }, canCreate);
    }
}
