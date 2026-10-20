using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using SuiviCandidatures.Coeur.Donnees;
using SuiviCandidatures.Coeur.Modeles;
using SuiviCandidatures.Coeur.Modeles.Enumerations;
using SuiviCandidatures.Coeur.Sauvegarde;
using SuiviCandidatures.Coeur.Services;
using SuiviCandidatures.Tests.Outils;

namespace SuiviCandidatures.Tests;

public sealed class ServiceSauvegardeTests : IDisposable
{
    private readonly BaseDeDonneesTest source = new();
    private readonly BaseDeDonneesTest destination = new();

    public void Dispose()
    {
        source.Dispose();
        destination.Dispose();
    }

    private static MemoryStream EnJson(ContenuSauvegarde contenu)
    {
        JsonSerializerOptions options = new() { Converters = { new JsonStringEnumConverter() } };
        return new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(contenu, options));
    }

    private static async Task<MemoryStream> ExporterAsync(BaseDeDonneesTest baseDeDonnees)
    {
        MemoryStream flux = new();
        await new ServiceSauvegarde(baseDeDonnees).ExporterAsync(flux);
        flux.Position = 0;
        return flux;
    }

    [Fact]
    public async Task Un_export_suivi_d_un_import_restitue_toutes_les_donnees_et_leurs_liens()
    {
        await source.SemerCandidatureCompleteAsync();
        using MemoryStream export = await ExporterAsync(source);

        ResumeSauvegarde resume = await new ServiceSauvegarde(destination).ImporterAsync(export);

        Assert.Equal(new ResumeSauvegarde(1, 1, 1), resume);

        await using ContexteBaseDeDonnees contexte = destination.CreateDbContext();
        Candidature candidature = await contexte.Candidatures
            .Include(c => c.Entreprise)
            .Include(c => c.Adresse)
            .Include(c => c.Offres)
            .Include(c => c.Actions)
            .Include(c => c.Contacts)
            .SingleAsync();

        Assert.Equal("Développeur", candidature.TitreOffre);
        Assert.Equal(StatutCandidature.EnCoursEtude, candidature.Statut);
        Assert.Equal(new DateTime(2026, 10, 1), candidature.DateCandidature);
        Assert.Equal("Acme", candidature.Entreprise.Nom);
        Assert.Equal("https://exemple.fr", candidature.Entreprise.SiteWeb);
        Assert.Equal("Paris", candidature.Adresse!.Ville);
        Assert.Equal("REF-4521", Assert.Single(candidature.Offres).Reference);
        Assert.Equal("Martin", Assert.Single(candidature.Contacts).Nom);
        Assert.Equal(3, candidature.Actions.Count);

        ActionCandidature changement = candidature.Actions.Single(a => a.Type == TypeAction.ChangementStatut);
        Assert.Equal(StatutCandidature.EnAttenteReponse, changement.AncienStatut);
        Assert.Equal(StatutCandidature.EnCoursEtude, changement.NouveauStatut);
    }

    [Fact]
    public async Task L_import_remplace_les_donnees_existantes()
    {
        await source.SemerCandidatureCompleteAsync();
        await destination.CreerCandidatureAsync("À remplacer", "Globex");
        using MemoryStream export = await ExporterAsync(source);

        await new ServiceSauvegarde(destination).ImporterAsync(export);

        await using ContexteBaseDeDonnees contexte = destination.CreateDbContext();
        Assert.Equal(["Acme"], await contexte.Entreprises.Select(e => e.Nom).ToListAsync());
        Assert.Equal(["Développeur"], await contexte.Candidatures.Select(c => c.TitreOffre).ToListAsync());
    }

    [Fact]
    public async Task Les_parametres_ne_sont_pas_touches_par_l_import()
    {
        await source.SemerCandidatureCompleteAsync();
        await new ServiceParametres(destination).EnregistrerAsync(new Parametres { Theme = ThemeApplication.Sombre });
        using MemoryStream export = await ExporterAsync(source);

        await new ServiceSauvegarde(destination).ImporterAsync(export);

        Assert.Equal(ThemeApplication.Sombre, (await new ServiceParametres(destination).ObtenirAsync()).Theme);
    }

    [Fact]
    public async Task L_analyse_resume_le_fichier_sans_modifier_la_base()
    {
        await source.SemerCandidatureCompleteAsync();
        await destination.CreerCandidatureAsync("Existante", "Globex");
        using MemoryStream export = await ExporterAsync(source);

        ResumeSauvegarde resume = await new ServiceSauvegarde(destination).AnalyserAsync(export);

        Assert.Equal(new ResumeSauvegarde(1, 1, 1), resume);
        await using ContexteBaseDeDonnees contexte = destination.CreateDbContext();
        Assert.Equal(["Existante"], await contexte.Candidatures.Select(c => c.TitreOffre).ToListAsync());
    }

    [Fact]
    public async Task Un_fichier_qui_n_est_pas_du_json_valide_est_refuse_sans_modifier_les_donnees()
    {
        await destination.CreerCandidatureAsync("Existante", "Globex");
        using MemoryStream invalide = new("ceci n'est pas du json"u8.ToArray());

        await Assert.ThrowsAsync<ExceptionValidationMetier>(() => new ServiceSauvegarde(destination).ImporterAsync(invalide));

        await using ContexteBaseDeDonnees contexte = destination.CreateDbContext();
        Assert.Equal(1, await contexte.Candidatures.CountAsync());
    }

    [Fact]
    public async Task Une_version_de_format_inconnue_est_refusee()
    {
        using MemoryStream flux = EnJson(new ContenuSauvegarde { Version = 99 });

        await Assert.ThrowsAsync<ExceptionValidationMetier>(() => new ServiceSauvegarde(destination).AnalyserAsync(flux));
    }

    [Fact]
    public async Task Une_candidature_qui_reference_une_entreprise_absente_est_refusee_sans_modifier_les_donnees()
    {
        await destination.CreerCandidatureAsync("Existante", "Globex");
        ContenuSauvegarde contenu = new()
        {
            Candidatures =
            [
                new CandidatureSauvegarde { Identifiant = 1, IdentifiantEntreprise = 42, TitreOffre = "Orpheline" }
            ]
        };
        using MemoryStream flux = EnJson(contenu);

        await Assert.ThrowsAsync<ExceptionValidationMetier>(() => new ServiceSauvegarde(destination).ImporterAsync(flux));

        await using ContexteBaseDeDonnees contexte = destination.CreateDbContext();
        Assert.Equal(["Existante"], await contexte.Candidatures.Select(c => c.TitreOffre).ToListAsync());
    }

    [Fact]
    public async Task Un_contact_d_une_autre_entreprise_est_refuse()
    {
        ContenuSauvegarde contenu = new()
        {
            Entreprises =
            [
                new EntrepriseSauvegarde { Identifiant = 1, Nom = "Acme" },
                new EntrepriseSauvegarde
                {
                    Identifiant = 2,
                    Nom = "Globex",
                    Contacts = [new ContactSauvegarde { Identifiant = 7, Nom = "Durand" }]
                }
            ],
            Candidatures =
            [
                new CandidatureSauvegarde
                {
                    Identifiant = 1,
                    IdentifiantEntreprise = 1,
                    TitreOffre = "Poste",
                    IdentifiantsContacts = [7]
                }
            ]
        };
        using MemoryStream flux = EnJson(contenu);

        await Assert.ThrowsAsync<ExceptionValidationMetier>(() => new ServiceSauvegarde(destination).AnalyserAsync(flux));
    }

    [Fact]
    public async Task Des_identifiants_en_double_sont_refuses()
    {
        ContenuSauvegarde contenu = new()
        {
            Entreprises =
            [
                new EntrepriseSauvegarde { Identifiant = 1, Nom = "Acme" },
                new EntrepriseSauvegarde { Identifiant = 1, Nom = "Globex" }
            ]
        };
        using MemoryStream flux = EnJson(contenu);

        await Assert.ThrowsAsync<ExceptionValidationMetier>(() => new ServiceSauvegarde(destination).AnalyserAsync(flux));
    }
}
