using SuiviCandidatures.Coeur.Transferts;

namespace SuiviCandidatures.Coeur.Services;

public sealed class ServiceRappels(
    IServiceRelances serviceRelances,
    IServiceNotificationsSysteme notifications,
    IServiceParametres serviceParametres) : IServiceRappels
{
    private const string TitreNotification = "Suivi des candidatures";
    private const int NombreMaximumCandidaturesCitees = 3;

    // Mémoire de l'exécution en cours : une relance n'est signalée qu'une fois, tant que sa date ne change pas.
    private readonly HashSet<(int IdentifiantCandidature, DateTime DateRelance)> dejaSignalees = [];
    private readonly SemaphoreSlim verrou = new(1, 1);

    public async Task<int> VerifierEtNotifierAsync(DateTime aujourdhui, CancellationToken jeton = default)
    {
        await verrou.WaitAsync(jeton);
        try
        {
            // Relu à chaque vérification : un changement dans les Paramètres s'applique sans redémarrage.
            if (!(await serviceParametres.ObtenirAsync(jeton)).NotificationsActivees)
            {
                return 0;
            }

            IReadOnlyList<ElementRelance> relances = await serviceRelances.ListerAsync(aujourdhui, jeton);

            List<ElementRelance> nouvelles = relances
                .Where(r => r.EstDue && !dejaSignalees.Contains((r.IdentifiantCandidature, r.DateRelance)))
                .ToList();
            if (nouvelles.Count == 0)
            {
                return 0;
            }

            try
            {
                notifications.Afficher(TitreNotification, ConstruireMessage(nouvelles));
            }
            catch (Exception)
            {
                // Notifications indisponibles ou refusées : les relances restent visibles sur la page d'accueil,
                // et seront de nouveau proposées à la prochaine vérification.
                return 0;
            }

            foreach (ElementRelance relance in nouvelles)
            {
                dejaSignalees.Add((relance.IdentifiantCandidature, relance.DateRelance));
            }

            return nouvelles.Count;
        }
        finally
        {
            verrou.Release();
        }
    }

    private static string ConstruireMessage(IReadOnlyList<ElementRelance> relances)
    {
        if (relances.Count == 1)
        {
            ElementRelance relance = relances[0];
            return $"Relance à faire : {relance.TitreOffre} chez {relance.NomEntreprise}.";
        }

        string citees = string.Join(
            " ; ",
            relances.Take(NombreMaximumCandidaturesCitees).Select(r => $"{r.TitreOffre} ({r.NomEntreprise})"));
        string suite = relances.Count > NombreMaximumCandidaturesCitees ? " et d'autres" : string.Empty;

        return $"{relances.Count} relances à faire : {citees}{suite}.";
    }
}
