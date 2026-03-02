using FluentAssertions;
using GurpsWizard.Core.Models;
using GurpsWizard.Core.Services;

namespace GurpsWizard.Core.Tests;

public class PointCalculatorExtendedTests
{
    // ─────────────────────────────────────────────────────────────────────────
    // TechniqueCost
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void TechniqueCost_ZeroLevels_ReturnsZero()
    {
        PointCalculator.TechniqueCost("A", 0).Should().Be(0);
    }

    [Fact]
    public void TechniqueCost_NegativeLevels_ReturnsZero()
    {
        PointCalculator.TechniqueCost("H", -1).Should().Be(0);
    }

    [Fact]
    public void TechniqueCost_OneLevelH_Returns2()
    {
        // Difícil: predefinido+1 = 1+1 = 2 pts
        PointCalculator.TechniqueCost("H", 1).Should().Be(2);
    }

    [Fact]
    public void TechniqueCost_TwoLevelsH_Returns3()
    {
        // Difícil: predefinido+2 = 2+1 = 3 pts
        PointCalculator.TechniqueCost("H", 2).Should().Be(3);
    }

    [Fact]
    public void TechniqueCost_ThreeLevelsH_Returns4()
    {
        // Difícil: predefinido+3 = 3+1 = 4 pts
        PointCalculator.TechniqueCost("H", 3).Should().Be(4);
    }

    [Fact]
    public void TechniqueCost_OneLevelA_Returns1()
    {
        // Média: predefinido+1 = 1 pt
        PointCalculator.TechniqueCost("A", 1).Should().Be(1);
    }

    [Fact]
    public void TechniqueCost_TwoLevelsA_Returns2()
    {
        // Média: predefinido+2 = 2 pts
        PointCalculator.TechniqueCost("A", 2).Should().Be(2);
    }

    [Fact]
    public void TechniqueCost_CaseInsensitive_Works()
    {
        PointCalculator.TechniqueCost("h", 1).Should().Be(2);
        PointCalculator.TechniqueCost("a", 1).Should().Be(1);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Calculate com técnicas e mágicas
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Calculate_WithSingleTechnique_SumsCostCorrectly()
    {
        var technique = new TechniqueEntry("t1", "Chute", "H", LevelsAboveDefault: 1, Cost: 2);
        var draft = CharacterDraft.Empty() with { Techniques = [technique] };

        PointCalculator.Calculate(draft).Spent.Should().Be(2);
    }

    [Fact]
    public void Calculate_WithMultipleTechniques_SumsCostCorrectly()
    {
        var t1 = new TechniqueEntry("t1", "Chute", "H", LevelsAboveDefault: 2, Cost: 3);
        var t2 = new TechniqueEntry("t2", "Banda", "A", LevelsAboveDefault: 1, Cost: 1);
        var draft = CharacterDraft.Empty() with { Techniques = [t1, t2] };

        PointCalculator.Calculate(draft).Spent.Should().Be(4);
    }

    [Fact]
    public void Calculate_WithSingleSpell_SumsCostCorrectly()
    {
        var spell = new SpellEntry("s1", "Bola de Fogo", "Fogo", "H", Level: 0, Cost: 4);
        var draft = CharacterDraft.Empty() with { Spells = [spell] };

        PointCalculator.Calculate(draft).Spent.Should().Be(4);
    }

    [Fact]
    public void Calculate_WithMultipleSpells_SumsCostCorrectly()
    {
        var s1 = new SpellEntry("s1", "Bola de Fogo", "Fogo", "H", Level: 0, Cost: 4);
        var s2 = new SpellEntry("s2", "Curar", "Corpo", "H", Level: 1, Cost: 8);
        var draft = CharacterDraft.Empty() with { Spells = [s1, s2] };

        PointCalculator.Calculate(draft).Spent.Should().Be(12);
    }

    [Fact]
    public void Calculate_EmptyTechniquesAndSpells_DoesNotAffectTotal()
    {
        var draft = CharacterDraft.Empty();

        PointCalculator.Calculate(draft).Spent.Should().Be(0);
    }

    [Fact]
    public void Calculate_TechniquesAndSpellsCombined_SumsAllCosts()
    {
        var technique = new TechniqueEntry("t1", "Chute", "H", LevelsAboveDefault: 1, Cost: 2);
        var spell = new SpellEntry("s1", "Curar", "Corpo", "H", Level: 0, Cost: 4);
        var skill = new SkillEntry("sk1", "Briga", "DX", "E", Level: 0, Cost: 1);

        var draft = CharacterDraft.Empty() with
        {
            Techniques = [technique],
            Spells = [spell],
            Skills = [skill],
        };

        PointCalculator.Calculate(draft).Spent.Should().Be(7);
    }
}
