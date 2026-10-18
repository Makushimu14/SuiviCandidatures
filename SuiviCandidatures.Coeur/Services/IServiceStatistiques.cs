using SuiviCandidatures.Coeur.Transferts;

namespace SuiviCandidatures.Coeur.Services;

public interface IServiceStatistiques
{
    /// <summary>
    /// Statistiques des candidatures de la période choisie. Le graphique hebdomadaire porte toujours sur
    /// les dernières semaines, quelle que soit la période.
    /// </summary>
    Task<Statistiques> CalculerAsync(PeriodeStatistiques periode, DateTime aujourdhui, CancellationToken jeton = default);
}
