using GurpsWizard.Core.Models;

namespace GurpsWizard.Core.Services;

/// <summary>
/// Calcula o custo em pontos de personagem para um <see cref="CharacterDraft"/>,
/// seguindo as regras simplificadas do GURPS 4ª Edição descritas no PRD.
/// </summary>
public static class PointCalculator
{
    // -------------------------------------------------------------------------
    // Custo de atributos primários (MB p.14)
    // ST e HT: 10 pts por ponto (acima ou abaixo de 10)
    // DX e IQ: 20 pts por ponto
    // -------------------------------------------------------------------------
    private const int CostPerST = 10;
    private const int CostPerHT = 10;
    private const int CostPerDX = 20;
    private const int CostPerIQ = 20;

    // -------------------------------------------------------------------------
    // Custo de atributos secundários (MB p.16)
    // PV: 2 pts; PF: 3 pts; Vontade/Per: 5 pts por nível acima do base
    // Vel (incremento de 0.25): 5 pts; Deslocamento: 5 pts por nível
    // -------------------------------------------------------------------------
    private const int CostPerHP   = 2;
    private const int CostPerFP   = 3;
    private const int CostPerWill = 5;
    private const int CostPerPer  = 5;
    private const int CostPerBasicSpeed = 5;
    private const int CostPerBasicMove  = 5;

    /// <summary>
    /// Calcula o custo total do rascunho do personagem e retorna
    /// um <see cref="CharacterPoints"/> com Spent e Remaining.
    /// </summary>
    public static CharacterPoints Calculate(CharacterDraft draft)
    {
        int spentAdvantages    = draft.Advantages.Sum(t => t.Cost);
        int spentDisadvantages = draft.Disadvantages.Sum(t => t.Cost);

        int spent = AttributeCost(draft.Attributes)
                  + SecondaryCost(draft.SecondaryAttributes)
                  + spentAdvantages
                  + spentDisadvantages
                  + draft.Skills.Sum(s => s.Cost)
                  + (draft.Techniques?.Sum(t => t.Cost) ?? 0)
                  + (draft.Spells?.Sum(s => s.Cost) ?? 0);

        return new CharacterPoints(draft.TotalPoints, spent, draft.TotalPoints - spent, spentAdvantages, spentDisadvantages);
    }

    // -------------------------------------------------------------------------
    // Métodos auxiliares públicos (usados pelo App ao calcular custo em tempo real)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Retorna o custo oficial para o nível relativo de uma perícia, conforme sua dificuldade (MB p. 170).
    /// </summary>
    /// <param name="difficulty">"E", "A", "H" ou "VH"</param>
    /// <param name="relativeLevel">Diferença em relação ao atributo base</param>
    public static int SkillCostFromDifficulty(string difficulty, int relativeLevel)
    {
        // ── Normalização para a tabela Easy ─────────────────────────────────
        // A dificuldade apenas "desloca" o nível relativo na tabela:
        // Easy(0) = 1 pt. Average(0) = Easy(+1). Hard(0) = Easy(+2). VH(0) = Easy(+3).
        int shift = difficulty.ToUpperInvariant() switch
        {
            "E"  => 0,
            "A"  => 1,
            "H"  => 2,
            "VH" => 3,
            _    => 0 // Default Easy
        };

        int adjustedLevel = relativeLevel + shift;

        // Tabela base (Easy):
        // <= -1: 0 pts
        //  0: 1 pt
        //  1: 2 pts
        //  2: 4 pts
        //  3: 8 pts
        // >= 4: 12 + (n-4)*4 pts
        return adjustedLevel switch
        {
            < 0   => 0,
            0     => 1,
            1     => 2,
            2     => 4,
            3     => 8,
            >= 4  => 12 + (adjustedLevel - 4) * 4
        };
    }

    /// <summary>
    /// Custo de uma técnica em pontos, conforme a Tabela de Custo das Técnicas (MB p.230).
    /// </summary>
    /// <param name="difficulty">"A" (Média) ou "H" (Difícil)</param>
    /// <param name="levelsAboveDefault">Níveis acima do valor predefinido (0 = no predefinido, custo zero)</param>
    public static int TechniqueCost(string difficulty, int levelsAboveDefault)
    {
        if (levelsAboveDefault <= 0) return 0;

        // Difícil: predefinido+1 = 2 pts, predefinido+2 = 3 pts, ... = n+1
        // Média:   predefinido+1 = 1 pt,  predefinido+2 = 2 pts, ... = n
        return difficulty.ToUpperInvariant() == "H"
            ? levelsAboveDefault + 1
            : levelsAboveDefault;
    }

    // -------------------------------------------------------------------------
    // Helpers internos
    // -------------------------------------------------------------------------

    private static int AttributeCost(Attributes a) =>
        (a.ST - 10) * CostPerST +
        (a.DX - 10) * CostPerDX +
        (a.IQ - 10) * CostPerIQ +
        (a.HT - 10) * CostPerHT;

    private static int SecondaryCost(SecondaryAttributes s) =>
        s.HPBonus          * CostPerHP    +
        s.FPBonus          * CostPerFP    +
        s.WillBonus        * CostPerWill  +
        s.PerBonus         * CostPerPer   +
        s.BasicSpeedBonus  * CostPerBasicSpeed +
        s.BasicMoveBonus   * CostPerBasicMove;
}
