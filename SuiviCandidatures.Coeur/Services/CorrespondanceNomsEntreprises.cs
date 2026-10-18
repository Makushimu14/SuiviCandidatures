using System.Globalization;
using SuiviCandidatures.Coeur.Transferts;

namespace SuiviCandidatures.Coeur.Services;

/// <summary>
/// Comparaison de noms d'entreprises sans tenir compte de la casse ni des accents.
/// Réalisée en mémoire : SQLite ne sait pas ignorer les accents.
/// </summary>
public static class CorrespondanceNomsEntreprises
{
    private const CompareOptions Options = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace;

    private static CompareInfo Comparateur => CultureInfo.InvariantCulture.CompareInfo;

    public static ResumeEntreprise? TrouverExacte(IEnumerable<ResumeEntreprise> entreprises, string nom)
    {
        string recherche = nom.Trim();
        if (recherche.Length == 0)
        {
            return null;
        }

        return entreprises.FirstOrDefault(e => Comparateur.Compare(e.Nom.Trim(), recherche, Options) == 0);
    }

    /// <summary>Entreprises dont le nom contient le texte, celles qui commencent par le texte en premier.</summary>
    public static IReadOnlyList<ResumeEntreprise> Suggerer(IEnumerable<ResumeEntreprise> entreprises, string texte, int maximum)
    {
        string recherche = texte.Trim();
        if (recherche.Length == 0)
        {
            return [];
        }

        return entreprises
            .Select(e => new { Entreprise = e, Position = Comparateur.IndexOf(e.Nom, recherche, Options) })
            .Where(x => x.Position >= 0)
            .OrderBy(x => x.Position == 0 ? 0 : 1)
            .ThenBy(x => x.Entreprise.Nom, StringComparer.CurrentCultureIgnoreCase)
            .Take(maximum)
            .Select(x => x.Entreprise)
            .ToList();
    }
}
