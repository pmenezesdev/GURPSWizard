using GurpsWizard.Data.Entities;
using GurpsWizard.Core.Models;

namespace GurpsWizard.Data.Repositories;

public interface ICharacterRepository
{
    Task<CharacterEntity> SaveAsync(CharacterDraft draft, int? existingId = null);
    Task<CharacterDraft?> GetByIdAsync(int id);
    Task<IReadOnlyList<CharacterEntity>> ListAllAsync();
    Task DeleteAsync(int id);
}
