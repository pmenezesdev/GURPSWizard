namespace GurpsWizard.Data.Entities;

/// <summary>Mágica da biblioteca GCS (GURPS Magia), armazenada no SQLite.</summary>
public class LibrarySpell
{
    public int Id { get; set; }

    public string GcsId { get; set; } = "";

    public string Name { get; set; } = "";

    /// <summary>Escola(s) da mágica, ex: "Ar", "Ar ou Clima". Vem do primeiro elemento do array college.</summary>
    public string College { get; set; } = "";

    /// <summary>Fonte de poder, ex: "Arcana".</summary>
    public string? PowerSource { get; set; }

    /// <summary>Classe: "Comum", "Área", "Toque", "Projétil", etc.</summary>
    public string? SpellClass { get; set; }

    /// <summary>Dificuldade: "H" ou "VH".</summary>
    public string Difficulty { get; set; } = "H";

    public string? Resist { get; set; }

    public string? CastingCost { get; set; }

    public string? MaintenanceCost { get; set; }

    public string? CastingTime { get; set; }

    public string? Duration { get; set; }

    public string? Notes { get; set; }

    public string? Reference { get; set; }

    /// <summary>Categorias separadas por vírgula.</summary>
    public string Tags { get; set; } = "";
}
