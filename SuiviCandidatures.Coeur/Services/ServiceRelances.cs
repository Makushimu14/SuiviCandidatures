using Microsoft.EntityFrameworkCore;
using SuiviCandidatures.Coeur.Donnees;
using SuiviCandidatures.Coeur.Modeles;
using SuiviCandidatures.Coeur.Transferts;

namespace SuiviCandidatures.Coeur.Services;

public sealed class ServiceRelances(
    IDbContextFactory<ContexteBaseDeDonnees> fabrique,
    IServiceParametres serviceParametres) : IServiceRelances
{
    public async Task<IReadOnlyList<ElementRelance>> ListerAsync(DateTime aujourdhui, CancellationToken jeton = default)
    {
        Parametres parametres = await serviceParametres.ObtenirAsync(jeton);

        await using ContexteBaseDeDonnees contexte = await fabrique.CreateDbContextAsync(jeton);

        // Le calcul se fait en mémoire, avec la même règle que la page de détail (CalculRelance).
        var candidatures = await contexte.Candidatures
            .AsNoTracking()
            .Where(c => !c.RelancesDesactivees)
            .Select(c => new
            {
                c.Identifiant,
                c.TitreOffre,
                NomEntreprise = c.Entreprise.Nom,
                c.DateCandidature,
                c.Statut,
                c.DelaiRelanceJours,
                Actions = c.Actions.Select(a => new { a.Type, a.Date }).ToList()
            })
            .ToListAsync(jeton);

        List<ElementRelance> relances = [];
        foreach (var candidature in candidatures)
        {
            EtatRelance etat = CalculRelance.Calculer(
                candidature.Statut,
                relancesDesactivees: false,
                candidature.DelaiRelanceJours,
                candidature.DateCandidature,
                candidature.Actions.Select(a => (a.Type, a.Date)),
                parametres.DelaiRelanceParDefautJours,
                aujourdhui);

            if (etat.EstActive)
            {
                relances.Add(new ElementRelance(
                    candidature.Identifiant,
                    candidature.TitreOffre,
                    candidature.NomEntreprise,
                    etat.DateRelance!.Value,
                    etat.JoursRestants!.Value));
            }
        }

        return relances
            .OrderBy(r => r.DateRelance)
            .ThenBy(r => r.TitreOffre, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }
}
