using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GurpsWizard.Data;

/// <summary>
/// Factory usada pelo dotnet-ef durante migrations (design time).
/// Cria o contexto com um banco SQLite temporário local.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=gurpswizard-design.db")
            .Options;
        return new AppDbContext(opts);
    }
}
