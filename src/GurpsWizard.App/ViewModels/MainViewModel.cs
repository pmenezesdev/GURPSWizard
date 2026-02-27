using GurpsWizard.Data.Repositories;
using GurpsWizard.Data.Entities;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using GurpsWizard.Core.Models;
using GurpsWizard.App.Wizard;
using GurpsWizard.App.Wizard.Steps;

namespace GurpsWizard.App.ViewModels;

public class MainViewModel : ReactiveObject
{
    private readonly ILibraryRepository _libraryRepo;
    private readonly ICharacterRepository _characterRepo;

    [Reactive] public object? CurrentContent { get; private set; }

    public MainViewModel(ILibraryRepository libraryRepo, ICharacterRepository characterRepo)
    {
        _libraryRepo   = libraryRepo;
        _characterRepo = characterRepo;

        ShowHome();
    }

    public void ShowHome()
    {
        CurrentContent = new HomeViewModel(this);
    }

    public void ShowCharacterList()
    {
        CurrentContent = new CharacterListViewModel(this, _characterRepo);
    }

    public void StartNewCharacter()
    {
        var engine = new WizardEngine(
        [
            new ConceptStep(),
            new AttributesStep(),
            new SecondaryStep(),
            new AdvantagesStep(),
            new DisadvantagesStep(),
            new SkillsStep(),
            new EquipmentStep(),
            new ReviewStep(),
        ]);

        CurrentContent = new WizardViewModel(engine, _libraryRepo, _characterRepo, this);
    }

    public void ImportDraft(CharacterDraft draft)
    {
        var engine = new WizardEngine(
        [
            new ConceptStep(),
            new AttributesStep(),
            new SecondaryStep(),
            new AdvantagesStep(),
            new DisadvantagesStep(),
            new SkillsStep(),
            new EquipmentStep(),
            new ReviewStep(),
        ]);

        CurrentContent = new WizardViewModel(engine, _libraryRepo, _characterRepo, this, draft, characterId: null);
    }

    public void LoadCharacter(CharacterEntity entity, CharacterDraft draft)
    {
        var engine = new WizardEngine(
        [
            new ConceptStep(),
            new AttributesStep(),
            new SecondaryStep(),
            new AdvantagesStep(),
            new DisadvantagesStep(),
            new SkillsStep(),
            new EquipmentStep(),
            new ReviewStep(),
        ]);

        CurrentContent = new WizardViewModel(engine, _libraryRepo, _characterRepo, this, draft, entity.Id);
    }
}
