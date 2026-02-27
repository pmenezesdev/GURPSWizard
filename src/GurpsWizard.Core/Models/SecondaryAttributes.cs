namespace GurpsWizard.Core.Models;

/// <summary>
/// Ajustes manuais dos atributos secundários acima do valor base calculado.
/// Valores base: PV=ST, PM=HT, Vontade=IQ, Per=IQ,
///               Vel=(DX+HT)/4 (inteiro), Desl=Vel.
/// Cada ponto de bônus tem um custo conforme as regras do GURPS 4e simplificadas.
/// </summary>
public record SecondaryAttributes(
    int HPBonus = 0,
    int FPBonus = 0,
    int WillBonus = 0,
    int PerBonus = 0,
    int BasicSpeedBonus = 0,
    int BasicMoveBonus = 0
)
{
    public static SecondaryAttributes Default => new();
}
