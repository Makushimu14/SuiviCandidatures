using SuiviCandidatures.Presentation.ViewModels;

namespace SuiviCandidatures.Presentation.Vues;

public partial class PageListeCandidatures : ContentPage
{
    private readonly ListeCandidaturesViewModel modeleVue;

    public PageListeCandidatures(ListeCandidaturesViewModel modeleVue)
    {
        InitializeComponent();
        this.modeleVue = modeleVue;
        BindingContext = modeleVue;
    }

    // Rechargement à chaque affichage : la liste est à jour au retour du formulaire.
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await modeleVue.ChargerCommand.ExecuteAsync(null);
    }

    private void QuandLigneTapee(object? expediteur, TappedEventArgs arguments)
    {
        if (expediteur is BindableObject { BindingContext: LigneCandidature ligne })
        {
            modeleVue.OuvrirCommand.Execute(ligne);
        }
    }
}
