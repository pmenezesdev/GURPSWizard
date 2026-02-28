using System.Collections.ObjectModel;
using GurpsWizard.App.ViewModels;
using GurpsWizard.Core.Models;
using GurpsWizard.Core.Services;
using GurpsWizard.Data.Entities;
using GurpsWizard.Data.Repositories;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace GurpsWizard.App.ViewModels.Steps;

public class TechniquesViewModel : ReactiveObject
{
    private readonly WizardViewModel _wizard;
    private readonly ILibraryRepository _repo;

    [Reactive] public string SearchQuery { get; set; } = "";
    [Reactive] public string? SelectedDifficulty { get; set; }
    [Reactive] public string? SelectedCategory { get; set; }
    [Reactive] public ObservableCollection<string> Categories { get; private set; } = [];
    [Reactive] public ObservableCollection<LibraryTechnique> SearchResults { get; private set; } = [];
    [Reactive] public LibraryTechnique? SelectedLibraryTechnique { get; set; }
    [Reactive] public ObservableCollection<TechniqueEntry> AddedTechniques { get; private set; } = [];
    [Reactive] public TechniqueEntry? SelectedAddedTechnique { get; set; }

    [Reactive] public int LevelsAboveDefault { get; set; } = 0;
    [Reactive] public int PreviewCost { get; private set; } = 0;

    public IReadOnlyList<string?> Difficulties { get; } = [null, "A", "H"];

    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> AddCommand { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> RemoveSelectedCommand { get; }
    public ReactiveCommand<object, System.Reactive.Unit> OpenReferenceCommand { get; }

    public TechniquesViewModel(WizardViewModel wizard, ILibraryRepository repo)
    {
        _wizard = wizard;
        _repo   = repo;

        wizard.WhenAnyValue(x => x.Draft)
              .Subscribe(d =>
              {
                  AddedTechniques = new ObservableCollection<TechniqueEntry>(d.Techniques ?? []);
              });

        this.WhenAnyValue(x => x.SearchQuery, x => x.SelectedDifficulty, x => x.SelectedCategory)
            .Throttle(TimeSpan.FromMilliseconds(250))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(async _ => await SearchAsync());

        this.WhenAnyValue(x => x.LevelsAboveDefault, x => x.SelectedLibraryTechnique)
            .Subscribe(t =>
            {
                var diff = t.Item2?.Difficulty ?? "A";
                PreviewCost = PointCalculator.TechniqueCost(diff, t.Item1);
            });

        var canAdd    = this.WhenAnyValue(x => x.SelectedLibraryTechnique).Select(t => t is not null);
        var canRemove = this.WhenAnyValue(x => x.SelectedAddedTechnique)  .Select(t => t is not null);

        AddCommand            = ReactiveCommand.Create(AddSelected, canAdd);
        RemoveSelectedCommand = ReactiveCommand.Create(RemoveSelected, canRemove);
        OpenReferenceCommand  = ReactiveCommand.Create<object>(param =>
        {
            if (param is LibraryTechnique lt) PdfService.OpenReference(lt.Reference, lt.DisplayName);
            else if (param is TechniqueEntry te) PdfService.OpenReference(te.Reference, te.Name);
        });

        _ = LoadCategoriesAsync();
    }

    private async Task LoadCategoriesAsync()
    {
        var cats = await _repo.GetTechniqueCategoriesAsync();
        var list = new List<string> { "Tudo" };
        list.AddRange(cats);
        Categories = new ObservableCollection<string>(list);
        SelectedCategory = "Tudo";
    }

    private async Task SearchAsync()
    {
        var category = SelectedCategory == "Tudo" ? null : SelectedCategory;
        var results = await _repo.SearchTechniquesAsync(SearchQuery, SelectedDifficulty, category);
        SearchResults = new ObservableCollection<LibraryTechnique>(results);
    }

    private void AddSelected()
    {
        if (SelectedLibraryTechnique is null) return;

        var tech = SelectedLibraryTechnique;
        var cost = PointCalculator.TechniqueCost(tech.Difficulty, LevelsAboveDefault);
        var entry = new TechniqueEntry(tech.GcsId, tech.DisplayName, tech.Difficulty, LevelsAboveDefault, cost, tech.Reference);
        var newList = new List<TechniqueEntry>(_wizard.Draft.Techniques ?? []) { entry };

        _wizard.Draft = _wizard.Draft with { Techniques = newList };
    }

    private void RemoveSelected()
    {
        var entry = SelectedAddedTechnique;
        if (entry is null) return;

        var newList   = (_wizard.Draft.Techniques ?? []).Where(t => t != entry).ToList();
        _wizard.Draft = _wizard.Draft with { Techniques = newList };
        SelectedAddedTechnique = null;
    }
}
