namespace GurpsWizard.Data.Entities;

/// <summary>Perícia da biblioteca GCS, armazenada no SQLite.</summary>
public class LibrarySkill
{
    public int Id { get; set; }

    /// <summary>ID original do arquivo GCS.</summary>
    public string GcsId { get; set; } = "";

    public string Name { get; set; } = "";

    /// <summary>Atributo base em maiúsculas (ex: "IQ", "DX", "ST", "HT").</summary>
    public string BaseAttribute { get; set; } = "";

    /// <summary>Dificuldade: "E" (fácil), "A" (média), "H" (difícil), "VH" (muito difícil).</summary>
    public string Difficulty { get; set; } = "";

    /// <summary>Especialização, quando presente (pode conter "@Especialização@" como placeholder).</summary>
    public string? Specialization { get; set; }

    /// <summary>
    /// Nome de exibição: inclui a especialização quando ela é um valor real
    /// (ignora placeholders como "@Especialização@").
    /// </summary>
    public string DisplayName =>
        string.IsNullOrEmpty(Specialization) || Specialization.Contains('@')
            ? Name
            : $"{Name} ({Specialization})";

    /// <summary>Categorias separadas por vírgula.</summary>
    public string Tags { get; set; } = "";

    public string? Notes { get; set; }

    public string? Reference { get; set; }

    /// <summary>JSON serializado de um Prerequisite tree (ou null se sem pré-requisitos).</summary>
    public string? PrerequisitesJson { get; set; }
}
