using Microsoft.EntityFrameworkCore;
using SuiviCandidatures.Coeur.Donnees;
using SuiviCandidatures.Coeur.Modeles;
using SuiviCandidatures.Coeur.Services;
using SuiviCandidatures.Coeur.Transferts;
using SuiviCandidatures.Tests.Outils;

namespace SuiviCandidatures.Tests;

public sealed class ServiceOffresEtContactsTests : IDisposable
{
    private readonly BaseDeDonneesTest baseDeDonnees = new();
    private readonly ServiceOffres offres;
    private readonly ServiceContacts contacts;

    public ServiceOffresEtContactsTests()
    {
        offres = new ServiceOffres(baseDeDonnees);
        contacts = new ServiceContacts(baseDeDonnees);
    }

    public void Dispose() => baseDeDonnees.Dispose();

    // ----- Offres -----

    [Fact]
    public async Task Une_offre_vide_est_refusee()
    {
        int identifiant = await baseDeDonnees.CreerCandidatureAsync("Poste", "Acme");

        await Assert.ThrowsAsync<ExceptionValidationMetier>(() => offres.EnregistrerAsync(
            new DonneesOffre { IdentifiantCandidature = identifiant, NomSite = " ", Reference = "", Lien = null }));
    }

    [Theory]
    [InlineData("ftp://exemple.fr/offre")]
    [InlineData("pas un lien")]
    public async Task Un_lien_d_offre_invalide_est_refuse(string lien)
    {
        int identifiant = await baseDeDonnees.CreerCandidatureAsync("Poste", "Acme");

        await Assert.ThrowsAsync<ExceptionValidationMetier>(() => offres.EnregistrerAsync(
            new DonneesOffre { IdentifiantCandidature = identifiant, Lien = lien }));
    }

    [Fact]
    public async Task Un_lien_sans_schema_est_enregistre_en_https()
    {
        int identifiant = await baseDeDonnees.CreerCandidatureAsync("Poste", "Acme");

        await offres.EnregistrerAsync(new DonneesOffre { IdentifiantCandidature = identifiant, Lien = "exemple.fr/offre/1" });

        await using ContexteBaseDeDonnees contexte = baseDeDonnees.CreateDbContext();
        Assert.Equal("https://exemple.fr/offre/1", (await contexte.Offres.SingleAsync()).Lien);
    }

    [Fact]
    public async Task Une_offre_peut_etre_modifiee_puis_supprimee()
    {
        int identifiant = await baseDeDonnees.CreerCandidatureAsync("Poste", "Acme");
        await offres.EnregistrerAsync(new DonneesOffre { IdentifiantCandidature = identifiant, NomSite = "Indeed", Reference = "A1" });

        int identifiantOffre;
        await using (ContexteBaseDeDonnees contexte = baseDeDonnees.CreateDbContext())
        {
            identifiantOffre = (await contexte.Offres.SingleAsync()).Identifiant;
        }

        await offres.EnregistrerAsync(new DonneesOffre
        {
            Identifiant = identifiantOffre,
            IdentifiantCandidature = identifiant,
            NomSite = "LinkedIn",
            Reference = "B2"
        });

        await using (ContexteBaseDeDonnees contexte = baseDeDonnees.CreateDbContext())
        {
            OffreCandidature offre = await contexte.Offres.SingleAsync();
            Assert.Equal("LinkedIn", offre.NomSite);
            Assert.Equal("B2", offre.Reference);
        }

        await offres.SupprimerAsync(identifiantOffre);

        await using (ContexteBaseDeDonnees contexte = baseDeDonnees.CreateDbContext())
        {
            Assert.Equal(0, await contexte.Offres.CountAsync());
        }
    }

    // ----- Contacts -----

    [Fact]
    public async Task Un_contact_cree_depuis_une_candidature_est_lie_a_la_candidature_et_a_son_entreprise()
    {
        int identifiant = await baseDeDonnees.CreerCandidatureAsync("Poste", "Acme");

        await contacts.EnregistrerAsync(new DonneesContact { IdentifiantCandidature = identifiant, Nom = "Martin" });

        await using ContexteBaseDeDonnees contexte = baseDeDonnees.CreateDbContext();
        Contact contact = await contexte.Contacts.Include(c => c.Candidatures).SingleAsync();
        Assert.Equal(identifiant, Assert.Single(contact.Candidatures).Identifiant);
        Assert.Equal((await contexte.Entreprises.SingleAsync()).Identifiant, contact.IdentifiantEntreprise);
    }

    [Fact]
    public async Task Un_contact_invalide_est_refuse()
    {
        int identifiant = await baseDeDonnees.CreerCandidatureAsync("Poste", "Acme");

        await Assert.ThrowsAsync<ExceptionValidationMetier>(() =>
            contacts.EnregistrerAsync(new DonneesContact { IdentifiantCandidature = identifiant, Nom = " " }));
        await Assert.ThrowsAsync<ExceptionValidationMetier>(() =>
            contacts.EnregistrerAsync(new DonneesContact { IdentifiantCandidature = identifiant, Nom = "Martin", Mail = "pas un mail" }));
        await Assert.ThrowsAsync<ExceptionValidationMetier>(() =>
            contacts.EnregistrerAsync(new DonneesContact { IdentifiantCandidature = identifiant, Nom = "Martin", Telephone = "12" }));
    }

    [Fact]
    public async Task Un_contact_peut_etre_lie_a_plusieurs_candidatures_de_la_meme_entreprise()
    {
        int premiere = await baseDeDonnees.CreerCandidatureAsync("Poste A", "Acme");
        int seconde = await baseDeDonnees.CreerCandidatureAsync("Poste B", "Acme");
        await contacts.EnregistrerAsync(new DonneesContact { IdentifiantCandidature = premiere, Nom = "Martin" });

        IReadOnlyList<ElementContact> disponibles = await contacts.ListerDisponiblesAsync(seconde);
        ElementContact contact = Assert.Single(disponibles);
        Assert.Empty(await contacts.ListerDisponiblesAsync(premiere));

        await contacts.LierAsync(seconde, contact.Identifiant);

        Assert.Empty(await contacts.ListerDisponiblesAsync(seconde));
        await using ContexteBaseDeDonnees contexte = baseDeDonnees.CreateDbContext();
        Contact enBase = await contexte.Contacts.Include(c => c.Candidatures).SingleAsync();
        Assert.Equal(2, enBase.Candidatures.Count);
    }

    [Fact]
    public async Task Un_contact_d_une_autre_entreprise_ne_peut_pas_etre_lie()
    {
        int chezAcme = await baseDeDonnees.CreerCandidatureAsync("Poste A", "Acme");
        int chezGlobex = await baseDeDonnees.CreerCandidatureAsync("Poste B", "Globex");
        await contacts.EnregistrerAsync(new DonneesContact { IdentifiantCandidature = chezAcme, Nom = "Martin" });
        int identifiantContact;
        await using (ContexteBaseDeDonnees contexte = baseDeDonnees.CreateDbContext())
        {
            identifiantContact = await contexte.Contacts.Select(c => c.Identifiant).SingleAsync();
        }

        await Assert.ThrowsAsync<ExceptionValidationMetier>(() => contacts.LierAsync(chezGlobex, identifiantContact));
    }

    [Fact]
    public async Task Delier_un_contact_le_conserve_pour_l_entreprise()
    {
        int identifiant = await baseDeDonnees.CreerCandidatureAsync("Poste", "Acme");
        await contacts.EnregistrerAsync(new DonneesContact { IdentifiantCandidature = identifiant, Nom = "Martin" });
        int identifiantContact;
        await using (ContexteBaseDeDonnees contexte = baseDeDonnees.CreateDbContext())
        {
            identifiantContact = await contexte.Contacts.Select(c => c.Identifiant).SingleAsync();
        }

        await contacts.DelierAsync(identifiant, identifiantContact);

        await using (ContexteBaseDeDonnees contexte = baseDeDonnees.CreateDbContext())
        {
            Contact contact = await contexte.Contacts.Include(c => c.Candidatures).SingleAsync();
            Assert.Empty(contact.Candidatures);
        }

        Assert.Single(await contacts.ListerDisponiblesAsync(identifiant));
    }

    [Fact]
    public async Task Un_contact_saisi_depuis_l_entreprise_n_est_lie_a_aucune_candidature_et_peut_etre_supprime()
    {
        await baseDeDonnees.CreerCandidatureAsync("Poste", "Acme");
        int identifiantEntreprise;
        await using (ContexteBaseDeDonnees contexte = baseDeDonnees.CreateDbContext())
        {
            identifiantEntreprise = await contexte.Entreprises.Select(e => e.Identifiant).SingleAsync();
        }

        await contacts.EnregistrerPourEntrepriseAsync(
            new DonneesContactEntreprise { IdentifiantEntreprise = identifiantEntreprise, Nom = "Durand" });

        int identifiantContact;
        await using (ContexteBaseDeDonnees contexte = baseDeDonnees.CreateDbContext())
        {
            Contact contact = await contexte.Contacts.Include(c => c.Candidatures).SingleAsync();
            identifiantContact = contact.Identifiant;
            Assert.Empty(contact.Candidatures);
        }

        await contacts.SupprimerAsync(identifiantContact);

        await using (ContexteBaseDeDonnees contexte = baseDeDonnees.CreateDbContext())
        {
            Assert.Equal(0, await contexte.Contacts.CountAsync());
        }
    }
}
