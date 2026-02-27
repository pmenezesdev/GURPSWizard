using GurpsWizard.App.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace GurpsWizard.App.ViewModels.Steps;

public class ConceptViewModel : ReactiveObject
{
    private readonly WizardViewModel _wizard;
    private bool _syncing;

    [Reactive] public string Name { get; set; } = "";
    [Reactive] public string Description { get; set; } = "";
    [Reactive] public int TotalPoints { get; set; } = 100;
    [Reactive] public bool HasNameError { get; private set; } = true;

    public ConceptViewModel(WizardViewModel wizard)
    {
        _wizard = wizard;

        // Sincroniza propriedades locais sempre que o Draft mudar (ex.: ao voltar)
        wizard.WhenAnyValue(x => x.Draft)
              .Subscribe(d =>
              {
                  _syncing    = true;
                  Name        = d.Name;
                  Description = d.Description;
                  TotalPoints = d.TotalPoints;
                  _syncing    = false;
              });

        // Mantém HasNameError sincronizado com o campo Name
        this.WhenAnyValue(x => x.Name)
            .Subscribe(n => HasNameError = string.IsNullOrWhiteSpace(n));

        // Propaga mudanças locais de volta ao Draft
        this.WhenAnyValue(x => x.Name)
            .Where(_ => !_syncing)
            .Subscribe(v => _wizard.Draft = _wizard.Draft with { Name = v });

        this.WhenAnyValue(x => x.Description)
            .Where(_ => !_syncing)
            .Subscribe(v => _wizard.Draft = _wizard.Draft with { Description = v });

        this.WhenAnyValue(x => x.TotalPoints)
            .Where(_ => !_syncing)
            .Subscribe(v => _wizard.Draft = _wizard.Draft with { TotalPoints = v });
    }
}
