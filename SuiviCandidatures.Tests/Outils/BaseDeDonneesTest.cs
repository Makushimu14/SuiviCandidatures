using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SuiviCandidatures.Coeur.Donnees;
using SuiviCandidatures.Coeur.Modeles.Enumerations;
using SuiviCandidatures.Coeur.Services;
using SuiviCandidatures.Coeur.Transferts;

namespace SuiviCandidatures.Tests.Outils;

/// <summary>
/// Base SQLite en mémoire, avec les clés étrangères actives (comme dans l'application), fournie aux services
/// comme fabrique de contextes. Chaque instance est une base vide indépendante.
/// </summary>
public sealed class BaseDeDonneesTest : IDbContextFactory<ContexteBaseDeDonnees>, IDisposable
{
    private readonly SqliteConnection connexion;
    private readonly DbContextOptions<ContexteBaseDeDonnees> options;

    public BaseDeDonneesTest()
    {
        connexion = new SqliteConnection("Data Source=:memory:");
        connexion.Open();

        using (SqliteCommand commande = connexion.CreateCommand())
        {
            commande.CommandText = "PRAGMA foreign_keys = ON;";
            commande.ExecuteNonQuery();
        }

        options = new DbContextOptionsBuilder<ContexteBaseDeDonnees>().UseSqlite(connexion).Options;

        using ContexteBaseDeDonnees contexte = new(options);
        contexte.Database.EnsureCreated();
    }

    public ContexteBaseDeDonnees CreateDbContext() => new(options);

    public void Dispose() => connexion.Dispose();

    /// <summary>Crée une candidature (et son entreprise si besoin) par le service, comme le ferait le formulaire.</summary>
    public async Task<int> CreerCandidatureAsync(
        string titre,
        string entreprise,
        DateTime? date = null,
        StatutCandidature statut = StatutCandidature.EnAttenteReponse)
    {
        ServiceCandidatures service = new(this);
        return await service.EnregistrerAsync(new DonneesCandidature
        {
            TitreOffre = titre,
            NomEntreprise = entreprise,
            DateCandidature = date ?? new DateTime(2026, 10, 1),
            Statut = statut
        });
    }

    /// <summary>
    /// Candidature complète : entreprise avec site et adresse, offre, contact lié, changement de statut et relance.
    /// Retourne l'identifiant de la candidature.
    /// </summary>
    public async Task<int> SemerCandidatureCompleteAsync()
    {
        ServiceCandidatures candidatures = new(this);
        int identifiant = await candidatures.EnregistrerAsync(new DonneesCandidature
        {
            TitreOffre = "Développeur",
            NomEntreprise = "Acme",
            DateCandidature = new DateTime(2026, 10, 1),
            NouvelleEntreprise = new DonneesNouvelleEntreprise("exemple.fr", "Industrie", "Notes"),
            NouvelleAdresse = new DonneesAdresse("1 rue de la Paix", "75002", "Paris", "contact@acme.fr", "01 23 45 67 89")
        });

        await new ServiceOffres(this).EnregistrerAsync(new DonneesOffre
        {
            IdentifiantCandidature = identifiant,
            NomSite = "Indeed",
            Reference = "REF-4521",
            Lien = "https://exemple.fr/offres/4521"
        });

        await new ServiceContacts(this).EnregistrerAsync(new DonneesContact
        {
            IdentifiantCandidature = identifiant,
            Nom = "Martin",
            Prenom = "Claire",
            Role = "Recruteuse",
            Mail = "c.martin@acme.fr"
        });

        ServiceSuiviCandidature suivi = new(this);
        await suivi.ChangerStatutAsync(identifiant, StatutCandidature.EnCoursEtude);
        await suivi.EnregistrerActionAsync(new DonneesAction
        {
            IdentifiantCandidature = identifiant,
            Type = TypeAction.RelanceParMail,
            Date = new DateTime(2026, 10, 8, 9, 30, 0),
            Note = "Relance"
        });

        return identifiant;
    }
}
