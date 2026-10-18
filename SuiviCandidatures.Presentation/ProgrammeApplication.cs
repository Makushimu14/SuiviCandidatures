using LiveChartsCore.SkiaSharpView.Maui;
using Microsoft.EntityFrameworkCore;
using SkiaSharp.Views.Maui.Controls.Hosting;
using SuiviCandidatures.Coeur.Donnees;
using SuiviCandidatures.Coeur.Services;
using SuiviCandidatures.Presentation.Services;
using SuiviCandidatures.Presentation.ViewModels;
using SuiviCandidatures.Presentation.Vues;

namespace SuiviCandidatures.Presentation;

public static class ProgrammeApplication
{
    public static MauiApp CreerApplicationMaui()
    {
        MauiAppBuilder constructeur = MauiApp.CreateBuilder();
        // Graphiques LiveCharts : SkiaSharp doit être enregistré avant LiveCharts.
        constructeur.UseSkiaSharp();
        constructeur.UseLiveCharts();
        constructeur.UseMauiApp<ApplicationSuivi>();

        // Base de données locale
        constructeur.Services.AddDbContextFactory<ContexteBaseDeDonnees>(options =>
            options.UseSqlite($"Data Source={EmplacementBaseDeDonnees.ObtenirChemin()}"));

        // Services
        constructeur.Services.AddSingleton<IServiceParametres, ServiceParametres>();
        constructeur.Services.AddSingleton<IServiceCandidatures, ServiceCandidatures>();
        constructeur.Services.AddSingleton<IServiceEntreprises, ServiceEntreprises>();
        constructeur.Services.AddSingleton<IServiceSuiviCandidature, ServiceSuiviCandidature>();
        constructeur.Services.AddSingleton<IServiceRelances, ServiceRelances>();
        constructeur.Services.AddSingleton<IServiceStatistiques, ServiceStatistiques>();
        constructeur.Services.AddSingleton<IServiceSauvegarde, ServiceSauvegarde>();
        constructeur.Services.AddSingleton<ServiceFichiersSauvegarde>();
        constructeur.Services.AddSingleton<IServiceRappels, ServiceRappels>();
        constructeur.Services.AddSingleton<ServiceMinuteurRappels>();
        constructeur.Services.AddSingleton<IServiceOffres, ServiceOffres>();
        constructeur.Services.AddSingleton<IServiceAdresses, ServiceAdresses>();
        constructeur.Services.AddSingleton<IServiceContacts, ServiceContacts>();
        constructeur.Services.AddSingleton<IServiceNavigation, ServiceNavigation>();
        constructeur.Services.AddSingleton<IServiceLiensExternes, ServiceLiensExternes>();
        constructeur.Services.AddSingleton<IServiceDialogues, ServiceDialogues>();
        constructeur.Services.AddSingleton<ServiceTheme>();
#if WINDOWS
        constructeur.Services.AddSingleton<IServiceNotificationsSysteme, ServiceNotificationsWindows>();
#else
        constructeur.Services.AddSingleton<IServiceNotificationsSysteme, ServiceNotificationsNonPrisesEnCharge>();
#endif

        // Pages et modèles de vue
        constructeur.Services.AddTransient<PageAccueil>();
        constructeur.Services.AddTransient<AccueilViewModel>();
        constructeur.Services.AddTransient<PageListeCandidatures>();
        constructeur.Services.AddTransient<ListeCandidaturesViewModel>();
        constructeur.Services.AddTransient<PageFormulaireCandidature>();
        constructeur.Services.AddTransient<FormulaireCandidatureViewModel>();
        constructeur.Services.AddTransient<OffresCandidatureViewModel>();
        constructeur.Services.AddTransient<ContactsCandidatureViewModel>();
        constructeur.Services.AddTransient<PageDetailCandidature>();
        constructeur.Services.AddTransient<DetailCandidatureViewModel>();
        constructeur.Services.AddTransient<PageListeEntreprises>();
        constructeur.Services.AddTransient<ListeEntreprisesViewModel>();
        constructeur.Services.AddTransient<PageFormulaireEntreprise>();
        constructeur.Services.AddTransient<FormulaireEntrepriseViewModel>();
        constructeur.Services.AddTransient<AdressesEntrepriseViewModel>();
        constructeur.Services.AddTransient<ContactsEntrepriseViewModel>();
        constructeur.Services.AddTransient<PageDetailEntreprise>();
        constructeur.Services.AddTransient<DetailEntrepriseViewModel>();
        constructeur.Services.AddTransient<PageStatistiques>();
        constructeur.Services.AddTransient<StatistiquesViewModel>();
        constructeur.Services.AddTransient<SauvegardeViewModel>();
        constructeur.Services.AddTransient<PageParametres>();
        constructeur.Services.AddTransient<ParametresViewModel>();

        return constructeur.Build();
    }
}
