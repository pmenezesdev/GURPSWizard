namespace GurpsWizard.Data.Entities;

/// <summary>Vantagem ou desvantagem da biblioteca GCS, armazenada no SQLite.</summary>
public class LibraryTrait
{
    public int Id { get; set; }

    /// <summary>ID original do arquivo GCS (campo "id" do JSON).</summary>
    public string GcsId { get; set; } = "";

    public string Name { get; set; } = "";

    /// <summary>Custo base em pontos. Negativo = desvantagem.</summary>
    public int BasePoints { get; set; }

    /// <summary>Verdadeiro quando a trait tem múltiplos níveis (can_level = true).</summary>
    public bool CanLevel { get; set; }

    /// <summary>Custo por nível adicional (quando CanLevel = true).</summary>
    public int PointsPerLevel { get; set; }

    /// <summary>Categorias separadas por vírgula (campo "tags" do JSON).</summary>
    public string Tags { get; set; } = "";

    public string? Notes { get; set; }

    /// <summary>Referência de página (ex: "MB97").</summary>
    public string? Reference { get; set; }

    /// <summary>JSON serializado de um Prerequisite tree (ou null se sem pré-requisitos).</summary>
    public string? PrerequisitesJson { get; set; }

    /// <summary>True se BasePoints &lt; 0 (desvantagem).</summary>
    public bool IsDisadvantage => BasePoints < 0;
}
