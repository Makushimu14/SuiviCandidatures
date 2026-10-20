using SuiviCandidatures.Coeur.Modeles;
using SuiviCandidatures.Coeur.Modeles.Enumerations;
using SuiviCandidatures.Coeur.Services;
using SuiviCandidatures.Coeur.Transferts;
using SuiviCandidatures.Tests.Outils;

namespace SuiviCandidatures.Tests;

public sealed class ServiceStatistiquesEtParametresTests : IDisposable
{
    private static readonly DateTime Aujourdhui = new(2026, 10, 19);

    private readonly BaseDeDonneesTest baseDeDonnees = new();

    public void Dispose() => baseDeDonnees.Dispose();

    // ----- Statistiques -----

    [Fact]
    public async Task La_periode_filtre_les_candidatures_mais_pas_le_graphique_hebdomadaire()
    {
        await baseDeDonnees.CreerCandidatureAsync("Récente", "Acme", new DateTime(2026, 10, 10));
        await baseDeDonnees.CreerCandidatureAsync("Plus ancienne", "Acme", new DateTime(2026, 8, 20));
        ServiceStatistiques service = new(baseDeDonnees);

        Statistiques toutes = await service.CalculerAsync(PeriodeStatistiques.Toutes, Aujourdhui);
        Statistiques recentes = await service.CalculerAsync(PeriodeStatistiques.TrenteJours, Aujourdhui);

        Assert.Equal(2, toutes.NombreCandidatures);
        Assert.Equal(1, recentes.NombreCandidatures);
        Assert.Equal(2, toutes.ParSemaine.Sum(p => p.Valeur));
        Assert.Equal(2, recentes.ParSemaine.Sum(p => p.Valeur));
    }

    [Fact]
    public async Task Les_statistiques_utilisent_l_historique_et_les_sites_des_offres()
    {
        int identifiant = await baseDeDonnees.SemerCandidatureCompleteAsync(); // passe en « en cours d'étude », offre Indeed
        ServiceStatistiques service = new(baseDeDonnees);

        Statistiques statistiques = await service.CalculerAsync(PeriodeStatistiques.Toutes, Aujourdhui);

        Assert.True(identifiant > 0);
        Assert.Equal(1, statistiques.NombreReponses);
        Assert.Equal(1, statistiques.NombreDelaisMesures);
        Assert.Equal("Indeed", Assert.Single(statistiques.ParSite).Libelle);
    }

    // ----- Paramètres -----

    [Fact]
    public async Task Les_parametres_par_defaut_sont_crees_a_la_premiere_lecture()
    {
        Parametres parametres = await new ServiceParametres(baseDeDonnees).ObtenirAsync();

        Assert.Equal(7, parametres.DelaiRelanceParDefautJours);
        Assert.Equal(10, parametres.TaillePage);
        Assert.Equal(ThemeApplication.Systeme, parametres.Theme);
        Assert.True(parametres.NotificationsActivees);
    }

    [Fact]
    public async Task Les_valeurs_hors_bornes_sont_ramenees_dans_les_valeurs_autorisees()
    {
        ServiceParametres service = new(baseDeDonnees);

        await service.EnregistrerAsync(new Parametres { DelaiRelanceParDefautJours = 500, TaillePage = 33 });
        Parametres enregistres = await service.ObtenirAsync();

        Assert.Equal(Parametres.DelaiRelanceMaximumJours, enregistres.DelaiRelanceParDefautJours);
        Assert.Equal(10, enregistres.TaillePage);
    }

    [Fact]
    public async Task Les_parametres_sont_conserves_a_la_creation_comme_a_la_mise_a_jour()
    {
        ServiceParametres service = new(baseDeDonnees);

        // Création directe (aucune ligne en base) avec les notifications coupées.
        await service.EnregistrerAsync(new Parametres { Theme = ThemeApplication.Sombre, NotificationsActivees = false, TaillePage = 50 });
        Parametres apresCreation = await service.ObtenirAsync();
        Assert.Equal(ThemeApplication.Sombre, apresCreation.Theme);
        Assert.False(apresCreation.NotificationsActivees);
        Assert.Equal(50, apresCreation.TaillePage);

        // Mise à jour de la ligne existante.
        await service.EnregistrerAsync(new Parametres { Theme = ThemeApplication.Clair, NotificationsActivees = true, TaillePage = 25 });
        Parametres apresMiseAJour = await service.ObtenirAsync();
        Assert.Equal(ThemeApplication.Clair, apresMiseAJour.Theme);
        Assert.True(apresMiseAJour.NotificationsActivees);
        Assert.Equal(25, apresMiseAJour.TaillePage);
    }
}
