using System.Reactive.Linq;
using GurpsWizard.App.ViewModels.Steps;
using GurpsWizard.App.Wizard;
using GurpsWizard.App.Wizard.Steps;
using GurpsWizard.Core.Models;
using GurpsWizard.Core.Services;
using GurpsWizard.Data.Repositories;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace GurpsWizard.App.ViewModels;

public record SidebarStep(string Title, int Index, bool IsCompleted, bool IsCurrent)
{
    public bool IsPending => !IsCompleted && !IsCurrent;
}

/// <summary>
/// ViewModel raiz do wizard. Gerencia o CharacterDraft, a navegação entre etapas
/// e o cálculo de pontos.
/// </summary>
public class WizardViewModel : ReactiveObject
{
    private readonly WizardEngine _engine;
    private readonly Dictionary<Type, object> _stepViewModels;

    // ── Estado ───────────────────────────────────────────────────────────────
    [Reactive] public CharacterDraft Draft { get; set; }
    [Reactive] public int? CharacterId { get; set; }

    // ── Navegação ─────────────────────────────────────────────────────────────
    [Reactive] public IWizardStep CurrentStep { get; private set; }
    [Reactive] public object? CurrentStepViewModel { get; private set; }
    [Reactive] public int StepIndex { get; private set; }

    // ── Pontos ────────────────────────────────────────────────────────────────
    [Reactive] public CharacterPoints Points { get; private set; }

    // ── Estado dos botões ─────────────────────────────────────────────────────
    [Reactive] public bool CanGoBack { get; private set; }
    [Reactive] public bool CanGoNext { get; private set; }
    [Reactive] public double ProgressPercent { get; private set; }
    [Reactive] public bool IsOverBudget { get; private set; }

    // ── Sidebar steps com status ──────────────────────────────────────────────
    [Reactive] public IReadOnlyList<SidebarStep> SidebarSteps { get; private set; } = [];

    public IReadOnlyList<IWizardStep> Steps => _engine.Steps;

    public ThemeService Theme => ThemeService.Instance;

    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> NextCommand { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> BackCommand { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> ExitCommand { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> ToggleThemeCommand { get; }
    public ReactiveCommand<int, System.Reactive.Unit> JumpToStepCommand { get; }

    public WizardViewModel(WizardEngine engine, ILibraryRepository libraryRepository,
        ICharacterRepository characterRepository, MainViewModel main, CharacterDraft? initialDraft = null, int? characterId = null)
    {
        _engine     = engine;
        Draft       = initialDraft ?? CharacterDraft.Empty();
        CharacterId = characterId;

        // ── Criar step ViewModels ─────────────────────────────────────────────
        _stepViewModels = new Dictionary<Type, object>
        {
            [typeof(ConceptStep)]       = new ConceptViewModel(this),
            [typeof(AttributesStep)]    = new AttributesViewModel(this),
            [typeof(SecondaryStep)]     = new SecondaryViewModel(this),
            [typeof(AdvantagesStep)]    = new AdvantagesViewModel(this, libraryRepository),
            [typeof(DisadvantagesStep)] = new DisadvantagesViewModel(this, libraryRepository),
            [typeof(SkillsStep)]        = new SkillsViewModel(this, libraryRepository),
            [typeof(TechniquesStep)]    = new TechniquesViewModel(this, libraryRepository),
            [typeof(SpellsStep)]        = new SpellsViewModel(this, libraryRepository),
            [typeof(EquipmentStep)]     = new EquipmentViewModel(this, libraryRepository),
            [typeof(ReviewStep)]        = new ReviewViewModel(this, characterRepository),
        };

        // Estado inicial
        CurrentStep          = engine.CurrentStep;
        CurrentStepViewModel = _stepViewModels[CurrentStep.GetType()];
        Points               = PointCalculator.Calculate(Draft);
        UpdateState();

        // ── Reações reativas ──────────────────────────────────────────────────

        // Draft mudou → recalcular pontos e revalidar step atual
        this.WhenAnyValue(x => x.Draft)
            .Subscribe(d =>
            {
                Points       = PointCalculator.Calculate(d);
                IsOverBudget = Points.Remaining < 0;
                CanGoNext    = CurrentStep.CanProceed(d);
            });

        // Step mudou → atualizar CurrentStepViewModel e revalidar
        this.WhenAnyValue(x => x.CurrentStep)
            .Subscribe(step =>
            {
                if (_stepViewModels.TryGetValue(step.GetType(), out var vm))
                    CurrentStepViewModel = vm;
                CanGoNext = step.CanProceed(Draft);
            });

        // Comandos
        var canNext = this.WhenAnyValue(x => x.CanGoNext);
        var canBack = this.WhenAnyValue(x => x.CanGoBack);

        NextCommand         = ReactiveCommand.CreateFromTask(ExecuteNextAsync, canNext);
        BackCommand         = ReactiveCommand.CreateFromTask(ExecuteBackAsync, canBack);
        ExitCommand         = ReactiveCommand.Create(main.ShowHome);
        ToggleThemeCommand  = ReactiveCommand.Create(ThemeService.Instance.Toggle);
        
        JumpToStepCommand   = ReactiveCommand.Create<int>(index =>
        {
            if (index >= 0 && index < _engine.TotalSteps)
            {
                _engine.JumpTo(index);
                CurrentStep = _engine.CurrentStep;
                UpdateState();
            }
        });
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void UpdateState()
    {
        CanGoBack       = _engine.CanGoBack;
        CanGoNext       = CurrentStep.CanProceed(Draft);
        ProgressPercent = (double)(_engine.CurrentIndex + 1) / _engine.TotalSteps * 100.0;
        StepIndex       = _engine.CurrentIndex;
        SidebarSteps    = [.. _engine.Steps.Select((s, i) =>
            new SidebarStep(s.Title, i, i < StepIndex, i == StepIndex))];
    }

    private async Task ExecuteNextAsync()
    {
        if (await _engine.TryAdvanceAsync(Draft))
        {
            CurrentStep = _engine.CurrentStep;
            UpdateState();
        }
    }

    private async Task ExecuteBackAsync()
    {
        await _engine.GoBackAsync(Draft);
        CurrentStep = _engine.CurrentStep;
        UpdateState();
    }
}
