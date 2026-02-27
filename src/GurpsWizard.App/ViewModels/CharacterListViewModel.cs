using System.Collections.ObjectModel;
using GurpsWizard.Core.Models;
using GurpsWizard.Data.Entities;
using GurpsWizard.Data.Repositories;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace GurpsWizard.App.ViewModels;

public class CharacterListViewModel : ReactiveObject
{
    private readonly MainViewModel _main;
    private readonly ICharacterRepository _characterRepo;

    public ObservableCollection<CharacterEntity> Characters { get; } = [];

    [Reactive] public CharacterEntity? SelectedCharacter { get; set; }
    [Reactive] public bool IsLoading { get; private set; }

    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> LoadSelectedCommand { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> DeleteSelectedCommand { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> BackCommand { get; }

    public CharacterListViewModel(MainViewModel main, ICharacterRepository characterRepo)
    {
        _main          = main;
        _characterRepo = characterRepo;

        var canExecuteSelected = this.WhenAnyValue(x => x.SelectedCharacter, (CharacterEntity? c) => c is not null);

        LoadSelectedCommand   = ReactiveCommand.CreateFromTask(LoadSelectedAsync, canExecuteSelected);
        DeleteSelectedCommand = ReactiveCommand.CreateFromTask(DeleteSelectedAsync, canExecuteSelected);
        BackCommand           = ReactiveCommand.Create(main.ShowHome);

        _ = LoadListAsync();
    }

    private async Task LoadListAsync()
    {
        IsLoading = true;
        try
        {
            var list = await _characterRepo.ListAllAsync();
            Characters.Clear();
            foreach (var c in list)
                Characters.Add(c);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadSelectedAsync()
    {
        if (SelectedCharacter is null) return;

        var draft = await _characterRepo.GetByIdAsync(SelectedCharacter.Id);
        if (draft is not null)
            _main.LoadCharacter(SelectedCharacter, draft);
    }

    private async Task DeleteSelectedAsync()
    {
        if (SelectedCharacter is null) return;
        await _characterRepo.DeleteAsync(SelectedCharacter.Id);
        Characters.Remove(SelectedCharacter);
        SelectedCharacter = null;
    }
}
