using GurpsWizard.Data.Entities;

namespace GurpsWizard.Data.Repositories;

public interface ILibraryRepository
{
    Task<IReadOnlyList<LibraryTrait>>      SearchTraitsAsync(string query, string? category = null, int? maxAbsCost = null);
    Task<IReadOnlyList<LibrarySkill>>      SearchSkillsAsync(string query, string? attribute = null, string? difficulty = null, string? category = null);
    Task<IReadOnlyList<LibraryTechnique>>  SearchTechniquesAsync(string query, string? difficulty = null, string? category = null);
    Task<IReadOnlyList<LibrarySpell>>      SearchSpellsAsync(string query, string? college = null, string? spellClass = null, string? difficulty = null);
    Task<IReadOnlyList<LibraryEquipment>>  SearchEquipmentAsync(string query, string? category = null);
    Task<IReadOnlyList<string>>            GetTraitCategoriesAsync();
    Task<IReadOnlyList<string>>            GetSkillCategoriesAsync();
    Task<IReadOnlyList<string>>            GetTechniqueCategoriesAsync();
    Task<IReadOnlyList<string>>            GetCollegesAsync();
    Task<IReadOnlyList<string>>            GetSpellClassesAsync();
    Task<IReadOnlyList<string>>            GetEquipmentCategoriesAsync();
}
