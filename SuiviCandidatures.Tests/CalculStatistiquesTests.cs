using SuiviCandidatures.Coeur.Modeles.Enumerations;
using SuiviCandidatures.Coeur.Services;
using SuiviCandidatures.Coeur.Transferts;

namespace SuiviCandidatures.Tests;

public class CalculStatistiquesTests
{
    // Un lundi : la fenêtre des 12 semaines commence le lundi 3 août 2026.
    private static readonly DateTime Aujourdhui = new(2026, 10, 19);

    private static CandidatureBrute Candidature(
        StatutCandidature statut,
        DateTime? date = null,
        TypeContrat contrat = TypeContrat.ContratDureeIndeterminee,
        TypeCandidature type = TypeCandidature.ARepondreAUneOffre,
        IReadOnlyList<ActionBrute>? actions = null,
        IReadOnlyList<string>? sites = null) =>
        new(date ?? new DateTime(2026, 10, 1), statut, contrat, type, actions ?? [], sites ?? []);

    private static ActionBrute ChangementVers(StatutCandidature statut, DateTime date) =>
        new(TypeAction.ChangementStatut, date, statut);

    private static Statistiques Calculer(params CandidatureBrute[] candidatures) =>
        CalculStatistiques.Calculer(candidatures, [], Aujourdhui);

    [Fact]
    public void Sans_candidature_les_taux_et_le_delai_sont_absents()
    {
        Statistiques statistiques = Calculer();

        Assert.Equal(0, statistiques.NombreCandidatures);
        Assert.Null(statistiques.TauxReponse);
        Assert.Null(statistiques.TauxEntretien);
        Assert.Null(statistiques.DelaiMoyenReponseJours);
        Assert.Empty(statistiques.ParStatut);
    }

    [Fact]
    public void Le_taux_de_reponse_exclut_les_candidatures_retirees()
    {
        Statistiques statistiques = Calculer(
            Candidature(StatutCandidature.EnAttenteReponse),
            Candidature(StatutCandidature.EnCoursEtude),
            Candidature(StatutCandidature.Refusee),
            Candidature(StatutCandidature.Retiree));

        Assert.Equal(4, statistiques.NombreCandidatures);
        Assert.Equal(2, statistiques.NombreReponses);
        Assert.NotNull(statistiques.TauxReponse);
        Assert.Equal(2.0 / 3.0, statistiques.TauxReponse.Value, 5);
    }

    [Fact]
    public void Un_cv_vu_n_est_pas_une_reponse()
    {
        Statistiques statistiques = Calculer(Candidature(StatutCandidature.CurriculumVitaeVu));

        Assert.Equal(0, statistiques.NombreReponses);
        Assert.Equal(0.0, statistiques.TauxReponse);
    }

    [Fact]
    public void Une_reponse_passee_compte_meme_si_le_statut_actuel_est_sans_reponse()
    {
        Statistiques statistiques = Calculer(Candidature(
            StatutCandidature.OffreTermineeSansReponse,
            actions: [ChangementVers(StatutCandidature.EnCoursEtude, new DateTime(2026, 10, 5))]));

        Assert.Equal(1, statistiques.NombreReponses);
    }

    [Fact]
    public void Le_taux_d_entretien_utilise_le_statut_ou_l_historique()
    {
        Statistiques statistiques = Calculer(
            Candidature(StatutCandidature.EntretienEffectue),
            Candidature(StatutCandidature.Refusee, actions: [new ActionBrute(TypeAction.Entretien, new DateTime(2026, 10, 8), null)]),
            Candidature(StatutCandidature.EnAttenteReponse),
            Candidature(StatutCandidature.EnAttenteReponse));

        Assert.Equal(2, statistiques.NombreEntretiens);
        Assert.Equal(0.5, statistiques.TauxEntretien);
    }

    [Fact]
    public void Le_delai_moyen_est_mesure_sur_les_changements_de_statut_vers_une_reponse()
    {
        Statistiques statistiques = Calculer(
            Candidature(StatutCandidature.EnCoursEtude, new DateTime(2026, 10, 1),
                actions: [ChangementVers(StatutCandidature.EnCoursEtude, new DateTime(2026, 10, 11))]),
            Candidature(StatutCandidature.Refusee, new DateTime(2026, 10, 1),
                actions: [ChangementVers(StatutCandidature.Refusee, new DateTime(2026, 10, 5))]),
            // Statut de réponse sans date connue : exclue de la moyenne.
            Candidature(StatutCandidature.Refusee, new DateTime(2026, 10, 1)));

        Assert.Equal(2, statistiques.NombreDelaisMesures);
        Assert.Equal(7.0, statistiques.DelaiMoyenReponseJours);
    }

    [Fact]
    public void Une_action_entretien_seule_ne_donne_pas_de_delai()
    {
        Statistiques statistiques = Calculer(Candidature(
            StatutCandidature.EnAttenteReponse,
            actions: [new ActionBrute(TypeAction.Entretien, new DateTime(2026, 10, 20), null)]));

        Assert.Equal(0, statistiques.NombreDelaisMesures);
        Assert.Null(statistiques.DelaiMoyenReponseJours);
    }

    [Fact]
    public void Les_offres_recues_sont_comptees_par_statut()
    {
        Statistiques statistiques = Calculer(
            Candidature(StatutCandidature.OffreRecue),
            Candidature(StatutCandidature.OffreRecue),
            Candidature(StatutCandidature.Refusee));

        Assert.Equal(2, statistiques.NombreOffresRecues);
    }

    [Fact]
    public void La_repartition_par_statut_suit_l_ordre_des_statuts_et_omet_les_absents()
    {
        Statistiques statistiques = Calculer(
            Candidature(StatutCandidature.Refusee),
            Candidature(StatutCandidature.CurriculumVitaeVu),
            Candidature(StatutCandidature.Refusee));

        Assert.Equal(
            [nameof(StatutCandidature.CurriculumVitaeVu), nameof(StatutCandidature.Refusee)],
            statistiques.ParStatut.Select(p => p.Cle));
        Assert.Equal([1, 2], statistiques.ParStatut.Select(p => p.Valeur));
    }

    [Fact]
    public void Les_semaines_commencent_le_lundi_et_couvrent_douze_semaines()
    {
        DateTime[] dates =
        [
            new DateTime(2026, 10, 19, 10, 0, 0), // lundi de la semaine en cours
            new DateTime(2026, 10, 18),           // dimanche : semaine précédente
            new DateTime(2026, 8, 2)              // avant la fenêtre : ignorée
        ];

        Statistiques statistiques = CalculStatistiques.Calculer([], dates, Aujourdhui);

        Assert.Equal(CalculStatistiques.NombreSemaines, statistiques.ParSemaine.Count);
        Assert.Equal("03/08", statistiques.ParSemaine[0].Libelle);
        Assert.Equal(1, statistiques.ParSemaine[^1].Valeur);
        Assert.Equal(1, statistiques.ParSemaine[^2].Valeur);
        Assert.Equal(2, statistiques.ParSemaine.Sum(p => p.Valeur));
    }

    [Fact]
    public void Le_lundi_de_la_semaine_d_un_dimanche_est_le_lundi_precedent()
    {
        Assert.Equal(new DateTime(2026, 10, 12), CalculStatistiques.LundiDeLaSemaine(new DateTime(2026, 10, 18)));
        Assert.Equal(new DateTime(2026, 10, 19), CalculStatistiques.LundiDeLaSemaine(new DateTime(2026, 10, 19)));
    }

    [Fact]
    public void Une_candidature_compte_une_fois_par_site_sans_tenir_compte_de_la_casse()
    {
        Statistiques statistiques = Calculer(
            Candidature(StatutCandidature.EnAttenteReponse, sites: ["Indeed", "indeed "]),
            Candidature(StatutCandidature.EnAttenteReponse, sites: ["LinkedIn", "Indeed"]));

        Assert.Equal(2, statistiques.ParSite.Count);
        Assert.Equal(2, statistiques.ParSite[0].Valeur);
        Assert.Equal("Indeed", statistiques.ParSite[0].Libelle);
        Assert.Equal("LinkedIn", statistiques.ParSite[1].Libelle);
    }
}
