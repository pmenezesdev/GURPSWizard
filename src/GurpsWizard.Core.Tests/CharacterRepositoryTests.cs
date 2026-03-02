using FluentAssertions;
using GurpsWizard.Core.Models;
using GurpsWizard.Data;
using GurpsWizard.Data.Entities;
using GurpsWizard.Data.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace GurpsWizard.Core.Tests;

/// <summary>
/// Testes de integração do CharacterRepository usando SQLite in-memory.
/// </summary>
public class CharacterRepositoryTests
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

    private static CharacterDraft SampleDraft(string name = "Thorin") =>
        CharacterDraft.Empty() with
        {
            Name = name,
            TotalPoints = 150,
            Skills = [new SkillEntry("s1", "Espadas", "DX", "A", Level: 0, Cost: 2)],
        };

    // ─────────────────────────────────────────────────────────────────────────
    // SaveAsync — novo personagem
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SaveAsync_NewCharacter_AssignsPositiveId()
    {
        var (db, conn) = CreateDb();
        try
        {
            var repo = new CharacterRepository(db);
            var entity = await repo.SaveAsync(SampleDraft());

            entity.Id.Should().BeGreaterThan(0);
            entity.Name.Should().Be("Thorin");
        }
        finally { conn.Dispose(); }
    }

    [Fact]
    public async Task SaveAsync_NewCharacter_PersistedInDatabase()
    {
        var (db, conn) = CreateDb();
        try
        {
            var repo = new CharacterRepository(db);
            var saved = await repo.SaveAsync(SampleDraft("Gandalf"));

            var found = await db.Characters.FindAsync(saved.Id);
            found.Should().NotBeNull();
            found!.Name.Should().Be("Gandalf");
        }
        finally { conn.Dispose(); }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SaveAsync — atualizar existente
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SaveAsync_UpdatesExisting_ByExistingId()
    {
        var (db, conn) = CreateDb();
        try
        {
            var repo = new CharacterRepository(db);
            var created = await repo.SaveAsync(SampleDraft("Gimli"));
            var originalUpdated = created.UpdatedAt;

            // Pequena pausa para garantir que UpdatedAt mude
            await Task.Delay(10);

            var updatedDraft = SampleDraft("Gimli o Anão");
            var updated = await repo.SaveAsync(updatedDraft, existingId: created.Id);

            updated.Id.Should().Be(created.Id);
            updated.Name.Should().Be("Gimli o Anão");
        }
        finally { conn.Dispose(); }
    }

    [Fact]
    public async Task SaveAsync_NonExistentId_ThrowsKeyNotFoundException()
    {
        var (db, conn) = CreateDb();
        try
        {
            var repo = new CharacterRepository(db);
            var act = async () => await repo.SaveAsync(SampleDraft(), existingId: 9999);

            await act.Should().ThrowAsync<KeyNotFoundException>();
        }
        finally { conn.Dispose(); }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GetByIdAsync
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ReturnsCorrectDraft()
    {
        var (db, conn) = CreateDb();
        try
        {
            var repo = new CharacterRepository(db);
            var saved = await repo.SaveAsync(SampleDraft("Legolas"));

            var draft = await repo.GetByIdAsync(saved.Id);

            draft.Should().NotBeNull();
            draft!.Name.Should().Be("Legolas");
            draft.TotalPoints.Should().Be(150);
            draft.Skills.Should().HaveCount(1);
        }
        finally { conn.Dispose(); }
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentId_ReturnsNull()
    {
        var (db, conn) = CreateDb();
        try
        {
            var repo = new CharacterRepository(db);
            var draft = await repo.GetByIdAsync(9999);

            draft.Should().BeNull();
        }
        finally { conn.Dispose(); }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ListAllAsync
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ListAllAsync_OrderedByUpdatedAtDescendente()
    {
        var (db, conn) = CreateDb();
        try
        {
            var now = DateTime.UtcNow;
            db.Characters.AddRange(
                new CharacterEntity { Name = "Mais Antigo", TotalPoints = 100, DraftJson = "{}", UpdatedAt = now.AddDays(-2) },
                new CharacterEntity { Name = "Mais Recente", TotalPoints = 100, DraftJson = "{}", UpdatedAt = now },
                new CharacterEntity { Name = "Intermediário", TotalPoints = 100, DraftJson = "{}", UpdatedAt = now.AddDays(-1) }
            );
            await db.SaveChangesAsync();

            var repo = new CharacterRepository(db);
            var list = await repo.ListAllAsync();

            list.Should().HaveCount(3);
            list[0].Name.Should().Be("Mais Recente");
            list[1].Name.Should().Be("Intermediário");
            list[2].Name.Should().Be("Mais Antigo");
        }
        finally { conn.Dispose(); }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // DeleteAsync
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_RemovesCharacter()
    {
        var (db, conn) = CreateDb();
        try
        {
            var repo = new CharacterRepository(db);
            var saved = await repo.SaveAsync(SampleDraft("Sauron"));

            await repo.DeleteAsync(saved.Id);

            var found = await repo.GetByIdAsync(saved.Id);
            found.Should().BeNull();
        }
        finally { conn.Dispose(); }
    }

    [Fact]
    public async Task DeleteAsync_NonExistentId_DoesNotThrow()
    {
        var (db, conn) = CreateDb();
        try
        {
            var repo = new CharacterRepository(db);
            var act = async () => await repo.DeleteAsync(9999);

            await act.Should().NotThrowAsync();
        }
        finally { conn.Dispose(); }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Compatibilidade com saves antigos (sem Techniques/Spells)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_NullTechniques_ReturnsEmptyList()
    {
        var (db, conn) = CreateDb();
        try
        {
            // JSON de save legado — sem campo "Techniques"
            var legacyJson = """
                {
                    "Name": "Legado",
                    "Description": "",
                    "TotalPoints": 100,
                    "Attributes": {"ST":10,"DX":10,"IQ":10,"HT":10},
                    "SecondaryAttributes": {"HPBonus":0,"FPBonus":0,"WillBonus":0,"PerBonus":0,"BasicSpeedBonus":0,"BasicMoveBonus":0},
                    "Advantages": [],
                    "Disadvantages": [],
                    "Skills": [],
                    "Equipment": []
                }
                """;
            db.Characters.Add(new CharacterEntity { Name = "Legado", TotalPoints = 100, DraftJson = legacyJson });
            await db.SaveChangesAsync();

            var entity = await db.Characters.FirstAsync(c => c.Name == "Legado");
            var repo = new CharacterRepository(db);
            var draft = await repo.GetByIdAsync(entity.Id);

            draft.Should().NotBeNull();
            draft!.Techniques.Should().NotBeNull().And.BeEmpty();
        }
        finally { conn.Dispose(); }
    }

    [Fact]
    public async Task GetByIdAsync_NullSpells_ReturnsEmptyList()
    {
        var (db, conn) = CreateDb();
        try
        {
            // JSON de save legado — sem campo "Spells"
            var legacyJson = """
                {
                    "Name": "Legado2",
                    "Description": "",
                    "TotalPoints": 100,
                    "Attributes": {"ST":10,"DX":10,"IQ":10,"HT":10},
                    "SecondaryAttributes": {"HPBonus":0,"FPBonus":0,"WillBonus":0,"PerBonus":0,"BasicSpeedBonus":0,"BasicMoveBonus":0},
                    "Advantages": [],
                    "Disadvantages": [],
                    "Skills": [],
                    "Techniques": [],
                    "Equipment": []
                }
                """;
            db.Characters.Add(new CharacterEntity { Name = "Legado2", TotalPoints = 100, DraftJson = legacyJson });
            await db.SaveChangesAsync();

            var entity = await db.Characters.FirstAsync(c => c.Name == "Legado2");
            var repo = new CharacterRepository(db);
            var draft = await repo.GetByIdAsync(entity.Id);

            draft.Should().NotBeNull();
            draft!.Spells.Should().NotBeNull().And.BeEmpty();
        }
        finally { conn.Dispose(); }
    }

    [Fact]
    public async Task SaveAsync_PreservesAllDraftFields()
    {
        var (db, conn) = CreateDb();
        try
        {
            var fullDraft = new CharacterDraft(
                Name: "Aragorn",
                Description: "Rei de Gondor",
                TotalPoints: 200,
                Attributes: new Attributes(ST: 12, DX: 12, IQ: 11, HT: 12),
                SecondaryAttributes: SecondaryAttributes.Default with { HPBonus = 2 },
                Advantages: [new TraitEntry("v1", "Reflexos em Combate", 15)],
                Disadvantages: [new TraitEntry("d1", "Senso do Dever", -15)],
                Skills: [new SkillEntry("s1", "Espadas", "DX", "A", Level: 1, Cost: 4)],
                Techniques: [new TechniqueEntry("t1", "Truque de Espada", "H", LevelsAboveDefault: 1, Cost: 2)],
                Spells: [new SpellEntry("sp1", "Curar", "Corpo", "H", Level: 0, Cost: 4)],
                Equipment: [new EquipmentEntry("e1", "Andúril", Value: 700m, Weight: "3.5 lb")]
            );

            var repo = new CharacterRepository(db);
            var saved = await repo.SaveAsync(fullDraft);
            var loaded = await repo.GetByIdAsync(saved.Id);

            loaded.Should().NotBeNull();
            loaded!.Name.Should().Be("Aragorn");
            loaded.TotalPoints.Should().Be(200);
            loaded.Attributes.ST.Should().Be(12);
            loaded.Skills.Should().HaveCount(1);
            loaded.Techniques.Should().HaveCount(1);
            loaded.Spells.Should().HaveCount(1);
            loaded.Equipment.Should().HaveCount(1);
            loaded.Advantages.Should().HaveCount(1);
            loaded.Disadvantages.Should().HaveCount(1);
        }
        finally { conn.Dispose(); }
    }
}
