using System.Collections.ObjectModel;
using GurpsWizard.App.ViewModels;
using GurpsWizard.Core.Models;
using GurpsWizard.Data.Entities;
using GurpsWizard.Data.Repositories;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace GurpsWizard.App.ViewModels.Steps;

/// <summary>
/// ViewModel reutilizável para seleção de vantagens e desvantagens.
/// Instâncias concretas: <see cref="AdvantagesViewModel"/> e <see cref="DisadvantagesViewModel"/>.
/// </summary>
public class TraitsViewModel : ReactiveObject
{
    private readonly WizardViewModel _wizard;
    private readonly ILibraryRepository _repo;
    private readonly bool _isDisadvantage;

    [Reactive] public string SearchQuery { get; set; } = "";
    [Reactive] public string? SelectedCategory { get; set; }
    [Reactive] public int? MaxAbsCost { get; set; }
    [Reactive] public ObservableCollection<string> Categories { get; private set; } = [];
    [Reactive] public ObservableCollection<LibraryTrait> SearchResults { get; private set; } = [];
    [Reactive] public LibraryTrait? SelectedLibraryTrait { get; set; }
    [Reactive] public ObservableCollection<TraitEntry> AddedTraits { get; private set; } = [];
    [Reactive] public TraitEntry? SelectedAddedTrait { get; set; }

    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> AddCommand { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> RemoveSelectedCommand { get; }

    public TraitsViewModel(WizardViewModel wizard, ILibraryRepository repo, bool isDisadvantage)
    {
        _wizard         = wizard;
        _repo           = repo;
        _isDisadvantage = isDisadvantage;

        // Sincroniza lista ao receber Draft
        wizard.WhenAnyValue(x => x.Draft)
              .Subscribe(d =>
              {
                  var source = isDisadvantage ? d.Disadvantages : d.Advantages;
                  AddedTraits = new ObservableCollection<TraitEntry>(source);
              });

        // Busca ao mudar query, categoria ou custo máximo
        this.WhenAnyValue(x => x.SearchQuery, x => x.SelectedCategory)
            .Throttle(TimeSpan.FromMilliseconds(250))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(async _ => await SearchAsync());

        this.WhenAnyValue(x => x.MaxAbsCost)
            .Throttle(TimeSpan.FromMilliseconds(400))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(async _ => await SearchAsync());

        var canAdd    = this.WhenAnyValue(x => x.SelectedLibraryTrait).Select(t => t is not null);
        var canRemove = this.WhenAnyValue(x => x.SelectedAddedTrait)  .Select(t => t is not null);

        AddCommand           = ReactiveCommand.CreateFromTask(AddSelectedAsync, canAdd);
        RemoveSelectedCommand = ReactiveCommand.Create(RemoveSelected, canRemove);

        _ = LoadCategoriesAsync();
    }

    private async Task SearchAsync()
    {
        var effectiveQuery = SearchSynonyms.Expand(SearchQuery);
        var results = await _repo.SearchTraitsAsync(effectiveQuery, SelectedCategory, MaxAbsCost);
        var filtered = _isDisadvantage
            ? results.Where(t => t.BasePoints < 0)
            : results.Where(t => t.BasePoints >= 0);
        SearchResults = new ObservableCollection<LibraryTrait>(filtered);
    }

    private async Task LoadCategoriesAsync()
    {
        var cats = await _repo.GetTraitCategoriesAsync();
        Categories = new ObservableCollection<string>(cats);
    }

    private Task AddSelectedAsync()
    {
        if (SelectedLibraryTrait is null) return Task.CompletedTask;

        var trait   = SelectedLibraryTrait;
        var entry   = new TraitEntry(trait.GcsId, trait.Name, trait.BasePoints);
        var d       = _wizard.Draft;
        var newList = _isDisadvantage
            ? new List<TraitEntry>(d.Disadvantages) { entry }
            : new List<TraitEntry>(d.Advantages)    { entry };

        _wizard.Draft = _isDisadvantage
            ? d with { Disadvantages = newList }
            : d with { Advantages    = newList };

        return Task.CompletedTask;
    }

    private void RemoveSelected()
    {
        var entry = SelectedAddedTrait;
        if (entry is null) return;

        var d       = _wizard.Draft;
        var newList = _isDisadvantage
            ? d.Disadvantages.Where(t => t != entry).ToList()
            : d.Advantages   .Where(t => t != entry).ToList();

        _wizard.Draft = _isDisadvantage
            ? d with { Disadvantages = newList }
            : d with { Advantages    = newList };

        SelectedAddedTrait = null;
    }
}

/// <summary>Instância de TraitsViewModel para a etapa de Vantagens.</summary>
public class AdvantagesViewModel(WizardViewModel wizard, ILibraryRepository repo)
    : TraitsViewModel(wizard, repo, isDisadvantage: false);

/// <summary>Instância de TraitsViewModel para a etapa de Desvantagens.</summary>
public class DisadvantagesViewModel(WizardViewModel wizard, ILibraryRepository repo)
    : TraitsViewModel(wizard, repo, isDisadvantage: true);
