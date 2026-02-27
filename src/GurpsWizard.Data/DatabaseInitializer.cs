using GurpsWizard.Data.Gcs;
using Microsoft.EntityFrameworkCore;

namespace GurpsWizard.Data;

/// <summary>
/// Verifica se o banco SQLite existe e está populado.
/// Na primeira execução, aplica as migrations e carrega todos os dados GCS.
/// </summary>
public class DatabaseInitializer(AppDbContext db, GcsLoader loader)
{
    /// <summary>
    /// Inicializa o banco. Retorna true se os dados foram carregados agora (primeira execução).
    /// <paramref name="gcsDataPath"/> deve apontar para o diretório raiz de gcs-ptbr.
    /// <paramref name="progress"/> recebe mensagens de status durante a carga (opcional).
    /// </summary>
    public async Task<bool> InitializeAsync(string gcsDataPath, IProgress<string>? progress = null)
    {
        progress?.Report("Aplicando migrações do banco de dados…");
        await db.Database.MigrateAsync();

        if (await db.LibraryTraits.AnyAsync())
        {
            progress?.Report("Biblioteca já carregada.");
            return false;
        }

        await LoadLibraryAsync(gcsDataPath, progress);
        return true;
    }

    private async Task LoadLibraryAsync(string basePath, IProgress<string>? progress)
    {
        // ── Vantagens e desvantagens (.adq) ───────────────────────────────────
        // Carrega todos os arquivos, deduplica por GcsId em memória,
        // depois insere apenas os que ainda não existem no banco.
        var adqFiles = Directory.GetFiles(basePath, "*.adq", SearchOption.AllDirectories);

        var allTraits = new Dictionary<string, Entities.LibraryTrait>(StringComparer.Ordinal);
        foreach (var file in adqFiles)
        {
            progress?.Report($"Lendo vantagens: {Path.GetFileName(file)}…");
            foreach (var t in await loader.LoadTraitsAsync(file))
                allTraits.TryAdd(t.GcsId, t);
        }

        var existingTraitIds = await db.LibraryTraits
            .Select(t => t.GcsId).ToHashSetAsync();

        foreach (var t in allTraits.Values.Where(t => !existingTraitIds.Contains(t.GcsId)))
            db.LibraryTraits.Add(t);

        progress?.Report($"Vantagens e desvantagens: {allTraits.Count} itens. Salvando…");
        await db.SaveChangesAsync();

        // ── Perícias (.skl) ───────────────────────────────────────────────────
        var sklFiles = Directory.GetFiles(basePath, "*.skl", SearchOption.AllDirectories);

        var allSkills = new Dictionary<string, Entities.LibrarySkill>(StringComparer.Ordinal);
        foreach (var file in sklFiles)
        {
            progress?.Report($"Lendo perícias: {Path.GetFileName(file)}…");
            foreach (var s in await loader.LoadSkillsAsync(file))
                allSkills.TryAdd(s.GcsId, s);
        }

        var existingSkillIds = await db.LibrarySkills
            .Select(s => s.GcsId).ToHashSetAsync();

        foreach (var s in allSkills.Values.Where(s => !existingSkillIds.Contains(s.GcsId)))
            db.LibrarySkills.Add(s);

        progress?.Report($"Perícias: {allSkills.Count} itens. Salvando…");
        await db.SaveChangesAsync();

        // ── Equipamentos (.eqp) ───────────────────────────────────────────────
        var eqpFiles = Directory.GetFiles(basePath, "*.eqp", SearchOption.AllDirectories);

        var allEquipment = new Dictionary<string, Entities.LibraryEquipment>(StringComparer.Ordinal);
        foreach (var file in eqpFiles)
        {
            progress?.Report($"Lendo equipamentos: {Path.GetFileName(file)}…");
            foreach (var e in await loader.LoadEquipmentAsync(file))
                allEquipment.TryAdd(e.GcsId, e);
        }

        var existingEqpIds = await db.LibraryEquipment
            .Select(e => e.GcsId).ToHashSetAsync();

        foreach (var e in allEquipment.Values.Where(e => !existingEqpIds.Contains(e.GcsId)))
            db.LibraryEquipment.Add(e);

        progress?.Report($"Equipamentos: {allEquipment.Count} itens. Salvando…");
        await db.SaveChangesAsync();

        progress?.Report("Biblioteca pronta!");
    }
}
