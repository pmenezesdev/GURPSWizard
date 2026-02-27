namespace GurpsWizard.Data.Entities;

/// <summary>
/// Personagem salvo. O rascunho completo é serializado em <see cref="DraftJson"/>
/// para flexibilidade no MVP.
/// </summary>
public class CharacterEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int TotalPoints { get; set; }

    /// <summary>Serialização JSON do CharacterDraft completo.</summary>
    public string DraftJson { get; set; } = "{}";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
