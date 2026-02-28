using GurpsWizard.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GurpsWizard.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // Biblioteca (read-only, populada pelo GcsLoader)
    public DbSet<LibraryTrait>      LibraryTraits      { get; set; }
    public DbSet<LibrarySkill>      LibrarySkills      { get; set; }
    public DbSet<LibraryTechnique>  LibraryTechniques  { get; set; }
    public DbSet<LibrarySpell>      LibrarySpells      { get; set; }
    public DbSet<LibraryEquipment>  LibraryEquipment   { get; set; }

    // Personagens e campanhas (read/write)
    public DbSet<CharacterEntity>  Characters        { get; set; }
    public DbSet<CampaignEntity>   Campaigns         { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LibraryTrait>(e =>
        {
            e.HasIndex(t => t.Name);
            e.HasIndex(t => t.GcsId).IsUnique();
        });

        modelBuilder.Entity<LibrarySkill>(e =>
        {
            e.HasIndex(s => s.Name);
            e.HasIndex(s => s.GcsId).IsUnique();
        });

        modelBuilder.Entity<LibraryTechnique>(e =>
        {
            e.HasIndex(t => t.Name);
            e.HasIndex(t => t.GcsId).IsUnique();
        });

        modelBuilder.Entity<LibrarySpell>(e =>
        {
            e.HasIndex(s => s.Name);
            e.HasIndex(s => s.GcsId).IsUnique();
            e.HasIndex(s => s.College);
        });

        modelBuilder.Entity<LibraryEquipment>(e =>
        {
            e.HasIndex(eq => eq.Name);
            e.HasIndex(eq => eq.GcsId).IsUnique();
        });

        modelBuilder.Entity<CharacterEntity>(e =>
        {
            e.HasIndex(c => c.Name);
        });
    }
}
