using FluentAssertions;
using GurpsWizard.Data.Gcs;

namespace GurpsWizard.Core.Tests;

/// <summary>
/// Testes de integração do GcsLoader contra os arquivos reais do GCS-PTBR.
///
/// Nota: "Sentido de Combate" não existe no dataset PTBR.
/// O equivalente é "Reflexos em Combate" (15 pts), que é usado nos testes abaixo.
/// </summary>
public class GcsLoaderTests
{
    // ─── Caminho para os dados GCS ──────────────────────────────────────────
    // AppContext.BaseDirectory = .../bin/Debug/net9.0/ (5 níveis acima = raiz da solução)
    private static readonly string SolutionRoot =
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../"));

    private static readonly string GcsBasePath =
        Path.Combine(SolutionRoot, "data", "gcs-ptbr", "Módulo Básico");

    private static string AdqPath =>
        Path.Combine(GcsBasePath, "Módulo Básico Vantagens e Desvantagens.adq");

    private static string SklPath =>
        Path.Combine(GcsBasePath, "Módulo Básico Perícias.skl");

    private static string EqpPath =>
        Path.Combine(GcsBasePath, "Módulo Básico Equipamentos.eqp");

    private readonly GcsLoader _loader = new();

    // ─────────────────────────────────────────────────────────────────────────
    // Testes de Traits (.adq)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task LoadTraits_DeveCarregarArquivoAdq()
    {
        var traits = (await _loader.LoadTraitsAsync(AdqPath)).ToList();

        traits.Should().NotBeEmpty();
        traits.Count.Should().BeGreaterThan(100);
    }

    [Fact]
    public async Task LoadTraits_ReflexosDeCombate_DeveExistirCom15Pontos()
    {
        // "Sentido de Combate" não existe no GCS-PTBR.
        // O equivalente correto na tradução é "Reflexos em Combate" (15 pts).
        var traits = await _loader.LoadTraitsAsync(AdqPath);

        var trait = traits.FirstOrDefault(t => t.Name == "Reflexos em Combate");
        trait.Should().NotBeNull("'Reflexos em Combate' deve existir no arquivo .adq");
        trait!.BasePoints.Should().Be(15);
    }

    [Fact]
    public async Task LoadTraits_DeveTerVantagensEDesvantagens()
    {
        var traits = (await _loader.LoadTraitsAsync(AdqPath)).ToList();

        traits.Should().Contain(t => t.BasePoints > 0, "deve ter ao menos uma vantagem");
        traits.Should().Contain(t => t.BasePoints < 0, "deve ter ao menos uma desvantagem");
    }

    [Fact]
    public async Task LoadTraits_NenhumaEntradaComIdVazio()
    {
        var traits = await _loader.LoadTraitsAsync(AdqPath);

        traits.Should().OnlyContain(t => !string.IsNullOrWhiteSpace(t.GcsId));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Testes de Perícias (.skl)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task LoadSkills_DeveCarregarArquivoSkl()
    {
        var skills = (await _loader.LoadSkillsAsync(SklPath)).ToList();

        skills.Should().NotBeEmpty();
        skills.Count.Should().BeGreaterThan(100);
    }

    [Fact]
    public async Task LoadSkills_DeveConterPelaoMenosUmaPericiaDX()
    {
        var skills = await _loader.LoadSkillsAsync(SklPath);

        skills.Should().Contain(s => s.BaseAttribute == "DX",
            "deve haver ao menos uma perícia baseada em DX");
    }

    [Fact]
    public async Task LoadSkills_DeveConterPericiasDeDiversosAtributos()
    {
        var skills = (await _loader.LoadSkillsAsync(SklPath)).ToList();

        var attrs = skills.Select(s => s.BaseAttribute).Distinct().ToList();
        attrs.Should().Contain("IQ");
        attrs.Should().Contain("DX");
    }

    [Fact]
    public async Task LoadSkills_DificuldadeDeveSerParsedaCorretamente()
    {
        var skills = (await _loader.LoadSkillsAsync(SklPath)).ToList();

        var diffs = skills.Select(s => s.Difficulty).Distinct().ToList();
        diffs.Should().Contain("A", "deve ter perícias de dificuldade média");
        diffs.Should().Contain("H", "deve ter perícias difíceis");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Testes de Equipamentos (.eqp)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task LoadEquipment_DeveCarregarArquivoEqp()
    {
        var equipment = (await _loader.LoadEquipmentAsync(EqpPath)).ToList();

        equipment.Should().NotBeEmpty();
        equipment.Count.Should().BeGreaterThan(50);
    }

    [Fact]
    public async Task LoadEquipment_DeveConterPelaoMenosUmItemComCustoPositivo()
    {
        var equipment = await _loader.LoadEquipmentAsync(EqpPath);

        equipment.Should().Contain(e => e.Value > 0,
            "deve haver ao menos um equipamento com custo > 0");
    }

    [Fact]
    public async Task LoadEquipment_NenhumItemComNomeVazio()
    {
        var equipment = await _loader.LoadEquipmentAsync(EqpPath);

        equipment.Should().OnlyContain(e => !string.IsNullOrWhiteSpace(e.Name));
    }
}
