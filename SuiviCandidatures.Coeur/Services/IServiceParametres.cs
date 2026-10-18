using SuiviCandidatures.Coeur.Modeles;

namespace SuiviCandidatures.Coeur.Services;

public interface IServiceParametres
{
    /// <summary>Retourne les paramètres, en les créant avec leurs valeurs par défaut s'ils n'existent pas encore.</summary>
    Task<Parametres> ObtenirAsync(CancellationToken jeton = default);

    /// <summary>Enregistre les paramètres après les avoir ramenés dans les valeurs autorisées.</summary>
    Task EnregistrerAsync(Parametres parametres, CancellationToken jeton = default);
}
