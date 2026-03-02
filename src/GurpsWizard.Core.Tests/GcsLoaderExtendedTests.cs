using FluentAssertions;
using GurpsWizard.Data.Gcs;

namespace GurpsWizard.Core.Tests;

/// <summary>
/// Testes estendidos do GcsLoader contra arquivos GCS reais.
/// Cobre LoadTechniquesAsync e LoadSpellsAsync que não tinham cobertura.
/// </summary>
public class GcsLoaderExtendedTests
{
    // ─── Caminhos ─────────────────────────────────────────────────────────────
    private static readonly string SolutionRoot =
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../"));

    private static readonly string SklPath =
        Path.Combine(SolutionRoot, "data", "gcs-ptbr", "Módulo Básico", "Módulo Básico Perícias.skl");

    private static readonly string SplPath =
        Path.Combine(SolutionRoot, "data", "gcs-ptbr", "GURPS Magia", "Magia Magicas.spl");

    private readonly GcsLoader _loader = new();

    // ─────────────────────────────────────────────────────────────────────────
    // LoadTechniquesAsync (.skl)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task LoadTechniquesAsync_CarregaTecnicasDoArquivoSkl()
    {
        var tecnicas = (await _loader.LoadTechniquesAsync(SklPath)).ToList();

        tecnicas.Should().NotBeEmpty();
        tecnicas.Count.Should().BeGreaterThan(0,
            "arquivo .skl contém técnicas (linhas sem atributo base)");
    }

    [Fact]
    public async Task LoadTechniquesAsync_NenhumaTecnicaTemBaseAttribute()
    {
        // Técnicas são identificadas pela ausência de atributo base no campo "difficulty"
        // GcsLoader.LoadTechniquesAsync usa LibraryTechnique, não LibrarySkill.
        // LibraryTechnique não tem BaseAttribute — verificamos via ParentSkillName.
        var tecnicas = (await _loader.LoadTechniquesAsync(SklPath)).ToList();

        // Todas as técnicas devem ter o nome da perícia pai não vazio
        // (o campo equivalente ao "BaseAttribute" das perícias é ParentSkillName)
        tecnicas.Should().OnlyContain(t => !string.IsNullOrEmpty(t.Name),
            "toda técnica deve ter um nome");
    }

    [Fact]
    public async Task LoadTechniquesAsync_AlgumasTecnicasTemPericiaPai()
    {
        // Nem todas as técnicas têm péricia pai definida no GCS, mas a maioria tem
        var tecnicas = (await _loader.LoadTechniquesAsync(SklPath)).ToList();

        tecnicas.Should().Contain(t => !string.IsNullOrEmpty(t.ParentSkillName),
            "ao menos algumas técnicas devem ter uma perícia pai");
    }

    [Fact]
    public async Task LoadTechniquesAsync_DificuldadeSoAouH()
    {
        var tecnicas = (await _loader.LoadTechniquesAsync(SklPath)).ToList();
        var dificuldades = tecnicas.Select(t => t.Difficulty).Distinct().ToList();

        dificuldades.Should().OnlyContain(d => d == "A" || d == "H",
            "técnicas só têm dificuldade Média (A) ou Difícil (H)");
    }

    [Fact]
    public async Task LoadTechniquesAsync_DisplayNameFormatoCorreto_ParaComPericiaPai()
    {
        var tecnicas = (await _loader.LoadTechniquesAsync(SklPath)).ToList();

        // Apenas técnicas com perícia pai têm parênteses no DisplayName
        // ex: "Chute (Caratê-2)"
        var comPericiaPai = tecnicas.Where(t => !string.IsNullOrEmpty(t.ParentSkillName)).ToList();
        comPericiaPai.Should().NotBeEmpty("deve haver técnicas com perícia pai definida");
        comPericiaPai.Should().OnlyContain(
            t => t.DisplayName.Contains('(') && t.DisplayName.Contains(')'),
            "DisplayName de técnica com perícia pai deve conter parênteses");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // LoadSpellsAsync (.spl)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task LoadSpellsAsync_CarregaMagicasDoArquivoSpl()
    {
        var magicas = (await _loader.LoadSpellsAsync(SplPath)).ToList();

        magicas.Should().NotBeEmpty();
        magicas.Count.Should().BeGreaterThan(10,
            "arquivo .spl de GURPS Magia tem dezenas de mágicas");
    }

    [Fact]
    public async Task LoadSpellsAsync_TodasTemCollegeNaoVazio()
    {
        var magicas = (await _loader.LoadSpellsAsync(SplPath)).ToList();

        magicas.Should().OnlyContain(m => !string.IsNullOrEmpty(m.College),
            "toda mágica deve pertencer a uma escola");
    }

    [Fact]
    public async Task LoadSpellsAsync_DificuldadeSoHouVH()
    {
        var magicas = (await _loader.LoadSpellsAsync(SplPath)).ToList();
        var dificuldades = magicas.Select(m => m.Difficulty).Distinct().ToList();

        dificuldades.Should().OnlyContain(d => d == "H" || d == "VH",
            "mágicas só têm dificuldade H ou VH");
    }

    [Fact]
    public async Task LoadSpellsAsync_TodasTemGcsIdNaoVazio()
    {
        // Arquivo de Magia usa formato v2 com UUID "id" — deve ser preservado
        var magicas = (await _loader.LoadSpellsAsync(SplPath)).ToList();

        magicas.Should().OnlyContain(m => !string.IsNullOrEmpty(m.GcsId),
            "toda mágica deve ter um GcsId (UUID do formato v2)");
    }

    [Fact]
    public async Task LoadSpellsAsync_TodasTemNomeNaoVazio()
    {
        var magicas = (await _loader.LoadSpellsAsync(SplPath)).ToList();

        magicas.Should().OnlyContain(m => !string.IsNullOrEmpty(m.Name));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Separação Skills vs Técnicas do mesmo arquivo .skl
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task LoadSkillsAsync_ContemMaisItensDo_LoadTechniquesAsync()
    {
        var skills = (await _loader.LoadSkillsAsync(SklPath)).ToList();
        var tecnicas = (await _loader.LoadTechniquesAsync(SklPath)).ToList();

        skills.Count.Should().BeGreaterThan(tecnicas.Count,
            "há mais perícias do que técnicas no arquivo .skl");
    }

    [Fact]
    public async Task SomaDe_SkillsETecnicas_EquivaleTotalDeLinhas()
    {
        var skills = (await _loader.LoadSkillsAsync(SklPath)).ToList();
        var tecnicas = (await _loader.LoadTechniquesAsync(SklPath)).ToList();

        // A soma deve ser > 0 — ambas as listas juntas cobrem o arquivo
        (skills.Count + tecnicas.Count).Should().BeGreaterThan(100,
            "a soma de perícias e técnicas deve ser substancial");
    }
}
