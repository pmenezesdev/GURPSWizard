using GurpsWizard.Data.Gcs;
using Microsoft.EntityFrameworkCore;

namespace GurpsWizard.Data;

/// <summary>
/// Verifica se o banco SQLite existe e está populado.
/// Na primeira execução, aplica as migrations e carrega todos os dados GCS.
/// Nas execuções subsequentes, carrega apenas as tabelas ainda vazias
/// (permite recuperação de carga parcial sem apagar personagens salvos).
/// </summary>
public class DatabaseInitializer(AppDbContext db, GcsLoader loader)
{
    /// <summary>
    /// Inicializa o banco. Retorna true se alguma dado foi carregado agora.
    /// <paramref name="gcsDataPath"/> deve apontar para o diretório raiz de gcs-ptbr.
    /// </summary>
    public async Task<bool> InitializeAsync(string gcsDataPath, IProgress<string>? progress = null)
    {
        progress?.Report("Aplicando migrações do banco de dados…");
        await db.Database.MigrateAsync();

        bool anyLoaded = false;

        if (!await db.LibraryTraits.AnyAsync())
        {
            await LoadTraitsAsync(gcsDataPath, progress);
            anyLoaded = true;
        }

        if (!await db.LibrarySkills.AnyAsync())
        {
            await LoadSkillsAsync(gcsDataPath, progress);
            anyLoaded = true;
        }

        if (!await db.LibraryEquipment.AnyAsync())
        {
            await LoadEquipmentAsync(gcsDataPath, progress);
            anyLoaded = true;
        }

        if (!anyLoaded)
            progress?.Report("Biblioteca já carregada.");
        else
            progress?.Report("Biblioteca pronta!");

        return anyLoaded;
    }

    // ── Vantagens e desvantagens (.adq) ───────────────────────────────────────

    private async Task LoadTraitsAsync(string basePath, IProgress<string>? progress)
    {
        var adqFiles = Directory.GetFiles(basePath, "*.adq", SearchOption.AllDirectories);

        var all = new Dictionary<string, Entities.LibraryTrait>(StringComparer.Ordinal);
        foreach (var file in adqFiles)
        {
            progress?.Report($"Lendo vantagens: {Path.GetFileName(file)}…");
            foreach (var t in await loader.LoadTraitsAsync(file))
                all.TryAdd(t.GcsId, t);
        }

        var existing = await db.LibraryTraits.Select(t => t.GcsId).ToHashSetAsync();
        foreach (var t in all.Values.Where(t => !existing.Contains(t.GcsId)))
            db.LibraryTraits.Add(t);

        progress?.Report($"Vantagens e desvantagens: {all.Count} itens. Salvando…");
        await db.SaveChangesAsync();
    }

    // ── Perícias (.skl) ───────────────────────────────────────────────────────

    private async Task LoadSkillsAsync(string basePath, IProgress<string>? progress)
    {
        var sklFiles = Directory.GetFiles(basePath, "*.skl", SearchOption.AllDirectories);

        var all = new Dictionary<string, Entities.LibrarySkill>(StringComparer.Ordinal);
        foreach (var file in sklFiles)
        {
            progress?.Report($"Lendo perícias: {Path.GetFileName(file)}…");
            foreach (var s in await loader.LoadSkillsAsync(file))
                all.TryAdd(s.GcsId, s);
        }

        var existing = await db.LibrarySkills.Select(s => s.GcsId).ToHashSetAsync();
        foreach (var s in all.Values.Where(s => !existing.Contains(s.GcsId)))
            db.LibrarySkills.Add(s);

        progress?.Report($"Perícias: {all.Count} itens. Salvando…");
        await db.SaveChangesAsync();
    }

    // ── Equipamentos (.eqp) ───────────────────────────────────────────────────

    private async Task LoadEquipmentAsync(string basePath, IProgress<string>? progress)
    {
        var eqpFiles = Directory.GetFiles(basePath, "*.eqp", SearchOption.AllDirectories);

        var all = new Dictionary<string, Entities.LibraryEquipment>(StringComparer.Ordinal);
        foreach (var file in eqpFiles)
        {
            progress?.Report($"Lendo equipamentos: {Path.GetFileName(file)}…");
            foreach (var e in await loader.LoadEquipmentAsync(file))
                all.TryAdd(e.GcsId, e);
        }

        var existing = await db.LibraryEquipment.Select(e => e.GcsId).ToHashSetAsync();
        foreach (var e in all.Values.Where(e => !existing.Contains(e.GcsId)))
            db.LibraryEquipment.Add(e);

        progress?.Report($"Equipamentos: {all.Count} itens. Salvando…");
        await db.SaveChangesAsync();
    }
}
