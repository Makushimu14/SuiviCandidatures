using Microsoft.EntityFrameworkCore;
using SuiviCandidatures.Coeur.Donnees;
using SuiviCandidatures.Coeur.Modeles;
using SuiviCandidatures.Coeur.Services;
using SuiviCandidatures.Presentation.Services;

namespace SuiviCandidatures.Presentation;

public partial class ApplicationSuivi : Application
{
    private readonly IServiceProvider fournisseurServices;

    public ApplicationSuivi(IServiceProvider fournisseurServices)
    {
        InitializeComponent();
        this.fournisseurServices = fournisseurServices;
    }

    protected override Window CreateWindow(IActivationState? etatActivation)
    {
        // Migration synchrone : la base doit être prête avant que la première page ne l'interroge.
        InitialiseurBaseDeDonnees.Initialiser(
            fournisseurServices.GetRequiredService<IDbContextFactory<ContexteBaseDeDonnees>>());

        IServiceNotificationsSysteme notifications = fournisseurServices.GetRequiredService<IServiceNotificationsSysteme>();

        Window fenetre = new(new CoquilleApplication())
        {
            Title = "Suivi des candidatures",
            Width = 1200,
            Height = 800
        };

        fenetre.Destroying += (_, _) => notifications.Terminer();
        fenetre.Created += async (_, _) => await InitialiserApresCreationAsync();
        return fenetre;
    }

    private async Task InitialiserApresCreationAsync()
    {
        Parametres parametres = await fournisseurServices.GetRequiredService<IServiceParametres>().ObtenirAsync();
        fournisseurServices.GetRequiredService<ServiceTheme>().Appliquer(parametres.Theme);

        // Relances dues : notification au démarrage, puis vérification régulière.
        await fournisseurServices.GetRequiredService<ServiceMinuteurRappels>().DemarrerAsync();
    }
}
