using System.Collections.ObjectModel;
using GurpsWizard.App.ViewModels;
using GurpsWizard.Core.Models;
using GurpsWizard.Core.Services;
using GurpsWizard.Data.Entities;
using GurpsWizard.Data.Repositories;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace GurpsWizard.App.ViewModels.Steps;

public class SkillsViewModel : ReactiveObject
{
    private readonly WizardViewModel _wizard;
    private readonly ILibraryRepository _repo;

    [Reactive] public string SearchQuery { get; set; } = "";
    [Reactive] public string? SelectedAttribute { get; set; }
    [Reactive] public string? SelectedDifficulty { get; set; }
    [Reactive] public string? SelectedCategory { get; set; }
    [Reactive] public ObservableCollection<string> Categories { get; private set; } = [];
    [Reactive] public ObservableCollection<LibrarySkill> SearchResults { get; private set; } = [];
    [Reactive] public LibrarySkill? SelectedLibrarySkill { get; set; }
    [Reactive] public ObservableCollection<SkillEntry> AddedSkills { get; private set; } = [];
    [Reactive] public SkillEntry? SelectedAddedSkill { get; set; }

    [Reactive] public int RelativeLevel { get; set; } = 0;
    [Reactive] public int PreviewCost { get; private set; } = 1;
    [Reactive] public bool HasNoSkills { get; private set; } = true;

    public IReadOnlyList<string?> Attributes { get; } = [null, "ST", "DX", "IQ", "HT"];
    public IReadOnlyList<string?> Difficulties { get; } = [null, "E", "A", "H", "VH"];

    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> AddCommand { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> RemoveSelectedCommand { get; }
    public ReactiveCommand<object, System.Reactive.Unit> OpenReferenceCommand { get; }

    public SkillsViewModel(WizardViewModel wizard, ILibraryRepository repo)
    {
        _wizard = wizard;
        _repo   = repo;

        wizard.WhenAnyValue(x => x.Draft)
              .Subscribe(d =>
              {
                  AddedSkills = new ObservableCollection<SkillEntry>(d.Skills);
                  HasNoSkills = d.Skills.Count == 0;
              });

        this.WhenAnyValue(x => x.SearchQuery, x => x.SelectedAttribute, x => x.SelectedDifficulty, x => x.SelectedCategory)
            .Throttle(TimeSpan.FromMilliseconds(250))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(async _ => await SearchAsync());

        // Recalcular preview do custo quando mudar o nível ou a perícia selecionada (dificuldade)
        this.WhenAnyValue(x => x.RelativeLevel, x => x.SelectedLibrarySkill)
            .Subscribe(t =>
            {
                var diff = t.Item2?.Difficulty ?? "E";
                PreviewCost = PointCalculator.SkillCostFromDifficulty(diff, t.Item1);
            });

        var canAdd    = this.WhenAnyValue(x => x.SelectedLibrarySkill).Select(s => s is not null);
        var canRemove = this.WhenAnyValue(x => x.SelectedAddedSkill)  .Select(s => s is not null);

        AddCommand           = ReactiveCommand.CreateFromTask(AddSelectedAsync, canAdd);
        RemoveSelectedCommand = ReactiveCommand.Create(RemoveSelected, canRemove);
        OpenReferenceCommand = ReactiveCommand.Create<object>(param =>
        {
            if (param is LibrarySkill ls) PdfService.OpenReference(ls.Reference, ls.DisplayName);
            else if (param is SkillEntry se) PdfService.OpenReference(se.Reference, se.Name);
        });

        _ = LoadCategoriesAsync();
    }

    private async Task LoadCategoriesAsync()
    {
        var cats = await _repo.GetSkillCategoriesAsync();
        var list = new List<string> { "Tudo" };
        list.AddRange(cats);
        Categories = new ObservableCollection<string>(list);
        SelectedCategory = "Tudo";
    }

    private async Task SearchAsync()
    {
        var effectiveQuery = SearchSynonyms.Expand(SearchQuery);
        var category = SelectedCategory == "Tudo" ? null : SelectedCategory;
        var results = await _repo.SearchSkillsAsync(effectiveQuery, SelectedAttribute, SelectedDifficulty, category);
        SearchResults = new ObservableCollection<LibrarySkill>(results);
    }

    private Task AddSelectedAsync()
    {
        if (SelectedLibrarySkill is null) return Task.CompletedTask;

        var skill = SelectedLibrarySkill;
        var diff  = skill.Difficulty;
        var cost  = PointCalculator.SkillCostFromDifficulty(diff, RelativeLevel);
        var entry = new SkillEntry(skill.GcsId, skill.DisplayName, skill.BaseAttribute, diff, RelativeLevel, cost, skill.Reference);
        var newList = new List<SkillEntry>(_wizard.Draft.Skills) { entry };

        _wizard.Draft = _wizard.Draft with { Skills = newList };
        return Task.CompletedTask;
    }

    private void RemoveSelected()
    {
        var entry = SelectedAddedSkill;
        if (entry is null) return;

        var newList   = _wizard.Draft.Skills.Where(s => s != entry).ToList();
        _wizard.Draft = _wizard.Draft with { Skills = newList };
        SelectedAddedSkill = null;
    }
}
