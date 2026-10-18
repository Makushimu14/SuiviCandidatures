using SuiviCandidatures.Coeur.Services;

namespace SuiviCandidatures.Presentation.Services;

/// <summary>
/// Vérifie les relances dues au démarrage, puis à intervalle régulier tant que l'application tourne.
/// Les rappels ne fonctionnent pas application fermée.
/// </summary>
public sealed class ServiceMinuteurRappels(IServiceRappels serviceRappels)
{
    private static readonly TimeSpan IntervalleVerification = TimeSpan.FromMinutes(30);

    private IDispatcherTimer? minuteur;

    public async Task DemarrerAsync()
    {
        if (minuteur is not null)
        {
            return;
        }

        await VerifierAsync();

        minuteur = Application.Current?.Dispatcher.CreateTimer();
        if (minuteur is null)
        {
            return;
        }

        minuteur.Interval = IntervalleVerification;
        minuteur.Tick += async (_, _) => await VerifierAsync();
        minuteur.Start();
    }

    private async Task VerifierAsync()
    {
        try
        {
            await serviceRappels.VerifierEtNotifierAsync(DateTime.Today);
        }
        catch (Exception)
        {
            // Un échec de rappel ne doit jamais perturber l'application : la page d'accueil reste la référence.
        }
    }
}
