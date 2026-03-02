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
    [Reactive] public int SelectedLevel { get; set; } = 1;
    [Reactive] public bool SelectedCanLevel { get; private set; }
    [Reactive] public int PreviewCost { get; private set; }
    [Reactive] public ObservableCollection<TraitEntry> AddedTraits { get; private set; } = [];
    [Reactive] public TraitEntry? SelectedAddedTrait { get; set; }

    [Reactive] public bool ShowCustomForm { get; set; }
    public CustomTraitFormViewModel CustomTraitForm { get; }

    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> AddCommand { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> RemoveSelectedCommand { get; }
    public ReactiveCommand<object, System.Reactive.Unit> OpenReferenceCommand { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> ToggleCustomFormCommand { get; }

    public TraitsViewModel(WizardViewModel wizard, ILibraryRepository repo, bool isDisadvantage)
    {
        _wizard         = wizard;
        _repo           = repo;
        _isDisadvantage = isDisadvantage;

        CustomTraitForm = new CustomTraitFormViewModel();
        ToggleCustomFormCommand = ReactiveCommand.Create(() => { ShowCustomForm = !ShowCustomForm; });

        // Subscribe to custom trait creation
        CustomTraitForm.CreateCommand.Subscribe(entry =>
        {
            if (entry is null) return;
            var d       = _wizard.Draft;
            var newList = _isDisadvantage
                ? new List<TraitEntry>(d.Disadvantages) { entry }
                : new List<TraitEntry>(d.Advantages)    { entry };
            _wizard.Draft = _isDisadvantage
                ? d with { Disadvantages = newList }
                : d with { Advantages    = newList };
            ShowCustomForm = false;
        });

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

        // When selected trait changes, update CanLevel and reset level
        this.WhenAnyValue(x => x.SelectedLibraryTrait)
            .Subscribe(t =>
            {
                SelectedCanLevel = t?.CanLevel ?? false;
                SelectedLevel = 1;
                PreviewCost = t?.BasePoints ?? 0;
            });

        // Recompute preview cost when level changes
        this.WhenAnyValue(x => x.SelectedLevel)
            .Subscribe(level =>
            {
                if (SelectedLibraryTrait is { } t)
                    PreviewCost = t.CanLevel
                        ? t.BasePoints + (level - 1) * t.PointsPerLevel
                        : t.BasePoints;
            });

        var canAdd    = this.WhenAnyValue(x => x.SelectedLibraryTrait).Select(t => t is not null);
        var canRemove = this.WhenAnyValue(x => x.SelectedAddedTrait)  .Select(t => t is not null);

        AddCommand           = ReactiveCommand.CreateFromTask(AddSelectedAsync, canAdd);
        RemoveSelectedCommand = ReactiveCommand.Create(RemoveSelected, canRemove);
        OpenReferenceCommand = ReactiveCommand.Create<object>(param =>
        {
            if (param is LibraryTrait lt) PdfService.OpenReference(lt.Reference, lt.Name);
            else if (param is TraitEntry te) PdfService.OpenReference(te.Reference, te.Name);
        });

        _ = LoadCategoriesAsync();
    }

    private async Task SearchAsync()
    {
        var effectiveQuery = SearchSynonyms.Expand(SearchQuery);
        var category = SelectedCategory == "Tudo" ? null : SelectedCategory;
        var results = await _repo.SearchTraitsAsync(effectiveQuery, category, MaxAbsCost);
        var filtered = _isDisadvantage
            ? results.Where(t => t.BasePoints < 0)
            : results.Where(t => t.BasePoints >= 0);
        SearchResults = new ObservableCollection<LibraryTrait>(filtered);
    }

    private async Task LoadCategoriesAsync()
    {
        var cats = await _repo.GetTraitCategoriesAsync();
        var list = new List<string> { "Tudo" };
        list.AddRange(cats);
        Categories = new ObservableCollection<string>(list);
        SelectedCategory = "Tudo";
    }

    private Task AddSelectedAsync()
    {
        if (SelectedLibraryTrait is null) return Task.CompletedTask;

        var trait = SelectedLibraryTrait;
        var level = SelectedCanLevel ? SelectedLevel : 1;
        var cost  = trait.CanLevel
            ? trait.BasePoints + (level - 1) * trait.PointsPerLevel
            : trait.BasePoints;

        var entry   = new TraitEntry(trait.GcsId, trait.Name, cost, level, trait.Reference);
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

        var d    = _wizard.Draft;
        var src  = _isDisadvantage ? d.Disadvantages : d.Advantages;
        var newList = RemoveFirstOccurrence(src, entry);

        _wizard.Draft = _isDisadvantage
            ? d with { Disadvantages = newList }
            : d with { Advantages    = newList };

        SelectedAddedTrait = null;
    }

    // Remove apenas a primeira ocorrência de entry (records usam value equality,
    // então Where(t != entry) removeria todas as cópias com mesmo valor).
    private static List<T> RemoveFirstOccurrence<T>(IEnumerable<T> source, T entry)
    {
        bool removed = false;
        return source.Where(t =>
        {
            if (!removed && EqualityComparer<T>.Default.Equals(t, entry)) { removed = true; return false; }
            return true;
        }).ToList();
    }
}

/// <summary>Instância de TraitsViewModel para a etapa de Vantagens.</summary>
public class AdvantagesViewModel(WizardViewModel wizard, ILibraryRepository repo)
    : TraitsViewModel(wizard, repo, isDisadvantage: false);

/// <summary>Instância de TraitsViewModel para a etapa de Desvantagens.</summary>
public class DisadvantagesViewModel(WizardViewModel wizard, ILibraryRepository repo)
    : TraitsViewModel(wizard, repo, isDisadvantage: true);
