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
        var q = db.LibrarySkills.AsQueryable();

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
            .Select(s => s.Tags)
            .ToListAsync();

        return allTags
            .SelectMany(t => t.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(t => t.Trim())
            .Distinct()
            .OrderBy(t => t)
            .ToList();
    }
}
