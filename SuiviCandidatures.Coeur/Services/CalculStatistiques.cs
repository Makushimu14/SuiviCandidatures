using System.Globalization;
using SuiviCandidatures.Coeur.Extensions;
using SuiviCandidatures.Coeur.Modeles.Enumerations;
using SuiviCandidatures.Coeur.Transferts;

namespace SuiviCandidatures.Coeur.Services;

/// <summary>
/// Calcul des statistiques, sans accès à la base de données.
/// Réponse : l'employeur a répondu (étude, entretien, offre ou refus), d'après le statut actuel ou l'historique.
/// Les candidatures retirées sont exclues des taux. Le délai de réponse court de la date de candidature au premier
/// changement de statut vers une réponse enregistré dans l'historique : une candidature créée directement avec un
/// statut de réponse n'a pas de date connue et n'entre pas dans la moyenne.
/// </summary>
public static class CalculStatistiques
{
    public const int NombreSemaines = 12;

    private const int NombreMaximumSites = 8;

    private static readonly CultureInfo Francais = CultureInfo.GetCultureInfo("fr-FR");

    private static readonly StatutCandidature[] StatutsReponse =
    [
        StatutCandidature.EnCoursEtude,
        StatutCandidature.EntretienPlanifie,
        StatutCandidature.EntretienEffectue,
        StatutCandidature.OffreRecue,
        StatutCandidature.Refusee
    ];

    private static readonly StatutCandidature[] StatutsEntretien =
    [
        StatutCandidature.EntretienPlanifie,
        StatutCandidature.EntretienEffectue,
        StatutCandidature.OffreRecue
    ];

    public static DateTime LundiDeLaSemaine(DateTime date)
    {
        int decalage = ((int)date.DayOfWeek + 6) % 7;
        return date.Date.AddDays(-decalage);
    }

    /// <summary>Premier jour (un lundi) de la fenêtre des dernières semaines affichée par le graphique hebdomadaire.</summary>
    public static DateTime DebutFenetreSemaines(DateTime aujourdhui) =>
        LundiDeLaSemaine(aujourdhui).AddDays(-7 * (NombreSemaines - 1));

    /// <param name="candidatures">Candidatures de la période choisie.</param>
    /// <param name="datesCandidaturesRecentes">Dates de toutes les candidatures de la fenêtre des dernières semaines, quelle que soit la période.</param>
    public static Statistiques Calculer(
        IReadOnlyList<CandidatureBrute> candidatures,
        IReadOnlyList<DateTime> datesCandidaturesRecentes,
        DateTime aujourdhui)
    {
        List<CandidatureBrute> eligibles = candidatures.Where(c => c.Statut != StatutCandidature.Retiree).ToList();

        int nombreReponses = eligibles.Count(ARecuUneReponse);
        int nombreEntretiens = eligibles.Count(ATenuUnEntretien);

        List<int> delais = candidatures
            .Select(DelaiReponseJours)
            .Where(delai => delai.HasValue)
            .Select(delai => delai!.Value)
            .ToList();

        return new Statistiques(
            candidatures.Count,
            nombreReponses,
            Proportion(nombreReponses, eligibles.Count),
            nombreEntretiens,
            Proportion(nombreEntretiens, eligibles.Count),
            candidatures.Count(c => c.Statut == StatutCandidature.OffreRecue),
            delais.Count == 0 ? null : delais.Average(),
            delais.Count,
            Repartir<StatutCandidature>(candidatures.Select(c => c.Statut)),
            Repartir<TypeContrat>(candidatures.Select(c => c.TypeContrat)),
            Repartir<TypeCandidature>(candidatures.Select(c => c.TypeCandidature)),
            RepartirParSite(candidatures),
            RepartirParSemaine(datesCandidaturesRecentes, aujourdhui));
    }

    private static double? Proportion(int nombre, int total) => total == 0 ? null : nombre / (double)total;

    private static bool ARecuUneReponse(CandidatureBrute candidature) =>
        StatutsReponse.Contains(candidature.Statut)
        || candidature.Actions.Any(a => a.Type == TypeAction.Entretien)
        || candidature.Actions.Any(a => a.Type == TypeAction.ChangementStatut
            && a.NouveauStatut is { } statut && StatutsReponse.Contains(statut));

    private static bool ATenuUnEntretien(CandidatureBrute candidature) =>
        StatutsEntretien.Contains(candidature.Statut)
        || candidature.Actions.Any(a => a.Type == TypeAction.Entretien)
        || candidature.Actions.Any(a => a.Type == TypeAction.ChangementStatut
            && a.NouveauStatut is { } statut && StatutsEntretien.Contains(statut));

    private static int? DelaiReponseJours(CandidatureBrute candidature)
    {
        DateTime? dateReponse = candidature.Actions
            .Where(a => a.Type == TypeAction.ChangementStatut
                && a.NouveauStatut is { } statut && StatutsReponse.Contains(statut))
            .Select(a => (DateTime?)a.Date)
            .Min();

        return dateReponse is null
            ? null
            : Math.Max(0, (dateReponse.Value.Date - candidature.DateCandidature.Date).Days);
    }

    /// <summary>Répartition par valeur d'énumération, dans l'ordre de déclaration. Les valeurs absentes sont omises.</summary>
    private static List<PointStatistique> Repartir<TEnumeration>(IEnumerable<TEnumeration> valeurs)
        where TEnumeration : struct, Enum
    {
        List<TEnumeration> liste = valeurs.ToList();

        return Enum.GetValues<TEnumeration>()
            .Select(valeur => new PointStatistique(
                valeur.ToString(),
                valeur.ObtenirLibelle(),
                liste.Count(v => EqualityComparer<TEnumeration>.Default.Equals(v, valeur))))
            .Where(point => point.Valeur > 0)
            .ToList();
    }

    /// <summary>Une candidature compte une fois par site distinct (sans tenir compte de la casse).</summary>
    private static List<PointStatistique> RepartirParSite(IReadOnlyList<CandidatureBrute> candidatures) =>
        candidatures
            .SelectMany(c => c.Sites
                .Select(site => site.Trim())
                .Where(site => site.Length > 0)
                .Distinct(StringComparer.CurrentCultureIgnoreCase))
            .GroupBy(site => site, StringComparer.CurrentCultureIgnoreCase)
            .Select(groupe => new PointStatistique(groupe.Key, groupe.Key, groupe.Count()))
            .OrderByDescending(point => point.Valeur)
            .ThenBy(point => point.Libelle, StringComparer.CurrentCultureIgnoreCase)
            .Take(NombreMaximumSites)
            .ToList();

    private static List<PointStatistique> RepartirParSemaine(IReadOnlyList<DateTime> dates, DateTime aujourdhui)
    {
        DateTime premierLundi = DebutFenetreSemaines(aujourdhui);

        return Enumerable.Range(0, NombreSemaines)
            .Select(indice =>
            {
                DateTime debut = premierLundi.AddDays(7 * indice);
                DateTime fin = debut.AddDays(7);
                return new PointStatistique(
                    debut.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    debut.ToString("dd/MM", Francais),
                    dates.Count(date => date.Date >= debut && date.Date < fin));
            })
            .ToList();
    }
}
