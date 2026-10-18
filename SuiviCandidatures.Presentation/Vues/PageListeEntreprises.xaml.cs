using SuiviCandidatures.Presentation.ViewModels;

namespace SuiviCandidatures.Presentation.Vues;

public partial class PageListeEntreprises : ContentPage
{
    private readonly ListeEntreprisesViewModel modeleVue;

    public PageListeEntreprises(ListeEntreprisesViewModel modeleVue)
    {
        InitializeComponent();
        this.modeleVue = modeleVue;
        BindingContext = modeleVue;
    }

    // Rechargement à chaque affichage : la liste est à jour au retour du formulaire ou du détail.
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await modeleVue.ChargerCommand.ExecuteAsync(null);
    }

    private void QuandLigneTapee(object? expediteur, TappedEventArgs arguments)
    {
        if (expediteur is BindableObject { BindingContext: LigneEntreprise ligne })
        {
            modeleVue.OuvrirCommand.Execute(ligne);
        }
    }
}
