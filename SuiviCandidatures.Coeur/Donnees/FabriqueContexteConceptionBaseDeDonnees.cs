using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SuiviCandidatures.Coeur.Donnees;

/// <summary>Utilisée uniquement par les outils EF Core (création des migrations). Jamais par l'application.</summary>
public class FabriqueContexteConceptionBaseDeDonnees : IDesignTimeDbContextFactory<ContexteBaseDeDonnees>
{
    public ContexteBaseDeDonnees CreateDbContext(string[] arguments)
    {
        DbContextOptions<ContexteBaseDeDonnees> options = new DbContextOptionsBuilder<ContexteBaseDeDonnees>()
            .UseSqlite("Data Source=conception.sqlite")
            .Options;
        return new ContexteBaseDeDonnees(options);
    }
}
