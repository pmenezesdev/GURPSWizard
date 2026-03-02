using System.Collections.ObjectModel;
using GurpsWizard.App.ViewModels;
using GurpsWizard.Core.Models;
using GurpsWizard.Core.Services;
using GurpsWizard.Data.Entities;
using GurpsWizard.Data.Repositories;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace GurpsWizard.App.ViewModels.Steps;

public class SpellsViewModel : ReactiveObject
{
    private readonly WizardViewModel _wizard;
    private readonly ILibraryRepository _repo;

    [Reactive] public string SearchQuery { get; set; } = "";
    [Reactive] public string? SelectedCollege { get; set; }
    [Reactive] public string? SelectedClass { get; set; }
    [Reactive] public string? SelectedDifficulty { get; set; }
    [Reactive] public ObservableCollection<string> Colleges { get; private set; } = [];
    [Reactive] public ObservableCollection<string> SpellClasses { get; private set; } = [];
    [Reactive] public ObservableCollection<LibrarySpell> SearchResults { get; private set; } = [];
    [Reactive] public LibrarySpell? SelectedLibrarySpell { get; set; }
    [Reactive] public ObservableCollection<SpellEntry> AddedSpells { get; private set; } = [];
    [Reactive] public SpellEntry? SelectedAddedSpell { get; set; }

    [Reactive] public int RelativeLevel { get; set; } = 0;
    [Reactive] public int PreviewCost { get; private set; } = 1;

    public IReadOnlyList<string?> Difficulties { get; } = [null, "H", "VH"];

    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> AddCommand { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> RemoveSelectedCommand { get; }
    public ReactiveCommand<object, System.Reactive.Unit> OpenReferenceCommand { get; }

    public SpellsViewModel(WizardViewModel wizard, ILibraryRepository repo)
    {
        _wizard = wizard;
        _repo   = repo;

        wizard.WhenAnyValue(x => x.Draft)
              .Subscribe(d =>
              {
                  AddedSpells = new ObservableCollection<SpellEntry>(d.Spells ?? []);
              });

        this.WhenAnyValue(x => x.SearchQuery, x => x.SelectedCollege, x => x.SelectedClass, x => x.SelectedDifficulty)
            .Throttle(TimeSpan.FromMilliseconds(250))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(async _ => await SearchAsync());

        this.WhenAnyValue(x => x.RelativeLevel, x => x.SelectedLibrarySpell)
            .Subscribe(t =>
            {
                var diff = t.Item2?.Difficulty ?? "H";
                PreviewCost = PointCalculator.SkillCostFromDifficulty(diff, t.Item1);
            });

        var canAdd    = this.WhenAnyValue(x => x.SelectedLibrarySpell).Select(s => s is not null);
        var canRemove = this.WhenAnyValue(x => x.SelectedAddedSpell)  .Select(s => s is not null);

        AddCommand            = ReactiveCommand.Create(AddSelected, canAdd);
        RemoveSelectedCommand = ReactiveCommand.Create(RemoveSelected, canRemove);
        OpenReferenceCommand  = ReactiveCommand.Create<object>(param =>
        {
            if (param is LibrarySpell ls) PdfService.OpenReference(ls.Reference, ls.Name);
            else if (param is SpellEntry se) PdfService.OpenReference(se.Reference, se.Name);
        });

        _ = LoadFiltersAsync();
    }

    private async Task LoadFiltersAsync()
    {
        var colleges = await _repo.GetCollegesAsync();
        var allColleges = new List<string> { "Todas as escolas" };
        allColleges.AddRange(colleges);
        Colleges = new ObservableCollection<string>(allColleges);
        SelectedCollege = "Todas as escolas";

        var classes = await _repo.GetSpellClassesAsync();
        var allClasses = new List<string> { "Todas as classes" };
        allClasses.AddRange(classes);
        SpellClasses = new ObservableCollection<string>(allClasses);
        SelectedClass = "Todas as classes";
    }

    private async Task SearchAsync()
    {
        var college    = SelectedCollege    == "Todas as escolas" ? null : SelectedCollege;
        var spellClass = SelectedClass      == "Todas as classes" ? null : SelectedClass;
        var results    = await _repo.SearchSpellsAsync(SearchQuery, college, spellClass, SelectedDifficulty);
        SearchResults = new ObservableCollection<LibrarySpell>(results);
    }

    private void AddSelected()
    {
        if (SelectedLibrarySpell is null) return;

        var spell   = SelectedLibrarySpell;
        var cost    = PointCalculator.SkillCostFromDifficulty(spell.Difficulty, RelativeLevel);
        var entry   = new SpellEntry(spell.GcsId, spell.Name, spell.College, spell.Difficulty, RelativeLevel, cost, spell.Reference);
        var newList = new List<SpellEntry>(_wizard.Draft.Spells ?? []) { entry };

        _wizard.Draft = _wizard.Draft with { Spells = newList };
    }

    private void RemoveSelected()
    {
        var entry = SelectedAddedSpell;
        if (entry is null) return;

        var newList   = RemoveFirstOccurrence(_wizard.Draft.Spells ?? [], entry);
        _wizard.Draft = _wizard.Draft with { Spells = newList };
        SelectedAddedSpell = null;
    }

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
