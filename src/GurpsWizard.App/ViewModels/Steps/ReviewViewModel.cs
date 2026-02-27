using GurpsWizard.App.ViewModels;
using GurpsWizard.Core.Models;
using GurpsWizard.Data.Repositories;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace GurpsWizard.App.ViewModels.Steps;

public class ReviewViewModel : ReactiveObject
{
    private readonly WizardViewModel _wizard;
    private readonly ICharacterRepository _characterRepo;

    [Reactive] public CharacterDraft Draft { get; private set; }
    [Reactive] public CharacterPoints Points { get; private set; }
    [Reactive] public string SaveStatus { get; private set; } = "";

    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> SaveCommand { get; }

    public ReviewViewModel(WizardViewModel wizard, ICharacterRepository characterRepo)
    {
        _wizard        = wizard;
        _characterRepo = characterRepo;

        Draft  = wizard.Draft;
        Points = wizard.Points;

        wizard.WhenAnyValue(x => x.Draft)
              .Subscribe(d => Draft = d);

        wizard.WhenAnyValue(x => x.Points)
              .Subscribe(p => Points = p);

        SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync);
    }

    private async Task SaveAsync()
    {
        SaveStatus = "Salvando...";
        try
        {
            var entity = await _characterRepo.SaveAsync(Draft, _wizard.CharacterId);
            _wizard.CharacterId = entity.Id;
            SaveStatus = "Personagem salvo com sucesso!";
        }
        catch (Exception ex)
        {
            SaveStatus = $"Erro ao salvar: {ex.Message}";
        }
    }
}
