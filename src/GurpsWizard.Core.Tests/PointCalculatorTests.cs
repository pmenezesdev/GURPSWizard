using FluentAssertions;
using GurpsWizard.Core.Models;
using GurpsWizard.Core.Services;

namespace GurpsWizard.Core.Tests;

public class PointCalculatorTests
{
    // -------------------------------------------------------------------------
    // Personagem zerado
    // -------------------------------------------------------------------------

    [Fact]
    public void PersonagemVazio_DeveTerZeroPontosGastos()
    {
        var draft = CharacterDraft.Empty();

        var points = PointCalculator.Calculate(draft);

        points.Spent.Should().Be(0);
    }

    [Fact]
    public void PersonagemVazio_PontosRestantesIgualAoTotal()
    {
        var draft = CharacterDraft.Empty();

        var points = PointCalculator.Calculate(draft);

        points.Remaining.Should().Be(draft.TotalPoints);
    }

    // -------------------------------------------------------------------------
    // Custo de atributos primários
    // -------------------------------------------------------------------------

    [Fact]
    public void ST12_DeveCustar20Pontos()
    {
        var draft = CharacterDraft.Empty() with
        {
            Attributes = Attributes.Default with { ST = 12 }
        };

        var points = PointCalculator.Calculate(draft);

        points.Spent.Should().Be(20);
    }

    [Fact]
    public void DX11_DeveCustar20Pontos()
    {
        var draft = CharacterDraft.Empty() with
        {
            Attributes = Attributes.Default with { DX = 11 }
        };

        var points = PointCalculator.Calculate(draft);

        points.Spent.Should().Be(20);
    }

    [Fact]
    public void IQ11_DeveCustar20Pontos()
    {
        var draft = CharacterDraft.Empty() with
        {
            Attributes = Attributes.Default with { IQ = 11 }
        };

        var points = PointCalculator.Calculate(draft);

        points.Spent.Should().Be(20);
    }

    [Fact]
    public void HT12_DeveCustar20Pontos()
    {
        var draft = CharacterDraft.Empty() with
        {
            Attributes = Attributes.Default with { HT = 12 }
        };

        var points = PointCalculator.Calculate(draft);

        points.Spent.Should().Be(20);
    }

    [Fact]
    public void ST8_DeveGerarMenos20Pontos()
    {
        var draft = CharacterDraft.Empty() with
        {
            Attributes = Attributes.Default with { ST = 8 }
        };

        var points = PointCalculator.Calculate(draft);

        points.Spent.Should().Be(-20);
    }

    // -------------------------------------------------------------------------
    // Custo de perícias por nível relativo
    // -------------------------------------------------------------------------

    [Theory]
    // Easy
    [InlineData("E", 0,  1)]
    [InlineData("E", 1,  2)]
    [InlineData("E", 2,  4)]
    [InlineData("E", 3,  8)]
    // Average
    [InlineData("A", -1, 1)]
    [InlineData("A", 0,  2)]
    [InlineData("A", 1,  4)]
    [InlineData("A", 2,  8)]
    // Hard
    [InlineData("H", -2, 1)]
    [InlineData("H", -1, 2)]
    [InlineData("H", 0,  4)]
    [InlineData("H", 1,  8)]
    // Very Hard
    [InlineData("VH", -3, 1)]
    [InlineData("VH", -2, 2)]
    [InlineData("VH", -1, 4)]
    [InlineData("VH", 0,  8)]
    public void CustoPericiaPorDificuldade_DeveRetornarValorCorreto(string diff, int nivel, int custoEsperado)
    {
        PointCalculator.SkillCostFromDifficulty(diff, nivel)
            .Should().Be(custoEsperado);
    }

    [Fact]
    public void Pericia_NivelRelativoMais2_DeveCustar4Pontos()
    {
        var skill = new SkillEntry("id1", "Atletismo", "DX", "E", Level: 2, Cost: 4);
        var draft = CharacterDraft.Empty() with
        {
            Skills = [skill]
        };

        var points = PointCalculator.Calculate(draft);

        points.Spent.Should().Be(4);
    }

    // -------------------------------------------------------------------------
    // Custo de vantagens e desvantagens
    // -------------------------------------------------------------------------

    [Fact]
    public void Vantagem_Custa15Pontos_DeveSomarCorretamente()
    {
        var advantage = new TraitEntry("id1", "Reflexos de Combate", 15);
        var draft = CharacterDraft.Empty() with
        {
            Advantages = [advantage]
        };

        var points = PointCalculator.Calculate(draft);

        points.Spent.Should().Be(15);
    }

    [Fact]
    public void Desvantagem_CustaNegativo_DeveSubtrairCorretamente()
    {
        var disadv = new TraitEntry("id1", "Fobia (Alturas)", -10);
        var draft = CharacterDraft.Empty() with
        {
            Disadvantages = [disadv]
        };

        var points = PointCalculator.Calculate(draft);

        points.Spent.Should().Be(-10);
    }

    // -------------------------------------------------------------------------
    // Custo de atributos secundários
    // -------------------------------------------------------------------------

    [Fact]
    public void HPBonus2_DeveCustar4Pontos()
    {
        var draft = CharacterDraft.Empty() with
        {
            SecondaryAttributes = SecondaryAttributes.Default with { HPBonus = 2 }
        };

        var points = PointCalculator.Calculate(draft);

        points.Spent.Should().Be(4);
    }

    [Fact]
    public void WillBonus1_DeveCustar5Pontos()
    {
        var draft = CharacterDraft.Empty() with
        {
            SecondaryAttributes = SecondaryAttributes.Default with { WillBonus = 1 }
        };

        var points = PointCalculator.Calculate(draft);

        points.Spent.Should().Be(5);
    }

    [Fact]
    public void PerBonus1_DeveCustar5Pontos()
    {
        var draft = CharacterDraft.Empty() with
        {
            SecondaryAttributes = SecondaryAttributes.Default with { PerBonus = 1 }
        };

        var points = PointCalculator.Calculate(draft);

        points.Spent.Should().Be(5);
    }

    [Fact]
    public void FPBonus1_DeveCustar3Pontos()
    {
        var draft = CharacterDraft.Empty() with
        {
            SecondaryAttributes = SecondaryAttributes.Default with { FPBonus = 1 }
        };

        var points = PointCalculator.Calculate(draft);

        points.Spent.Should().Be(3);
    }

    [Fact]
    public void BasicSpeedBonus1_DeveCustar5Pontos()
    {
        var draft = CharacterDraft.Empty() with
        {
            SecondaryAttributes = SecondaryAttributes.Default with { BasicSpeedBonus = 1 }
        };

        var points = PointCalculator.Calculate(draft);

        points.Spent.Should().Be(5);
    }

    // -------------------------------------------------------------------------
    // Cálculo combinado
    // -------------------------------------------------------------------------

    [Fact]
    public void PersonagemComplexo_DeveSomarTodosOsCustos()
    {
        // ST=12 (+20), DX=11 (+20), Vantagem 15 pts, Desvantagem -10, Perícia nível+2 (4 pts)
        // Total esperado: 20 + 20 + 15 - 10 + 4 = 49
        var draft = new CharacterDraft(
            Name: "Teste",
            Description: "",
            TotalPoints: 150,
            Attributes: new Attributes(ST: 12, DX: 11, IQ: 10, HT: 10),
            SecondaryAttributes: SecondaryAttributes.Default,
            Advantages: [new TraitEntry("v1", "Reflexos de Combate", 15)],
            Disadvantages: [new TraitEntry("d1", "Acrofobia", -10)],
            Skills: [new SkillEntry("s1", "Espadas", "DX", "E", Level: 2, Cost: 4)],
            Equipment: []
        );

        var points = PointCalculator.Calculate(draft);

        points.Spent.Should().Be(49);
        points.Remaining.Should().Be(150 - 49);
    }
}
