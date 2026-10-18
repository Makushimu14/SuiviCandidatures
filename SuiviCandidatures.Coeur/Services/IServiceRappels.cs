namespace SuiviCandidatures.Coeur.Services;

public interface IServiceRappels
{
    /// <summary>
    /// Envoie une notification système pour les relances dues pas encore signalées pendant cette exécution.
    /// Au premier appel (démarrage), toutes les relances dues sont signalées ; ensuite, seulement les nouvelles.
    /// Ne fait rien si les notifications sont désactivées dans les paramètres.
    /// Retourne le nombre de relances signalées. Ne lève pas d'exception si les notifications sont indisponibles.
    /// </summary>
    Task<int> VerifierEtNotifierAsync(DateTime aujourdhui, CancellationToken jeton = default);
}
