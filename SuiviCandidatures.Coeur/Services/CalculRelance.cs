using SuiviCandidatures.Coeur.Extensions;
using SuiviCandidatures.Coeur.Modeles.Enumerations;
using SuiviCandidatures.Coeur.Transferts;

namespace SuiviCandidatures.Coeur.Services;

/// <summary>
/// Calcule la prochaine relance : date de référence + délai. La date de référence est la plus récente entre
/// la date de candidature et les actions « Candidature envoyée », relances et entretiens.
/// Les changements de statut et les actions « Autre » ne repoussent pas la relance.
/// Aucune relance pour un statut terminal ou si l'utilisateur l'a désactivée.
/// </summary>
public static class CalculRelance
{
    private static readonly TypeAction[] TypesRepoussantLaRelance =
    [
        TypeAction.CandidatureEnvoyee,
        TypeAction.RelanceParMail,
        TypeAction.RelanceParTelephone,
        TypeAction.Entretien
    ];

    public static EtatRelance Calculer(DetailCandidature detail, int delaiParDefautJours, DateTime aujourdhui) =>
        Calculer(
            detail.Statut,
            detail.RelancesDesactivees,
            detail.DelaiRelanceJours,
            detail.DateCandidature,
            detail.Actions.Select(a => (a.Type, a.Date)),
            delaiParDefautJours,
            aujourdhui);

    public static EtatRelance Calculer(
        StatutCandidature statut,
        bool relancesDesactivees,
        int? delaiPropreJours,
        DateTime dateCandidature,
        IEnumerable<(TypeAction Type, DateTime Date)> actions,
        int delaiParDefautJours,
        DateTime aujourdhui)
    {
        if (statut.EstTerminal())
        {
            return new EtatRelance(false, null, null, RaisonRelanceInactive.StatutTerminal);
        }

        if (relancesDesactivees)
        {
            return new EtatRelance(false, null, null, RaisonRelanceInactive.DesactiveeParUtilisateur);
        }

        DateTime reference = dateCandidature.Date;
        foreach ((TypeAction type, DateTime date) in actions)
        {
            if (TypesRepoussantLaRelance.Contains(type) && date.Date > reference)
            {
                reference = date.Date;
            }
        }

        DateTime dateRelance = reference.AddDays(delaiPropreJours ?? delaiParDefautJours);
        int joursRestants = (dateRelance - aujourdhui.Date).Days;

        return new EtatRelance(true, dateRelance, joursRestants, RaisonRelanceInactive.Aucune);
    }
}
