using FluentAssertions;
using GurpsWizard.Data;
using GurpsWizard.Data.Entities;
using GurpsWizard.Data.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace GurpsWizard.Core.Tests;

/// <summary>
/// Testes de integração do LibraryRepository usando SQLite in-memory.
/// Cada método cria seu próprio banco isolado.
/// </summary>
public class LibraryRepositoryTests
{
    // ─── Helper ──────────────────────────────────────────────────────────────

    private static (AppDbContext db, SqliteConnection conn) CreateDb()
    {
        var conn = new SqliteConnection("DataSource=:memory:");
        conn.Open();
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(conn)
            .Options;
        var db = new AppDbContext(opts);
        db.Database.EnsureCreated();
        return (db, conn);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SearchTraitsAsync
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SearchTraitsAsync_QueryVazia_RetornaTodos()
    {
        var (db, conn) = CreateDb();
        try
        {
            db.LibraryTraits.AddRange(
                new LibraryTrait { GcsId = "1", Name = "Reflexos em Combate", BasePoints = 15 },
                new LibraryTrait { GcsId = "2", Name = "Acrofobia", BasePoints = -10 }
            );
            await db.SaveChangesAsync();

            var repo = new LibraryRepository(db);
            var results = await repo.SearchTraitsAsync("");

            results.Should().HaveCount(2);
        }
        finally { conn.Dispose(); }
    }

    [Fact]
    public async Task SearchTraitsAsync_QueryPorNome_FiltroLike()
    {
        var (db, conn) = CreateDb();
        try
        {
            db.LibraryTraits.AddRange(
                new LibraryTrait { GcsId = "1", Name = "Reflexos em Combate", BasePoints = 15 },
                new LibraryTrait { GcsId = "2", Name = "Acrofobia", BasePoints = -10 }
            );
            await db.SaveChangesAsync();

            var repo = new LibraryRepository(db);
            var results = await repo.SearchTraitsAsync("Reflex");

            results.Should().HaveCount(1);
            results[0].Name.Should().Be("Reflexos em Combate");
        }
        finally { conn.Dispose(); }
    }

    [Fact]
    public async Task SearchTraitsAsync_FiltroPorCategoria()
    {
        var (db, conn) = CreateDb();
        try
        {
            db.LibraryTraits.AddRange(
                new LibraryTrait { GcsId = "1", Name = "Ambidestria", BasePoints = 5, Tags = "Combate,Mental" },
                new LibraryTrait { GcsId = "2", Name = "Acrofobia", BasePoints = -10, Tags = "Mental" }
            );
            await db.SaveChangesAsync();

            var repo = new LibraryRepository(db);
            var results = await repo.SearchTraitsAsync("", category: "Combate");

            results.Should().HaveCount(1);
            results[0].Name.Should().Be("Ambidestria");
        }
        finally { conn.Dispose(); }
    }

    [Fact]
    public async Task SearchTraitsAsync_FiltroMaxAbsCost_RemoveAcimaDeLimite()
    {
        var (db, conn) = CreateDb();
        try
        {
            db.LibraryTraits.AddRange(
                new LibraryTrait { GcsId = "1", Name = "Barato", BasePoints = 5 },
                new LibraryTrait { GcsId = "2", Name = "Caro", BasePoints = 25 },
                new LibraryTrait { GcsId = "3", Name = "Desvantagem Pequena", BasePoints = -5 },
                new LibraryTrait { GcsId = "4", Name = "Desvantagem Grande", BasePoints = -30 }
            );
            await db.SaveChangesAsync();

            var repo = new LibraryRepository(db);
            var results = await repo.SearchTraitsAsync("", maxAbsCost: 10);

            results.Should().HaveCount(2);
            results.Should().OnlyContain(t => Math.Abs(t.BasePoints) <= 10);
        }
        finally { conn.Dispose(); }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SearchSkillsAsync
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SearchSkillsAsync_QueryVazia_RetornaTodasAsPericias()
    {
        var (db, conn) = CreateDb();
        try
        {
            db.LibrarySkills.AddRange(
                new LibrarySkill { GcsId = "1", Name = "Espadas", BaseAttribute = "DX", Difficulty = "A" },
                new LibrarySkill { GcsId = "2", Name = "Medicina", BaseAttribute = "IQ", Difficulty = "H" },
                new LibrarySkill { GcsId = "t1", Name = "Chute", BaseAttribute = "", Difficulty = "H" } // técnica
            );
            await db.SaveChangesAsync();

            var repo = new LibraryRepository(db);
            var results = await repo.SearchSkillsAsync("");

            // Técnica (BaseAttribute="") não deve aparecer
            results.Should().HaveCount(2);
            results.Should().OnlyContain(s => s.BaseAttribute != "");
        }
        finally { conn.Dispose(); }
    }

    [Fact]
    public async Task SearchSkillsAsync_FiltroPorAtributo_RetornaSoIQ()
    {
        var (db, conn) = CreateDb();
        try
        {
            db.LibrarySkills.AddRange(
                new LibrarySkill { GcsId = "1", Name = "Espadas", BaseAttribute = "DX", Difficulty = "A" },
                new LibrarySkill { GcsId = "2", Name = "Medicina", BaseAttribute = "IQ", Difficulty = "H" },
                new LibrarySkill { GcsId = "3", Name = "Diplomacia", BaseAttribute = "IQ", Difficulty = "A" }
            );
            await db.SaveChangesAsync();

            var repo = new LibraryRepository(db);
            var results = await repo.SearchSkillsAsync("", attribute: "IQ");

            results.Should().HaveCount(2);
            results.Should().OnlyContain(s => s.BaseAttribute == "IQ");
        }
        finally { conn.Dispose(); }
    }

    [Fact]
    public async Task SearchSkillsAsync_FiltroPorDificuldade_RetornaSoH()
    {
        var (db, conn) = CreateDb();
        try
        {
            db.LibrarySkills.AddRange(
                new LibrarySkill { GcsId = "1", Name = "Espadas", BaseAttribute = "DX", Difficulty = "A" },
                new LibrarySkill { GcsId = "2", Name = "Medicina", BaseAttribute = "IQ", Difficulty = "H" }
            );
            await db.SaveChangesAsync();

            var repo = new LibraryRepository(db);
            var results = await repo.SearchSkillsAsync("", difficulty: "H");

            results.Should().HaveCount(1);
            results[0].Difficulty.Should().Be("H");
        }
        finally { conn.Dispose(); }
    }

    [Fact]
    public async Task SearchSkillsAsync_NaoRetornaTecnicas()
    {
        var (db, conn) = CreateDb();
        try
        {
            db.LibrarySkills.AddRange(
                new LibrarySkill { GcsId = "1", Name = "Espadas", BaseAttribute = "DX", Difficulty = "A" },
                new LibrarySkill { GcsId = "t1", Name = "Chute", BaseAttribute = "", Difficulty = "H" }
            );
            await db.SaveChangesAsync();

            var repo = new LibraryRepository(db);
            var results = await repo.SearchSkillsAsync("");

            results.Should().NotContain(s => s.BaseAttribute == "");
        }
        finally { conn.Dispose(); }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SearchTechniquesAsync
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SearchTechniquesAsync_QueryVazia_RetornaTodasAsTecnicas()
    {
        var (db, conn) = CreateDb();
        try
        {
            db.LibraryTechniques.AddRange(
                new LibraryTechnique { GcsId = "1", Name = "Chute", Difficulty = "H", ParentSkillName = "Caratê" },
                new LibraryTechnique { GcsId = "2", Name = "Banda", Difficulty = "A", ParentSkillName = "Briga" }
            );
            await db.SaveChangesAsync();

            var repo = new LibraryRepository(db);
            var results = await repo.SearchTechniquesAsync("");

            results.Should().HaveCount(2);
        }
        finally { conn.Dispose(); }
    }

    [Fact]
    public async Task SearchTechniquesAsync_FiltroPorDificuldade()
    {
        var (db, conn) = CreateDb();
        try
        {
            db.LibraryTechniques.AddRange(
                new LibraryTechnique { GcsId = "1", Name = "Chute", Difficulty = "H", ParentSkillName = "Caratê" },
                new LibraryTechnique { GcsId = "2", Name = "Banda", Difficulty = "A", ParentSkillName = "Briga" }
            );
            await db.SaveChangesAsync();

            var repo = new LibraryRepository(db);
            var results = await repo.SearchTechniquesAsync("", difficulty: "A");

            results.Should().HaveCount(1);
            results[0].Difficulty.Should().Be("A");
        }
        finally { conn.Dispose(); }
    }

    [Fact]
    public async Task SearchTechniquesAsync_QueryFiltroPericiaPai_ViaLike()
    {
        var (db, conn) = CreateDb();
        try
        {
            db.LibraryTechniques.AddRange(
                new LibraryTechnique { GcsId = "1", Name = "Chute", Difficulty = "H", ParentSkillName = "Caratê" },
                new LibraryTechnique { GcsId = "2", Name = "Banda", Difficulty = "A", ParentSkillName = "Briga" },
                new LibraryTechnique { GcsId = "3", Name = "Joelhada", Difficulty = "H", ParentSkillName = "Caratê" }
            );
            await db.SaveChangesAsync();

            var repo = new LibraryRepository(db);
            // Query "Caratê" deve bater pelo ParentSkillName
            var results = await repo.SearchTechniquesAsync("Carat");

            results.Should().HaveCount(2);
            results.Should().OnlyContain(t => t.ParentSkillName == "Caratê");
        }
        finally { conn.Dispose(); }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SearchSpellsAsync
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SearchSpellsAsync_QueryVazia_RetornaTodasAsMagicas()
    {
        var (db, conn) = CreateDb();
        try
        {
            db.LibrarySpells.AddRange(
                new LibrarySpell { GcsId = "1", Name = "Bola de Fogo", College = "Fogo", Difficulty = "H" },
                new LibrarySpell { GcsId = "2", Name = "Curar", College = "Corpo", Difficulty = "H" }
            );
            await db.SaveChangesAsync();

            var repo = new LibraryRepository(db);
            var results = await repo.SearchSpellsAsync("");

            results.Should().HaveCount(2);
        }
        finally { conn.Dispose(); }
    }

    [Fact]
    public async Task SearchSpellsAsync_FiltroPorCollege_ViaLike()
    {
        var (db, conn) = CreateDb();
        try
        {
            db.LibrarySpells.AddRange(
                new LibrarySpell { GcsId = "1", Name = "Tempestade", College = "Ar ou Clima", Difficulty = "H" },
                new LibrarySpell { GcsId = "2", Name = "Bola de Fogo", College = "Fogo", Difficulty = "H" }
            );
            await db.SaveChangesAsync();

            var repo = new LibraryRepository(db);
            // Busca "Ar" deve encontrar "Ar ou Clima" via LIKE
            var results = await repo.SearchSpellsAsync("", college: "Ar");

            results.Should().HaveCount(1);
            results[0].Name.Should().Be("Tempestade");
        }
        finally { conn.Dispose(); }
    }

    [Fact]
    public async Task SearchSpellsAsync_FiltroPorSpellClass()
    {
        var (db, conn) = CreateDb();
        try
        {
            db.LibrarySpells.AddRange(
                new LibrarySpell { GcsId = "1", Name = "Bola de Fogo", College = "Fogo", SpellClass = "Projétil", Difficulty = "H" },
                new LibrarySpell { GcsId = "2", Name = "Cura", College = "Corpo", SpellClass = "Comum", Difficulty = "H" }
            );
            await db.SaveChangesAsync();

            var repo = new LibraryRepository(db);
            var results = await repo.SearchSpellsAsync("", spellClass: "Comum");

            results.Should().HaveCount(1);
            results[0].SpellClass.Should().Be("Comum");
        }
        finally { conn.Dispose(); }
    }

    [Fact]
    public async Task SearchSpellsAsync_FiltroPorDificuldade()
    {
        var (db, conn) = CreateDb();
        try
        {
            db.LibrarySpells.AddRange(
                new LibrarySpell { GcsId = "1", Name = "Bola de Fogo", College = "Fogo", Difficulty = "H" },
                new LibrarySpell { GcsId = "2", Name = "Mestre das Chamas", College = "Fogo", Difficulty = "VH" }
            );
            await db.SaveChangesAsync();

            var repo = new LibraryRepository(db);
            var results = await repo.SearchSpellsAsync("", difficulty: "VH");

            results.Should().HaveCount(1);
            results[0].Difficulty.Should().Be("VH");
        }
        finally { conn.Dispose(); }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Métodos de categoria
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetTraitCategoriesAsync_RetornaTagsUnicas()
    {
        var (db, conn) = CreateDb();
        try
        {
            db.LibraryTraits.AddRange(
                new LibraryTrait { GcsId = "1", Name = "Ambidestria", BasePoints = 5, Tags = "Combate,Mental" },
                new LibraryTrait { GcsId = "2", Name = "Acrofobia", BasePoints = -10, Tags = "Mental,Fobia" }
            );
            await db.SaveChangesAsync();

            var repo = new LibraryRepository(db);
            var categories = await repo.GetTraitCategoriesAsync();

            categories.Should().BeEquivalentTo(new[] { "Combate", "Fobia", "Mental" });
        }
        finally { conn.Dispose(); }
    }

    [Fact]
    public async Task GetSkillCategoriesAsync_ExcluiTecnicas()
    {
        var (db, conn) = CreateDb();
        try
        {
            db.LibrarySkills.AddRange(
                new LibrarySkill { GcsId = "1", Name = "Espadas", BaseAttribute = "DX", Difficulty = "A", Tags = "Combate" },
                new LibrarySkill { GcsId = "t1", Name = "Chute", BaseAttribute = "", Difficulty = "H", Tags = "Técnica" }
            );
            await db.SaveChangesAsync();

            var repo = new LibraryRepository(db);
            var categories = await repo.GetSkillCategoriesAsync();

            categories.Should().Contain("Combate");
            categories.Should().NotContain("Técnica");
        }
        finally { conn.Dispose(); }
    }

    [Fact]
    public async Task GetTechniqueCategoriesAsync_RetornaTagsDeTecnicas()
    {
        var (db, conn) = CreateDb();
        try
        {
            db.LibraryTechniques.AddRange(
                new LibraryTechnique { GcsId = "1", Name = "Chute", Difficulty = "H", ParentSkillName = "Caratê", Tags = "Combate,Artes Marciais" }
            );
            await db.SaveChangesAsync();

            var repo = new LibraryRepository(db);
            var categories = await repo.GetTechniqueCategoriesAsync();

            categories.Should().Contain("Combate");
            categories.Should().Contain("Artes Marciais");
        }
        finally { conn.Dispose(); }
    }

    [Fact]
    public async Task GetCollegesAsync_DivideCollegiosCompostos()
    {
        var (db, conn) = CreateDb();
        try
        {
            db.LibrarySpells.AddRange(
                new LibrarySpell { GcsId = "1", Name = "Tempestade", College = "Ar ou Clima", Difficulty = "H" },
                new LibrarySpell { GcsId = "2", Name = "Bola de Fogo", College = "Fogo", Difficulty = "H" }
            );
            await db.SaveChangesAsync();

            var repo = new LibraryRepository(db);
            var colleges = await repo.GetCollegesAsync();

            colleges.Should().Contain("Ar");
            colleges.Should().Contain("Clima");
            colleges.Should().Contain("Fogo");
            colleges.Should().NotContain("Ar ou Clima"); // deve ser dividido
        }
        finally { conn.Dispose(); }
    }

    [Fact]
    public async Task GetSpellClassesAsync_DivideClassesCompostas()
    {
        var (db, conn) = CreateDb();
        try
        {
            db.LibrarySpells.AddRange(
                new LibrarySpell { GcsId = "1", Name = "Chuva de Fogo", College = "Fogo", SpellClass = "Comum/Área", Difficulty = "H" },
                new LibrarySpell { GcsId = "2", Name = "Curar", College = "Corpo", SpellClass = "Comum", Difficulty = "H" }
            );
            await db.SaveChangesAsync();

            var repo = new LibraryRepository(db);
            var classes = await repo.GetSpellClassesAsync();

            classes.Should().Contain("Comum");
            classes.Should().Contain("Área");
        }
        finally { conn.Dispose(); }
    }

    [Fact]
    public async Task GetEquipmentCategoriesAsync_RetornaTagsUnicas()
    {
        var (db, conn) = CreateDb();
        try
        {
            db.LibraryEquipment.AddRange(
                new LibraryEquipment { GcsId = "1", Name = "Espada Longa", Value = 700, Tags = "Armas,Melee" },
                new LibraryEquipment { GcsId = "2", Name = "Elmo", Value = 60, Tags = "Armadura" }
            );
            await db.SaveChangesAsync();

            var repo = new LibraryRepository(db);
            var categories = await repo.GetEquipmentCategoriesAsync();

            categories.Should().Contain("Armas");
            categories.Should().Contain("Melee");
            categories.Should().Contain("Armadura");
        }
        finally { conn.Dispose(); }
    }
}
