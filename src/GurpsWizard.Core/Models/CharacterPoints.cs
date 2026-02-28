namespace GurpsWizard.Core.Models;

/// <summary>Resumo de pontos do personagem em criação.</summary>
public record CharacterPoints(int Total, int Spent, int Remaining, int SpentAdvantages = 0, int SpentDisadvantages = 0)
{
    public static CharacterPoints Zero(int total) => new(total, 0, total);
}
