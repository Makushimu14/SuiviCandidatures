using SuiviCandidatures.Coeur.Modeles;
using SuiviCandidatures.Coeur.Modeles.Enumerations;
using SuiviCandidatures.Coeur.Services;
using SuiviCandidatures.Coeur.Transferts;
using SuiviCandidatures.Tests.Outils;

namespace SuiviCandidatures.Tests;

public sealed class ServiceRelancesEtRappelsTests : IDisposable
{
    private static readonly DateTime Aujourdhui = new(2026, 10, 19);

    private readonly BaseDeDonneesTest baseDeDonnees = new();

    public void Dispose() => baseDeDonnees.Dispose();

    // ----- Relances calculées depuis la base -----

    [Fact]
    public async Task Les_relances_sont_triees_et_excluent_les_candidatures_terminees_ou_desactivees()
    {
        ServiceRelances relances = new(baseDeDonnees, new ServiceParametres(baseDeDonnees));
        int ancienne = await baseDeDonnees.CreerCandidatureAsync("Ancienne", "Acme", new DateTime(2026, 10, 1));
        await baseDeDonnees.CreerCandidatureAsync("Récente", "Acme", new DateTime(2026, 10, 15));
        await baseDeDonnees.CreerCandidatureAsync("Refusée", "Acme", new DateTime(2026, 9, 1), StatutCandidature.Refusee);
        int desactivee = await baseDeDonnees.CreerCandidatureAsync("Désactivée", "Acme", new DateTime(2026, 9, 1));
        await new ServiceSuiviCandidature(baseDeDonnees).EnregistrerParametresRelanceAsync(desactivee, null, true);

        IReadOnlyList<ElementRelance> liste = await relances.ListerAsync(Aujourdhui);

        Assert.Equal(["Ancienne", "Récente"], liste.Select(r => r.TitreOffre));
        Assert.Equal(ancienne, liste[0].IdentifiantCandidature);
        Assert.Equal(-11, liste[0].JoursRestants);
        Assert.True(liste[0].EstDue);
        Assert.Equal(3, liste[1].JoursRestants);
        Assert.False(liste[1].EstDue);
    }

    [Fact]
    public async Task Le_delai_par_defaut_des_parametres_est_utilise()
    {
        ServiceParametres parametres = new(baseDeDonnees);
        await parametres.EnregistrerAsync(new Parametres { DelaiRelanceParDefautJours = 30 });
        await baseDeDonnees.CreerCandidatureAsync("Poste", "Acme", new DateTime(2026, 10, 1));

        IReadOnlyList<ElementRelance> liste = await new ServiceRelances(baseDeDonnees, parametres).ListerAsync(Aujourdhui);

        Assert.Equal(new DateTime(2026, 10, 31), Assert.Single(liste).DateRelance);
    }

    // ----- Rappels -----

    private static ElementRelance Relance(int identifiant, string titre, int joursRestants) =>
        new(identifiant, titre, "Acme", Aujourdhui.AddDays(joursRestants), joursRestants);

    private ServiceRappels CreerRappels(RelancesFactices relances, NotificationsFactices notifications) =>
        new(relances, notifications, new ServiceParametres(baseDeDonnees));

    [Fact]
    public async Task Les_relances_dues_sont_signalees_une_seule_fois()
    {
        RelancesFactices relances = new();
        NotificationsFactices notifications = new();
        relances.Relances.Add(Relance(1, "Poste A", -2));
        ServiceRappels rappels = CreerRappels(relances, notifications);

        Assert.Equal(1, await rappels.VerifierEtNotifierAsync(Aujourdhui));
        Assert.Equal(0, await rappels.VerifierEtNotifierAsync(Aujourdhui));

        (string titre, string message) = Assert.Single(notifications.Notifications);
        Assert.Equal("Suivi des candidatures", titre);
        Assert.Contains("Poste A", message);
        Assert.Contains("Acme", message);
    }

    [Fact]
    public async Task Une_nouvelle_relance_due_est_signalee_sans_repeter_les_precedentes()
    {
        RelancesFactices relances = new();
        NotificationsFactices notifications = new();
        relances.Relances.Add(Relance(1, "Poste A", -2));
        ServiceRappels rappels = CreerRappels(relances, notifications);
        await rappels.VerifierEtNotifierAsync(Aujourdhui);

        relances.Relances.Add(Relance(2, "Poste B", 0));
        int signalees = await rappels.VerifierEtNotifierAsync(Aujourdhui);

        Assert.Equal(1, signalees);
        Assert.Contains("Poste B", notifications.Notifications[^1].Message);
        Assert.DoesNotContain("Poste A", notifications.Notifications[^1].Message);
    }

    [Fact]
    public async Task Plusieurs_relances_dues_donnent_un_seul_message_regroupe()
    {
        RelancesFactices relances = new();
        NotificationsFactices notifications = new();
        relances.Relances.Add(Relance(1, "Poste A", -3));
        relances.Relances.Add(Relance(2, "Poste B", -1));
        ServiceRappels rappels = CreerRappels(relances, notifications);

        Assert.Equal(2, await rappels.VerifierEtNotifierAsync(Aujourdhui));

        Assert.Contains("2 relances", Assert.Single(notifications.Notifications).Message);
    }

    [Fact]
    public async Task Les_relances_a_venir_ne_sont_pas_signalees()
    {
        RelancesFactices relances = new();
        NotificationsFactices notifications = new();
        relances.Relances.Add(Relance(1, "Poste A", 3));
        ServiceRappels rappels = CreerRappels(relances, notifications);

        Assert.Equal(0, await rappels.VerifierEtNotifierAsync(Aujourdhui));

        Assert.Empty(notifications.Notifications);
    }

    [Fact]
    public async Task Rien_n_est_signale_quand_les_notifications_sont_desactivees()
    {
        await new ServiceParametres(baseDeDonnees).EnregistrerAsync(new Parametres { NotificationsActivees = false });
        RelancesFactices relances = new();
        NotificationsFactices notifications = new();
        relances.Relances.Add(Relance(1, "Poste A", -2));
        ServiceRappels rappels = CreerRappels(relances, notifications);

        Assert.Equal(0, await rappels.VerifierEtNotifierAsync(Aujourdhui));

        Assert.Empty(notifications.Notifications);
    }

    [Fact]
    public async Task Un_echec_de_notification_n_est_pas_memorise_et_la_relance_est_reproposee()
    {
        RelancesFactices relances = new();
        NotificationsFactices notifications = new() { Echouer = true };
        relances.Relances.Add(Relance(1, "Poste A", -2));
        ServiceRappels rappels = CreerRappels(relances, notifications);

        Assert.Equal(0, await rappels.VerifierEtNotifierAsync(Aujourdhui));

        notifications.Echouer = false;
        Assert.Equal(1, await rappels.VerifierEtNotifierAsync(Aujourdhui));
    }

    [Fact]
    public async Task Une_relance_dont_la_date_change_est_de_nouveau_signalee()
    {
        RelancesFactices relances = new();
        NotificationsFactices notifications = new();
        relances.Relances.Add(Relance(1, "Poste A", -2));
        ServiceRappels rappels = CreerRappels(relances, notifications);
        await rappels.VerifierEtNotifierAsync(Aujourdhui);

        relances.Relances.Clear();
        relances.Relances.Add(Relance(1, "Poste A", -1));

        Assert.Equal(1, await rappels.VerifierEtNotifierAsync(Aujourdhui));
    }
}
