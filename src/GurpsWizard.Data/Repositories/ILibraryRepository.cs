using GurpsWizard.Data.Entities;

namespace GurpsWizard.Data.Repositories;

public interface ILibraryRepository
{
    Task<IReadOnlyList<LibraryTrait>>     SearchTraitsAsync(string query, string? category = null);
    Task<IReadOnlyList<LibrarySkill>>     SearchSkillsAsync(string query);
    Task<IReadOnlyList<LibraryEquipment>> SearchEquipmentAsync(string query);
    Task<IReadOnlyList<string>>           GetTraitCategoriesAsync();
}
