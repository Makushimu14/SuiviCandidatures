using Microsoft.EntityFrameworkCore;

namespace SuiviCandidatures.Coeur.Donnees;

public static class InitialiseurBaseDeDonnees
{
    /// <summary>Applique les migrations en attente. Appelée au démarrage, avant l'affichage de la première page.</summary>
    public static void Initialiser(IDbContextFactory<ContexteBaseDeDonnees> fabrique)
    {
        using ContexteBaseDeDonnees contexte = fabrique.CreateDbContext();

        if (!contexte.Database.GetMigrations().Any())
        {
            throw new InvalidOperationException(
                "Aucune migration EF Core n'a été trouvée. Créez la migration initiale : " +
                "dotnet ef migrations add Initiale --project SuiviCandidatures.Coeur " +
                "--startup-project SuiviCandidatures.Coeur --output-dir Donnees/Migrations");
        }

        contexte.Database.Migrate();
    }
}
