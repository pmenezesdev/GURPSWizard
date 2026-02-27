using GurpsWizard.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GurpsWizard.Data.Repositories;

public class LibraryRepository(AppDbContext db) : ILibraryRepository
{
    public async Task<IReadOnlyList<LibraryTrait>> SearchTraitsAsync(string query, string? category = null)
    {
        var q = db.LibraryTraits.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(t => EF.Functions.Like(t.Name, $"%{query}%"));

        if (!string.IsNullOrWhiteSpace(category))
            q = q.Where(t => EF.Functions.Like(t.Tags, $"%{category}%"));

        return await q.OrderBy(t => t.Name).ToListAsync();
    }

    public async Task<IReadOnlyList<LibrarySkill>> SearchSkillsAsync(string query)
    {
        var q = db.LibrarySkills.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(s => EF.Functions.Like(s.Name, $"%{query}%"));

        return await q.OrderBy(s => s.Name).ToListAsync();
    }

    public async Task<IReadOnlyList<LibraryEquipment>> SearchEquipmentAsync(string query)
    {
        var q = db.LibraryEquipment.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(e => EF.Functions.Like(e.Name, $"%{query}%"));

        return await q.OrderBy(e => e.Name).ToListAsync();
    }

    public async Task<IReadOnlyList<string>> GetTraitCategoriesAsync()
    {
        // As categorias estão armazenadas como "Vantagem,Exótica,Física".
        // Extraímos valores únicos em memória (tabela pequena).
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
}
