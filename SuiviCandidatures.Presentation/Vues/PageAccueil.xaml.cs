using SuiviCandidatures.Presentation.ViewModels;

namespace SuiviCandidatures.Presentation.Vues;

public partial class PageAccueil : ContentPage
{
    private readonly AccueilViewModel modeleVue;

    public PageAccueil(AccueilViewModel modeleVue)
    {
        InitializeComponent();
        this.modeleVue = modeleVue;
        BindingContext = modeleVue;
    }

    // Rechargement à chaque affichage : relances et compteurs sont à jour au retour des autres pages.
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await modeleVue.ChargerCommand.ExecuteAsync(null);
    }

    private void QuandRelanceTapee(object? expediteur, TappedEventArgs arguments)
    {
        if (expediteur is BindableObject { BindingContext: LigneRelance ligne })
        {
            modeleVue.OuvrirCandidatureCommand.Execute(ligne.Identifiant);
        }
    }

    private void QuandCandidatureTapee(object? expediteur, TappedEventArgs arguments)
    {
        if (expediteur is BindableObject { BindingContext: LigneCandidature ligne })
        {
            modeleVue.OuvrirCandidatureCommand.Execute(ligne.Identifiant);
        }
    }
}
