using Microsoft.EntityFrameworkCore;
using SuiviCandidatures.Coeur.Donnees;
using SuiviCandidatures.Coeur.Modeles;
using SuiviCandidatures.Coeur.Services;
using SuiviCandidatures.Coeur.Transferts;
using SuiviCandidatures.Tests.Outils;

namespace SuiviCandidatures.Tests;

public sealed class ServiceAdressesTests : IDisposable
{
    private readonly BaseDeDonneesTest baseDeDonnees = new();
    private readonly ServiceAdresses service;

    public ServiceAdressesTests()
    {
        service = new ServiceAdresses(baseDeDonnees);
    }

    public void Dispose() => baseDeDonnees.Dispose();

    [Fact]
    public async Task Une_adresse_vide_ou_invalide_est_refusee()
    {
        await baseDeDonnees.CreerCandidatureAsync("Poste", "Acme");
        int entreprise = await IdentifiantEntrepriseAsync();

        await Assert.ThrowsAsync<ExceptionValidationMetier>(() =>
            service.EnregistrerAsync(new DonneesAdresseEntreprise { IdentifiantEntreprise = entreprise }));
        await Assert.ThrowsAsync<ExceptionValidationMetier>(() =>
            service.EnregistrerAsync(new DonneesAdresseEntreprise { IdentifiantEntreprise = entreprise, CodePostal = "12" }));
        await Assert.ThrowsAsync<ExceptionValidationMetier>(() =>
            service.EnregistrerAsync(new DonneesAdresseEntreprise { IdentifiantEntreprise = entreprise, Mail = "pas un mail" }));
    }

    [Fact]
    public async Task Supprimer_une_adresse_conserve_les_candidatures_qui_y_etaient_rattachees()
    {
        await baseDeDonnees.SemerCandidatureCompleteAsync();
        int identifiantAdresse;
        await using (ContexteBaseDeDonnees contexte = baseDeDonnees.CreateDbContext())
        {
            identifiantAdresse = await contexte.Adresses.Select(a => a.Identifiant).SingleAsync();
        }

        await service.SupprimerAsync(identifiantAdresse);

        await using (ContexteBaseDeDonnees contexte = baseDeDonnees.CreateDbContext())
        {
            Assert.Equal(0, await contexte.Adresses.CountAsync());
            Candidature candidature = await contexte.Candidatures.SingleAsync();
            Assert.Null(candidature.IdentifiantAdresse);
        }
    }

    private async Task<int> IdentifiantEntrepriseAsync()
    {
        await using ContexteBaseDeDonnees contexte = baseDeDonnees.CreateDbContext();
        return await contexte.Entreprises.Select(e => e.Identifiant).SingleAsync();
    }
}
