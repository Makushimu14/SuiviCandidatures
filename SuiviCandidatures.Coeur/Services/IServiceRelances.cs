using SuiviCandidatures.Coeur.Transferts;

namespace SuiviCandidatures.Coeur.Services;

public interface IServiceRelances
{
    /// <summary>
    /// Relances prévues de toutes les candidatures actives (statut non terminal, relances non désactivées),
    /// de la plus ancienne à la plus récente. Les relances dues ou en retard ont un nombre de jours restants ≤ 0.
    /// </summary>
    Task<IReadOnlyList<ElementRelance>> ListerAsync(DateTime aujourdhui, CancellationToken jeton = default);
}
