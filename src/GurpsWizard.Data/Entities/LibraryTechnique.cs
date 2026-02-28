namespace GurpsWizard.Data.Entities;

/// <summary>Técnica da biblioteca GCS, armazenada no SQLite.</summary>
public class LibraryTechnique
{
    public int Id { get; set; }

    /// <summary>ID original do arquivo GCS.</summary>
    public string GcsId { get; set; } = "";

    public string Name { get; set; } = "";

    /// <summary>Dificuldade: "A" (Média) ou "H" (Difícil).</summary>
    public string Difficulty { get; set; } = "";

    /// <summary>Nome da perícia pai (ex: "Briga", "Caratê").</summary>
    public string ParentSkillName { get; set; } = "";

    /// <summary>Modificador do valor predefinido em relação à perícia pai (ex: -2 → Briga-2).</summary>
    public int DefaultModifier { get; set; }

    /// <summary>Nível máximo acima do predefinido (null = sem limite explícito).</summary>
    public int? MaxAboveDefault { get; set; }

    /// <summary>Categorias separadas por vírgula.</summary>
    public string Tags { get; set; } = "";

    public string? Notes { get; set; }

    public string? Reference { get; set; }

    /// <summary>Nome de exibição: "Chute (Caratê-2)".</summary>
    public string DisplayName =>
        string.IsNullOrEmpty(ParentSkillName)
            ? Name
            : $"{Name} ({ParentSkillName}{DefaultModifier})";
}
