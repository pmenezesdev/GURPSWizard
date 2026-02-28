using System.Text.Json;
using GurpsWizard.Core.Models;
using GurpsWizard.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GurpsWizard.Data.Repositories;

public class CharacterRepository(AppDbContext db) : ICharacterRepository
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = false,
    };

    public async Task<CharacterEntity> SaveAsync(CharacterDraft draft, int? existingId = null)
    {
        var json = JsonSerializer.Serialize(draft, JsonOpts);
        var now  = DateTime.UtcNow;

        CharacterEntity entity;

        if (existingId.HasValue)
        {
            entity = await db.Characters.FindAsync(existingId.Value)
                     ?? throw new KeyNotFoundException($"Personagem {existingId} não encontrado.");
            entity.Name        = draft.Name;
            entity.TotalPoints = draft.TotalPoints;
            entity.DraftJson   = json;
            entity.UpdatedAt   = now;
        }
        else
        {
            entity = new CharacterEntity
            {
                Name        = draft.Name,
                TotalPoints = draft.TotalPoints,
                DraftJson   = json,
                CreatedAt   = now,
                UpdatedAt   = now,
            };
            db.Characters.Add(entity);
        }

        await db.SaveChangesAsync();
        return entity;
    }

    public async Task<CharacterDraft?> GetByIdAsync(int id)
    {
        var entity = await db.Characters.FindAsync(id);
        if (entity is null) return null;

        var draft = JsonSerializer.Deserialize<CharacterDraft>(entity.DraftJson, JsonOpts);
        // Compatibilidade com saves anteriores: campos novos chegam null quando ausentes no JSON
        if (draft is not null && draft.Techniques is null)
            draft = draft with { Techniques = [] };
        if (draft is not null && draft.Spells is null)
            draft = draft with { Spells = [] };
        return draft;
    }

    public async Task<IReadOnlyList<CharacterEntity>> ListAllAsync() =>
        await db.Characters.OrderByDescending(c => c.UpdatedAt).ToListAsync();

    public async Task DeleteAsync(int id)
    {
        var entity = await db.Characters.FindAsync(id);
        if (entity is not null)
        {
            db.Characters.Remove(entity);
            await db.SaveChangesAsync();
        }
    }
}
