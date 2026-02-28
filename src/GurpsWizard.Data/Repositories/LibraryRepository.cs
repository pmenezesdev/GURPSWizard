using GurpsWizard.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GurpsWizard.Data.Repositories;

public class LibraryRepository(AppDbContext db) : ILibraryRepository
{
    public async Task<IReadOnlyList<LibraryTrait>> SearchTraitsAsync(string query, string? category = null, int? maxAbsCost = null)
    {
        var q = db.LibraryTraits.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(t => EF.Functions.Like(t.Name, $"%{query}%"));

        if (!string.IsNullOrWhiteSpace(category))
            q = q.Where(t => EF.Functions.Like(t.Tags, $"%{category}%"));

        var results = await q.OrderBy(t => t.Name).ToListAsync();

        if (maxAbsCost.HasValue)
            results = results.Where(t => Math.Abs(t.BasePoints) <= maxAbsCost.Value).ToList();

        return results;
    }

    public async Task<IReadOnlyList<LibrarySkill>> SearchSkillsAsync(string query, string? attribute = null, string? difficulty = null, string? category = null)
    {
        // Garante que técnicas (BaseAttribute == "") nunca aparecem na lista de perícias
        var q = db.LibrarySkills.Where(s => s.BaseAttribute != "");

        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(s => EF.Functions.Like(s.Name, $"%{query}%"));

        if (!string.IsNullOrWhiteSpace(attribute))
            q = q.Where(s => s.BaseAttribute == attribute);

        if (!string.IsNullOrWhiteSpace(difficulty))
            q = q.Where(s => s.Difficulty == difficulty);

        if (!string.IsNullOrWhiteSpace(category))
            q = q.Where(s => EF.Functions.Like(s.Tags, $"%{category}%"));

        return await q.OrderBy(s => s.Name).ToListAsync();
    }

    public async Task<IReadOnlyList<LibraryTechnique>> SearchTechniquesAsync(string query, string? difficulty = null, string? category = null)
    {
        var q = db.LibraryTechniques.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(t => EF.Functions.Like(t.Name, $"%{query}%")
                           || EF.Functions.Like(t.ParentSkillName, $"%{query}%"));

        if (!string.IsNullOrWhiteSpace(difficulty))
            q = q.Where(t => t.Difficulty == difficulty);

        if (!string.IsNullOrWhiteSpace(category))
            q = q.Where(t => EF.Functions.Like(t.Tags, $"%{category}%"));

        return await q.OrderBy(t => t.Name).ThenBy(t => t.ParentSkillName).ToListAsync();
    }

    public async Task<IReadOnlyList<LibraryEquipment>> SearchEquipmentAsync(string query, string? category = null)
    {
        var q = db.LibraryEquipment.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(e => EF.Functions.Like(e.Name, $"%{query}%"));

        if (!string.IsNullOrWhiteSpace(category))
            q = q.Where(e => EF.Functions.Like(e.Tags, $"%{category}%"));

        return await q.OrderBy(e => e.Name).ToListAsync();
    }

    public async Task<IReadOnlyList<string>> GetEquipmentCategoriesAsync()
    {
        var allTags = await db.LibraryEquipment
            .Select(e => e.Tags)
            .ToListAsync();

        return allTags
            .SelectMany(t => t.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(t => t.Trim())
            .Distinct()
            .OrderBy(t => t)
            .ToList();
    }

    public async Task<IReadOnlyList<string>> GetTraitCategoriesAsync()
    {
        var allTags = await db.LibraryTraits
            .Select(t => t.Tags)
            .ToListAsync();

        return allTags
            .SelectMany(t => t.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(t => t.Trim())
            .Distinct()
            .OrderBy(t => t)
            .ToList();
    }

    public async Task<IReadOnlyList<string>> GetSkillCategoriesAsync()
    {
        var allTags = await db.LibrarySkills
            .Where(s => s.BaseAttribute != "")
            .Select(s => s.Tags)
            .ToListAsync();

        return allTags
            .SelectMany(t => t.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(t => t.Trim())
            .Distinct()
            .OrderBy(t => t)
            .ToList();
    }

    public async Task<IReadOnlyList<LibrarySpell>> SearchSpellsAsync(string query, string? college = null, string? spellClass = null, string? difficulty = null)
    {
        var q = db.LibrarySpells.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(s => EF.Functions.Like(s.Name, $"%{query}%"));

        // College usa LIKE para capturar escolas compostas (ex: "Ar ou Clima" ao buscar "Ar")
        if (!string.IsNullOrWhiteSpace(college))
            q = q.Where(s => EF.Functions.Like(s.College, $"%{college}%"));

        if (!string.IsNullOrWhiteSpace(spellClass))
            q = q.Where(s => EF.Functions.Like(s.SpellClass ?? "", $"%{spellClass}%"));

        if (!string.IsNullOrWhiteSpace(difficulty))
            q = q.Where(s => s.Difficulty == difficulty);

        return await q.OrderBy(s => s.Name).ToListAsync();
    }

    public async Task<IReadOnlyList<string>> GetCollegesAsync()
    {
        var allColleges = await db.LibrarySpells
            .Where(s => s.College != "")
            .Select(s => s.College)
            .ToListAsync();

        // Extrai nomes atômicos de escolas compostas (ex: "Ar ou Clima" → ["Ar", "Clima"])
        return allColleges
            .SelectMany(c => c.Split(" ou ", StringSplitOptions.RemoveEmptyEntries))
            .Select(c => c.Trim())
            .Distinct()
            .OrderBy(c => c)
            .ToList();
    }

    public async Task<IReadOnlyList<string>> GetSpellClassesAsync()
    {
        var allClasses = await db.LibrarySpells
            .Where(s => s.SpellClass != null && s.SpellClass != "")
            .Select(s => s.SpellClass!)
            .ToListAsync();

        // Extrai classes atômicas de valores compostos (ex: "Comum/Área" → ["Comum", "Área"])
        return allClasses
            .SelectMany(c => c.Split(new[] { '/', ';', ',' }, StringSplitOptions.RemoveEmptyEntries))
            .Select(c => c.Trim())
            .Distinct()
            .OrderBy(c => c)
            .ToList();
    }

    public async Task<IReadOnlyList<string>> GetTechniqueCategoriesAsync()
    {
        var allTags = await db.LibraryTechniques
            .Select(t => t.Tags)
            .ToListAsync();

        return allTags
            .SelectMany(t => t.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(t => t.Trim())
            .Distinct()
            .OrderBy(t => t)
            .ToList();
    }
}
