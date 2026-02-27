namespace GurpsWizard.Core.Models;

/// <summary>Atributos primários do personagem.</summary>
public record Attributes(int ST, int DX, int IQ, int HT)
{
    public static Attributes Default => new(10, 10, 10, 10);
}
