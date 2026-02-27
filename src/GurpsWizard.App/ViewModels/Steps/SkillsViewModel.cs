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
    [Reactive] public ObservableCollection<LibrarySkill> SearchResults { get; private set; } = [];
    [Reactive] public LibrarySkill? SelectedLibrarySkill { get; set; }
    [Reactive] public ObservableCollection<SkillEntry> AddedSkills { get; private set; } = [];
    [Reactive] public SkillEntry? SelectedAddedSkill { get; set; }

    [Reactive] public int RelativeLevel { get; set; } = 0;
    [Reactive] public int PreviewCost { get; private set; } = 1;

    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> AddCommand { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> RemoveSelectedCommand { get; }

    public SkillsViewModel(WizardViewModel wizard, ILibraryRepository repo)
    {
        _wizard = wizard;
        _repo   = repo;

        wizard.WhenAnyValue(x => x.Draft)
              .Subscribe(d => AddedSkills = new ObservableCollection<SkillEntry>(d.Skills));

        this.WhenAnyValue(x => x.SearchQuery)
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
    }

    private async Task SearchAsync()
    {
        var results = await _repo.SearchSkillsAsync(SearchQuery);
        SearchResults = new ObservableCollection<LibrarySkill>(results);
    }

    private Task AddSelectedAsync()
    {
        if (SelectedLibrarySkill is null) return Task.CompletedTask;

        var skill = SelectedLibrarySkill;
        var diff  = skill.Difficulty;
        var cost  = PointCalculator.SkillCostFromDifficulty(diff, RelativeLevel);
        var entry = new SkillEntry(skill.GcsId, skill.Name, skill.BaseAttribute, diff, RelativeLevel, cost);
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
