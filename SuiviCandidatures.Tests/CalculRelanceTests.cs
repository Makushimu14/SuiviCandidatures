using SuiviCandidatures.Coeur.Modeles.Enumerations;
using SuiviCandidatures.Coeur.Services;
using SuiviCandidatures.Coeur.Transferts;

namespace SuiviCandidatures.Tests;

public class CalculRelanceTests
{
    private static readonly DateTime Aujourdhui = new(2026, 10, 19);

    private static EtatRelance Calculer(
        StatutCandidature statut = StatutCandidature.EnAttenteReponse,
        bool relancesDesactivees = false,
        int? delaiPropreJours = null,
        DateTime? dateCandidature = null,
        IEnumerable<(TypeAction Type, DateTime Date)>? actions = null,
        int delaiParDefautJours = 7) =>
        CalculRelance.Calculer(
            statut,
            relancesDesactivees,
            delaiPropreJours,
            dateCandidature ?? new DateTime(2026, 10, 10),
            actions ?? [],
            delaiParDefautJours,
            Aujourdhui);

    [Theory]
    [InlineData(StatutCandidature.OffreRecue)]
    [InlineData(StatutCandidature.OffreTermineeSansReponse)]
    [InlineData(StatutCandidature.Refusee)]
    [InlineData(StatutCandidature.Retiree)]
    public void Un_statut_terminal_desactive_la_relance(StatutCandidature statut)
    {
        EtatRelance etat = Calculer(statut);

        Assert.False(etat.EstActive);
        Assert.Equal(RaisonRelanceInactive.StatutTerminal, etat.Raison);
        Assert.Null(etat.DateRelance);
    }

    [Fact]
    public void La_relance_desactivee_par_l_utilisateur_n_est_pas_active()
    {
        EtatRelance etat = Calculer(relancesDesactivees: true);

        Assert.False(etat.EstActive);
        Assert.Equal(RaisonRelanceInactive.DesactiveeParUtilisateur, etat.Raison);
    }

    [Fact]
    public void Sans_action_la_relance_est_la_date_de_candidature_plus_le_delai_par_defaut()
    {
        EtatRelance etat = Calculer();

        Assert.True(etat.EstActive);
        Assert.Equal(new DateTime(2026, 10, 17), etat.DateRelance);
        Assert.Equal(-2, etat.JoursRestants);
    }

    [Fact]
    public void Le_delai_propre_a_la_candidature_est_prioritaire()
    {
        EtatRelance etat = Calculer(delaiPropreJours: 3);

        Assert.Equal(new DateTime(2026, 10, 13), etat.DateRelance);
        Assert.Equal(-6, etat.JoursRestants);
    }

    [Fact]
    public void Une_relance_recente_repousse_la_prochaine_relance()
    {
        EtatRelance etat = Calculer(actions: [(TypeAction.RelanceParMail, new DateTime(2026, 10, 15, 14, 0, 0))]);

        Assert.Equal(new DateTime(2026, 10, 22), etat.DateRelance);
        Assert.Equal(3, etat.JoursRestants);
    }

    [Fact]
    public void Un_changement_de_statut_ou_une_action_autre_ne_repoussent_pas_la_relance()
    {
        EtatRelance etat = Calculer(actions:
        [
            (TypeAction.ChangementStatut, new DateTime(2026, 10, 18)),
            (TypeAction.Autre, new DateTime(2026, 10, 18))
        ]);

        Assert.Equal(new DateTime(2026, 10, 17), etat.DateRelance);
    }

    [Fact]
    public void Un_entretien_a_venir_repousse_la_relance_apres_sa_date()
    {
        EtatRelance etat = Calculer(actions: [(TypeAction.Entretien, new DateTime(2026, 10, 25))]);

        Assert.Equal(new DateTime(2026, 11, 1), etat.DateRelance);
        Assert.Equal(13, etat.JoursRestants);
    }

    [Fact]
    public void Une_relance_prevue_aujourd_hui_a_zero_jour_restant()
    {
        EtatRelance etat = Calculer(dateCandidature: new DateTime(2026, 10, 12));

        Assert.Equal(0, etat.JoursRestants);
    }

    [Fact]
    public void Une_action_plus_ancienne_que_la_candidature_est_ignoree()
    {
        EtatRelance etat = Calculer(actions: [(TypeAction.RelanceParTelephone, new DateTime(2026, 9, 1))]);

        Assert.Equal(new DateTime(2026, 10, 17), etat.DateRelance);
    }
}
