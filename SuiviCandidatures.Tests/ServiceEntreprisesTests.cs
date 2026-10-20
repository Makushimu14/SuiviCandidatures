using Microsoft.EntityFrameworkCore;
using SuiviCandidatures.Coeur.Donnees;
using SuiviCandidatures.Coeur.Services;
using SuiviCandidatures.Coeur.Transferts;
using SuiviCandidatures.Tests.Outils;

namespace SuiviCandidatures.Tests;

public sealed class ServiceEntreprisesTests : IDisposable
{
    private readonly BaseDeDonneesTest baseDeDonnees = new();
    private readonly ServiceEntreprises service;

    public ServiceEntreprisesTests()
    {
        service = new ServiceEntreprises(baseDeDonnees);
    }

    public void Dispose() => baseDeDonnees.Dispose();

    [Fact]
    public async Task Un_nom_deja_utilise_est_refuse_sans_tenir_compte_de_la_casse_ni_des_accents()
    {
        await service.EnregistrerAsync(new DonneesEntreprise { Nom = "Société Générale" });

        await Assert.ThrowsAsync<ExceptionValidationMetier>(() =>
            service.EnregistrerAsync(new DonneesEntreprise { Nom = "societe generale" }));
    }

    [Fact]
    public async Task Une_entreprise_peut_etre_enregistree_de_nouveau_sous_son_propre_nom()
    {
        int identifiant = await service.EnregistrerAsync(new DonneesEntreprise { Nom = "Acme", Secteur = "Industrie" });

        await service.EnregistrerAsync(new DonneesEntreprise { Identifiant = identifiant, Nom = "ACME", Secteur = "Services" });

        DonneesEntreprise donnees = (await service.ObtenirPourModificationAsync(identifiant))!;
        Assert.Equal("ACME", donnees.Nom);
        Assert.Equal("Services", donnees.Secteur);
    }

    [Fact]
    public async Task Le_nom_est_obligatoire_et_le_site_web_doit_etre_valide()
    {
        await Assert.ThrowsAsync<ExceptionValidationMetier>(() =>
            service.EnregistrerAsync(new DonneesEntreprise { Nom = "  " }));
        await Assert.ThrowsAsync<ExceptionValidationMetier>(() =>
            service.EnregistrerAsync(new DonneesEntreprise { Nom = "Acme", SiteWeb = "ftp://exemple.fr" }));
    }

    [Fact]
    public async Task Le_site_web_sans_schema_est_enregistre_en_https()
    {
        int identifiant = await service.EnregistrerAsync(new DonneesEntreprise { Nom = "Acme", SiteWeb = "exemple.fr" });

        Assert.Equal("https://exemple.fr", (await service.ObtenirPourModificationAsync(identifiant))!.SiteWeb);
    }

    [Fact]
    public async Task La_recherche_porte_sur_le_nom_le_secteur_et_la_ville()
    {
        await baseDeDonnees.SemerCandidatureCompleteAsync(); // Acme, Industrie, Paris
        await service.EnregistrerAsync(new DonneesEntreprise { Nom = "Globex", Secteur = "Banque" });

        Assert.Equal(["Acme"], await Noms("acme"));
        Assert.Equal(["Acme"], await Noms("industrie"));
        Assert.Equal(["Acme"], await Noms("paris"));
        Assert.Equal(["Globex"], await Noms("banque"));
        Assert.Empty(await Noms("introuvable"));
    }

    [Fact]
    public async Task Le_tri_par_nombre_de_candidatures_et_la_pagination_fonctionnent()
    {
        await baseDeDonnees.CreerCandidatureAsync("A1", "Alpha");
        await baseDeDonnees.CreerCandidatureAsync("B1", "Beta");
        await baseDeDonnees.CreerCandidatureAsync("B2", "Beta");
        await baseDeDonnees.CreerCandidatureAsync("C1", "Gamma");
        await baseDeDonnees.CreerCandidatureAsync("C2", "Gamma");
        await baseDeDonnees.CreerCandidatureAsync("C3", "Gamma");

        ResultatPagine<ElementListeEntreprise> resultat = await service.RechercherAsync(new CritereRechercheEntreprises
        {
            Tri = CritereTriEntreprises.NombreCandidatures,
            Decroissant = true,
            TaillePage = 2
        });

        Assert.Equal(3, resultat.NombreTotal);
        Assert.Equal(2, resultat.NombrePages);
        Assert.Equal(["Gamma", "Beta"], resultat.Elements.Select(e => e.Nom));
        Assert.Equal([3, 2], resultat.Elements.Select(e => e.NombreCandidatures));
    }

    [Fact]
    public async Task Supprimer_une_entreprise_supprime_tout_ce_qui_en_depend()
    {
        await baseDeDonnees.SemerCandidatureCompleteAsync();
        int identifiant;
        await using (ContexteBaseDeDonnees contexte = baseDeDonnees.CreateDbContext())
        {
            identifiant = await contexte.Entreprises.Select(e => e.Identifiant).SingleAsync();
        }

        await service.SupprimerAsync(identifiant);

        await using (ContexteBaseDeDonnees contexte = baseDeDonnees.CreateDbContext())
        {
            Assert.Equal(0, await contexte.Entreprises.CountAsync());
            Assert.Equal(0, await contexte.Adresses.CountAsync());
            Assert.Equal(0, await contexte.Contacts.CountAsync());
            Assert.Equal(0, await contexte.Candidatures.CountAsync());
            Assert.Equal(0, await contexte.Actions.CountAsync());
            Assert.Equal(0, await contexte.Offres.CountAsync());
        }
    }

    [Fact]
    public async Task Le_detail_donne_les_adresses_les_contacts_et_les_candidatures()
    {
        await baseDeDonnees.SemerCandidatureCompleteAsync();
        int identifiant;
        await using (ContexteBaseDeDonnees contexte = baseDeDonnees.CreateDbContext())
        {
            identifiant = await contexte.Entreprises.Select(e => e.Identifiant).SingleAsync();
        }

        DetailEntreprise detail = (await service.ObtenirDetailAsync(identifiant))!;

        Assert.Single(detail.Adresses);
        ElementContactEntreprise contact = Assert.Single(detail.Contacts);
        Assert.Equal(1, contact.NombreCandidatures);
        Assert.Single(detail.Candidatures);
    }

    private async Task<List<string>> Noms(string recherche)
    {
        ResultatPagine<ElementListeEntreprise> resultat = await service.RechercherAsync(
            new CritereRechercheEntreprises { TexteRecherche = recherche });
        return resultat.Elements.Select(e => e.Nom).ToList();
    }
}
