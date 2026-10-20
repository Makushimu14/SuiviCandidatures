using Microsoft.EntityFrameworkCore;
using SuiviCandidatures.Coeur.Donnees;
using SuiviCandidatures.Coeur.Modeles;
using SuiviCandidatures.Coeur.Modeles.Enumerations;
using SuiviCandidatures.Coeur.Services;
using SuiviCandidatures.Coeur.Transferts;
using SuiviCandidatures.Tests.Outils;

namespace SuiviCandidatures.Tests;

public sealed class ServiceCandidaturesTests : IDisposable
{
    private readonly BaseDeDonneesTest baseDeDonnees = new();
    private readonly ServiceCandidatures service;

    public ServiceCandidaturesTests()
    {
        service = new ServiceCandidatures(baseDeDonnees);
    }

    public void Dispose() => baseDeDonnees.Dispose();

    [Fact]
    public async Task Creer_une_candidature_cree_l_entreprise_l_adresse_et_l_action_initiale()
    {
        int identifiant = await baseDeDonnees.SemerCandidatureCompleteAsync();

        await using ContexteBaseDeDonnees contexte = baseDeDonnees.CreateDbContext();
        Entreprise entreprise = await contexte.Entreprises.Include(e => e.Adresses).SingleAsync();
        Assert.Equal("Acme", entreprise.Nom);
        Assert.Equal("https://exemple.fr", entreprise.SiteWeb);
        Assert.Equal("Industrie", entreprise.Secteur);
        AdresseEntreprise adresse = Assert.Single(entreprise.Adresses);

        Candidature candidature = await contexte.Candidatures.SingleAsync(c => c.Identifiant == identifiant);
        Assert.Equal(adresse.Identifiant, candidature.IdentifiantAdresse);

        ActionCandidature action = await contexte.Actions.FirstAsync(a => a.Type == TypeAction.CandidatureEnvoyee);
        Assert.Equal(new DateTime(2026, 10, 1), action.Date);
    }

    [Fact]
    public async Task Une_entreprise_existante_est_reutilisee_sans_tenir_compte_de_la_casse_ni_des_accents()
    {
        await baseDeDonnees.CreerCandidatureAsync("Poste A", "Société Générale");
        await baseDeDonnees.CreerCandidatureAsync("Poste B", "societe generale");

        await using ContexteBaseDeDonnees contexte = baseDeDonnees.CreateDbContext();
        Assert.Equal(1, await contexte.Entreprises.CountAsync());
        Assert.Equal(2, await contexte.Candidatures.CountAsync());
    }

    [Fact]
    public async Task Une_adresse_d_une_autre_entreprise_est_refusee()
    {
        await baseDeDonnees.SemerCandidatureCompleteAsync();
        int identifiantAdresse;
        await using (ContexteBaseDeDonnees contexte = baseDeDonnees.CreateDbContext())
        {
            identifiantAdresse = await contexte.Adresses.Select(a => a.Identifiant).SingleAsync();
        }

        DonneesCandidature donnees = new()
        {
            TitreOffre = "Autre poste",
            NomEntreprise = "Globex",
            IdentifiantAdresse = identifiantAdresse
        };

        await Assert.ThrowsAsync<ExceptionValidationMetier>(() => service.EnregistrerAsync(donnees));
    }

    [Fact]
    public async Task Un_titre_ou_un_nom_d_entreprise_vide_est_refuse()
    {
        await Assert.ThrowsAsync<ExceptionValidationMetier>(() =>
            service.EnregistrerAsync(new DonneesCandidature { TitreOffre = "  ", NomEntreprise = "Acme" }));
        await Assert.ThrowsAsync<ExceptionValidationMetier>(() =>
            service.EnregistrerAsync(new DonneesCandidature { TitreOffre = "Poste", NomEntreprise = "" }));
    }

    [Fact]
    public async Task Un_site_web_invalide_est_refuse_pour_une_nouvelle_entreprise()
    {
        DonneesCandidature donnees = new()
        {
            TitreOffre = "Poste",
            NomEntreprise = "Acme",
            NouvelleEntreprise = new DonneesNouvelleEntreprise("ftp://exemple.fr", null, null)
        };

        await Assert.ThrowsAsync<ExceptionValidationMetier>(() => service.EnregistrerAsync(donnees));
    }

    [Fact]
    public async Task Modifier_le_statut_ajoute_une_action_et_ne_rien_changer_n_en_ajoute_pas()
    {
        int identifiant = await baseDeDonnees.CreerCandidatureAsync("Poste", "Acme");

        DonneesCandidature donnees = (await service.ObtenirPourModificationAsync(identifiant))!;
        await service.EnregistrerAsync(donnees);

        await using (ContexteBaseDeDonnees contexte = baseDeDonnees.CreateDbContext())
        {
            Assert.Equal(0, await contexte.Actions.CountAsync(a => a.Type == TypeAction.ChangementStatut));
        }

        donnees.Statut = StatutCandidature.Refusee;
        await service.EnregistrerAsync(donnees);

        await using (ContexteBaseDeDonnees contexte = baseDeDonnees.CreateDbContext())
        {
            ActionCandidature action = await contexte.Actions.SingleAsync(a => a.Type == TypeAction.ChangementStatut);
            Assert.Equal(StatutCandidature.EnAttenteReponse, action.AncienStatut);
            Assert.Equal(StatutCandidature.Refusee, action.NouveauStatut);
        }
    }

    [Fact]
    public async Task La_recherche_porte_sur_le_titre_l_entreprise_la_reference_et_le_lien()
    {
        await baseDeDonnees.SemerCandidatureCompleteAsync();
        await baseDeDonnees.CreerCandidatureAsync("Comptable", "Globex");

        Assert.Equal(["Développeur"], await Titres("développeur"));
        Assert.Equal(["Développeur"], await Titres("acme"));
        Assert.Equal(["Comptable"], await Titres("globex"));
        Assert.Equal(["Développeur"], await Titres("REF-4521"));
        Assert.Equal(["Développeur"], await Titres("4521"));
        Assert.Equal(["Développeur"], await Titres("https://exemple.fr/offres/4521"));
        Assert.Empty(await Titres("introuvable"));
    }

    [Theory]
    [InlineData("%")]
    [InlineData("_")]
    [InlineData("\\")]
    public async Task Les_caracteres_speciaux_de_la_recherche_sont_traites_comme_du_texte(string saisie)
    {
        await baseDeDonnees.SemerCandidatureCompleteAsync();

        Assert.Empty(await Titres(saisie));
    }

    [Fact]
    public async Task Le_filtre_par_statut_ne_garde_que_les_candidatures_de_ce_statut()
    {
        await baseDeDonnees.CreerCandidatureAsync("A", "Acme", statut: StatutCandidature.Refusee);
        await baseDeDonnees.CreerCandidatureAsync("B", "Acme", statut: StatutCandidature.EnAttenteReponse);

        ResultatPagine<ElementListeCandidature> resultat = await service.RechercherAsync(
            new CritereRechercheCandidatures { Statut = StatutCandidature.Refusee });

        Assert.Equal(["A"], resultat.Elements.Select(e => e.TitreOffre));
    }

    [Fact]
    public async Task La_pagination_decoupe_les_resultats_et_ramene_une_page_inexistante_a_la_derniere()
    {
        for (int numero = 1; numero <= 5; numero++)
        {
            await baseDeDonnees.CreerCandidatureAsync($"Poste {numero}", "Acme", new DateTime(2026, 10, numero));
        }

        ResultatPagine<ElementListeCandidature> premierePage = await service.RechercherAsync(
            new CritereRechercheCandidatures { TaillePage = 2, NumeroPage = 1 });
        ResultatPagine<ElementListeCandidature> dernierePage = await service.RechercherAsync(
            new CritereRechercheCandidatures { TaillePage = 2, NumeroPage = 99 });

        Assert.Equal(5, premierePage.NombreTotal);
        Assert.Equal(3, premierePage.NombrePages);
        Assert.Equal(["Poste 5", "Poste 4"], premierePage.Elements.Select(e => e.TitreOffre));
        Assert.Equal(3, dernierePage.NumeroPage);
        Assert.Equal(["Poste 1"], dernierePage.Elements.Select(e => e.TitreOffre));
    }

    [Fact]
    public async Task Le_tri_par_statut_suit_l_ordre_des_statuts_et_non_l_ordre_alphabetique()
    {
        await baseDeDonnees.CreerCandidatureAsync("Refusée", "Acme", statut: StatutCandidature.Refusee);
        await baseDeDonnees.CreerCandidatureAsync("CV vu", "Acme", statut: StatutCandidature.CurriculumVitaeVu);
        await baseDeDonnees.CreerCandidatureAsync("En attente", "Acme", statut: StatutCandidature.EnAttenteReponse);
        await baseDeDonnees.CreerCandidatureAsync("Offre", "Acme", statut: StatutCandidature.OffreRecue);

        ResultatPagine<ElementListeCandidature> croissant = await service.RechercherAsync(
            new CritereRechercheCandidatures { Tri = CritereTriCandidatures.Statut, Decroissant = false });
        ResultatPagine<ElementListeCandidature> decroissant = await service.RechercherAsync(
            new CritereRechercheCandidatures { Tri = CritereTriCandidatures.Statut, Decroissant = true });

        Assert.Equal(["CV vu", "En attente", "Offre", "Refusée"], croissant.Elements.Select(e => e.TitreOffre));
        Assert.Equal(["Refusée", "Offre", "En attente", "CV vu"], decroissant.Elements.Select(e => e.TitreOffre));
    }

    [Fact]
    public async Task Le_comptage_par_statut_ignore_les_statuts_absents()
    {
        await baseDeDonnees.CreerCandidatureAsync("A", "Acme", statut: StatutCandidature.Refusee);
        await baseDeDonnees.CreerCandidatureAsync("B", "Acme", statut: StatutCandidature.Refusee);
        await baseDeDonnees.CreerCandidatureAsync("C", "Acme", statut: StatutCandidature.OffreRecue);

        IReadOnlyDictionary<StatutCandidature, int> comptes = await service.CompterParStatutAsync();

        Assert.Equal(2, comptes[StatutCandidature.Refusee]);
        Assert.Equal(1, comptes[StatutCandidature.OffreRecue]);
        Assert.False(comptes.ContainsKey(StatutCandidature.Retiree));
    }

    private async Task<List<string>> Titres(string recherche)
    {
        ResultatPagine<ElementListeCandidature> resultat = await service.RechercherAsync(
            new CritereRechercheCandidatures { TexteRecherche = recherche });
        return resultat.Elements.Select(e => e.TitreOffre).ToList();
    }
}
