using Microsoft.EntityFrameworkCore;
using SuiviCandidatures.Coeur.Donnees;
using SuiviCandidatures.Coeur.Modeles;
using SuiviCandidatures.Coeur.Transferts;

namespace SuiviCandidatures.Coeur.Services;

public sealed class ServiceStatistiques(IDbContextFactory<ContexteBaseDeDonnees> fabrique) : IServiceStatistiques
{
    public async Task<Statistiques> CalculerAsync(PeriodeStatistiques periode, DateTime aujourdhui, CancellationToken jeton = default)
    {
        await using ContexteBaseDeDonnees contexte = await fabrique.CreateDbContextAsync(jeton);

        IQueryable<Candidature> requete = contexte.Candidatures.AsNoTracking();
        if (DebutPeriode(periode, aujourdhui) is { } debut)
        {
            requete = requete.Where(c => c.DateCandidature >= debut);
        }

        // Le calcul se fait en mémoire : volumes très faibles pour un usage personnel.
        var lignes = await requete
            .Select(c => new
            {
                c.DateCandidature,
                c.Statut,
                c.TypeContrat,
                c.TypeCandidature,
                Actions = c.Actions.Select(a => new { a.Type, a.Date, a.NouveauStatut }).ToList(),
                Sites = c.Offres.Where(o => o.NomSite != null).Select(o => o.NomSite!).ToList()
            })
            .ToListAsync(jeton);

        List<CandidatureBrute> candidatures = lignes
            .Select(l => new CandidatureBrute(
                l.DateCandidature,
                l.Statut,
                l.TypeContrat,
                l.TypeCandidature,
                l.Actions.Select(a => new ActionBrute(a.Type, a.Date, a.NouveauStatut)).ToList(),
                l.Sites))
            .ToList();

        DateTime debutSemaines = CalculStatistiques.DebutFenetreSemaines(aujourdhui);
        List<DateTime> datesRecentes = await contexte.Candidatures
            .AsNoTracking()
            .Where(c => c.DateCandidature >= debutSemaines)
            .Select(c => c.DateCandidature)
            .ToListAsync(jeton);

        return CalculStatistiques.Calculer(candidatures, datesRecentes, aujourdhui);
    }

    private static DateTime? DebutPeriode(PeriodeStatistiques periode, DateTime aujourdhui) => periode switch
    {
        PeriodeStatistiques.TrenteJours => aujourdhui.Date.AddDays(-30),
        PeriodeStatistiques.QuatreVingtDixJours => aujourdhui.Date.AddDays(-90),
        PeriodeStatistiques.DouzeMois => aujourdhui.Date.AddMonths(-12),
        _ => null
    };
}
