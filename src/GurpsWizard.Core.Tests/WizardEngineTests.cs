using FluentAssertions;
using GurpsWizard.App.Wizard;
using GurpsWizard.App.Wizard.Steps;
using GurpsWizard.Core.Models;

namespace GurpsWizard.Core.Tests;

public class WizardEngineTests
{
    // ─── Stub mínimo de IWizardStep ─────────────────────────────────────────

    private sealed class FakeStep(bool canProceed = true) : IWizardStep
    {
        public string Title => "Fake";
        public string Description => "Fake step";
        public string? BackgroundImage => null;
        public bool CanProceed(CharacterDraft draft) => canProceed;
        public Task OnEnterAsync(CharacterDraft draft) => Task.CompletedTask;
        public Task OnLeaveAsync(CharacterDraft draft) => Task.CompletedTask;
    }

    private static CharacterDraft EmptyDraft => CharacterDraft.Empty();

    // ─────────────────────────────────────────────────────────────────────────
    // Construtor
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_EmptyStepList_ThrowsArgumentException()
    {
        var act = () => new WizardEngine(new List<IWizardStep>());
        act.Should().Throw<ArgumentException>();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Estado inicial
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void InitialState_IndexIsZero()
    {
        var engine = new WizardEngine([new FakeStep(), new FakeStep()]);
        engine.CurrentIndex.Should().Be(0);
    }

    [Fact]
    public void InitialState_CanGoBackIsFalse()
    {
        var engine = new WizardEngine([new FakeStep(), new FakeStep()]);
        engine.CanGoBack.Should().BeFalse();
    }

    [Fact]
    public void InitialState_IsLastStepIsFalse_WhenMultipleSteps()
    {
        var engine = new WizardEngine([new FakeStep(), new FakeStep()]);
        engine.IsLastStep.Should().BeFalse();
    }

    [Fact]
    public void TotalSteps_ReturnsCorrectCount()
    {
        var engine = new WizardEngine([new FakeStep(), new FakeStep(), new FakeStep()]);
        engine.TotalSteps.Should().Be(3);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TryAdvanceAsync
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task TryAdvanceAsync_CanProceedTrue_AdvancesAndReturnsTrue()
    {
        var engine = new WizardEngine([new FakeStep(canProceed: true), new FakeStep()]);

        var result = await engine.TryAdvanceAsync(EmptyDraft);

        result.Should().BeTrue();
        engine.CurrentIndex.Should().Be(1);
    }

    [Fact]
    public async Task TryAdvanceAsync_CanProceedFalse_DoesNotAdvanceAndReturnsFalse()
    {
        var engine = new WizardEngine([new FakeStep(canProceed: false), new FakeStep()]);

        var result = await engine.TryAdvanceAsync(EmptyDraft);

        result.Should().BeFalse();
        engine.CurrentIndex.Should().Be(0);
    }

    [Fact]
    public async Task TryAdvanceAsync_NaUltimaEtapa_IndexNaoMuda()
    {
        // Na última etapa, o índice não avança (independente do valor de retorno)
        var engine = new WizardEngine([new FakeStep(), new FakeStep()]);
        await engine.TryAdvanceAsync(EmptyDraft); // vai para índice 1
        engine.CurrentIndex.Should().Be(1);

        await engine.TryAdvanceAsync(EmptyDraft); // está na última, não avança
        engine.CurrentIndex.Should().Be(1);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GoBackAsync
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GoBackAsync_DoSegundoStep_VoltaParaPrimeiro()
    {
        var engine = new WizardEngine([new FakeStep(), new FakeStep()]);
        await engine.TryAdvanceAsync(EmptyDraft);

        await engine.GoBackAsync(EmptyDraft);

        engine.CurrentIndex.Should().Be(0);
    }

    [Fact]
    public async Task GoBackAsync_DoPrimeiroStep_PermanecePrimeiro()
    {
        var engine = new WizardEngine([new FakeStep(), new FakeStep()]);

        await engine.GoBackAsync(EmptyDraft);

        engine.CurrentIndex.Should().Be(0);
    }

    [Fact]
    public async Task GoBackAsync_HabilitaCanGoBack_AposAvançar()
    {
        var engine = new WizardEngine([new FakeStep(), new FakeStep()]);
        await engine.TryAdvanceAsync(EmptyDraft);
        engine.CanGoBack.Should().BeTrue();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // JumpTo
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void JumpTo_ValidIndex_SetsCurrentIndexSemVerificarCanProceed()
    {
        // Pula para step 2 sem chamar CanProceed do step 0 ou 1
        var blocker = new FakeStep(canProceed: false);
        var engine = new WizardEngine([blocker, new FakeStep(), new FakeStep()]);

        engine.JumpTo(2);

        engine.CurrentIndex.Should().Be(2);
    }

    [Fact]
    public void JumpTo_NegativeIndex_DoesNothing()
    {
        var engine = new WizardEngine([new FakeStep(), new FakeStep()]);

        engine.JumpTo(-1);

        engine.CurrentIndex.Should().Be(0);
    }

    [Fact]
    public void JumpTo_OutOfRangeIndex_DoesNothing()
    {
        var engine = new WizardEngine([new FakeStep(), new FakeStep()]);

        engine.JumpTo(99);

        engine.CurrentIndex.Should().Be(0);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // IsLastStep
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void IsLastStep_TrueApenasNaUltimaEtapa()
    {
        var engine = new WizardEngine([new FakeStep(), new FakeStep(), new FakeStep()]);

        engine.JumpTo(0); engine.IsLastStep.Should().BeFalse();
        engine.JumpTo(1); engine.IsLastStep.Should().BeFalse();
        engine.JumpTo(2); engine.IsLastStep.Should().BeTrue();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CanProceed — ConceptStep
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void ConceptStep_NomeVazio_CanProceedFalse()
    {
        var step = new ConceptStep();
        var draft = CharacterDraft.Empty(); // Name = ""
        step.CanProceed(draft).Should().BeFalse();
    }

    [Fact]
    public void ConceptStep_NomeValido_CanProceedTrue()
    {
        var step = new ConceptStep();
        var draft = CharacterDraft.Empty() with { Name = "Thorin" };
        step.CanProceed(draft).Should().BeTrue();
    }

    [Fact]
    public void ConceptStep_NomeSoEspacos_CanProceedFalse()
    {
        var step = new ConceptStep();
        var draft = CharacterDraft.Empty() with { Name = "   " };
        step.CanProceed(draft).Should().BeFalse();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CanProceed — SkillsStep
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void SkillsStep_SemPericias_CanProceedFalse()
    {
        var step = new SkillsStep();
        step.CanProceed(CharacterDraft.Empty()).Should().BeFalse();
    }

    [Fact]
    public void SkillsStep_ComUmaPericia_CanProceedTrue()
    {
        var step = new SkillsStep();
        var draft = CharacterDraft.Empty() with
        {
            Skills = [new SkillEntry("s1", "Espadas", "DX", "A", Level: 0, Cost: 2)]
        };
        step.CanProceed(draft).Should().BeTrue();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CanProceed — DisadvantagesStep
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void DisadvantagesStep_ZeroPontos_CanProceedTrue()
    {
        var step = new DisadvantagesStep();
        step.CanProceed(CharacterDraft.Empty()).Should().BeTrue();
    }

    [Fact]
    public void DisadvantagesStep_75Pontos_CanProceedTrue()
    {
        var step = new DisadvantagesStep();
        var draft = CharacterDraft.Empty() with
        {
            Disadvantages = [new TraitEntry("d1", "Pacifismo", -75)]
        };
        step.CanProceed(draft).Should().BeTrue();
    }

    [Fact]
    public void DisadvantagesStep_76Pontos_CanProceedFalse()
    {
        var step = new DisadvantagesStep();
        var draft = CharacterDraft.Empty() with
        {
            Disadvantages = [new TraitEntry("d1", "Pacifismo", -76)]
        };
        step.CanProceed(draft).Should().BeFalse();
    }

    [Fact]
    public void DisadvantagesStep_SomaDe76Pontos_CanProceedFalse()
    {
        var step = new DisadvantagesStep();
        var draft = CharacterDraft.Empty() with
        {
            Disadvantages = [
                new TraitEntry("d1", "Desvantagem A", -40),
                new TraitEntry("d2", "Desvantagem B", -36),
            ]
        };
        step.CanProceed(draft).Should().BeFalse();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CanProceed — Steps que sempre retornam true
    // ─────────────────────────────────────────────────────────────────────────

    [Theory]
    [MemberData(nameof(AlwaysTrueSteps))]
    public void AlwaysTrueStep_CanProceedAlwaysTrue(IWizardStep step)
    {
        step.CanProceed(CharacterDraft.Empty()).Should().BeTrue();
    }

    public static TheoryData<IWizardStep> AlwaysTrueSteps() => new()
    {
        new AttributesStep(),
        new SecondaryStep(),
        new AdvantagesStep(),
        new TechniquesStep(),
        new SpellsStep(),
        new EquipmentStep(),
        new ReviewStep(),
    };
}
