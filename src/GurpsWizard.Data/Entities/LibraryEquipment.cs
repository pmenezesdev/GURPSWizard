namespace GurpsWizard.Data.Entities;

/// <summary>Equipamento da biblioteca GCS, armazenado no SQLite.</summary>
public class LibraryEquipment
{
    public int Id { get; set; }

    /// <summary>ID original do arquivo GCS.</summary>
    public string GcsId { get; set; } = "";

    /// <summary>Nome do item (campo "description" no JSON).</summary>
    public string Name { get; set; } = "";

    /// <summary>Valor em moedas padrão ($).</summary>
    public decimal Value { get; set; }

    /// <summary>Peso com unidade (ex: "1.5 lb"). Vazio se não informado.</summary>
    public string Weight { get; set; } = "";

    /// <summary>Categorias separadas por vírgula.</summary>
    public string Tags { get; set; } = "";

    public string? Notes { get; set; }

    public string? Reference { get; set; }

    public string? TechLevel { get; set; }
}
