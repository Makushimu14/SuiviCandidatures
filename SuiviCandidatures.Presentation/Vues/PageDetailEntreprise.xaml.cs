using SuiviCandidatures.Presentation.ViewModels;

namespace SuiviCandidatures.Presentation.Vues;

public partial class PageDetailEntreprise : ContentPage
{
    private readonly DetailEntrepriseViewModel modeleVue;

    public PageDetailEntreprise(DetailEntrepriseViewModel modeleVue)
    {
        InitializeComponent();
        this.modeleVue = modeleVue;
        BindingContext = modeleVue;
    }

    // Rechargement à chaque affichage : la page est à jour au retour du formulaire de modification
    // ou de la page d'une candidature.
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await modeleVue.ChargerCommand.ExecuteAsync(null);
    }

    private void QuandCandidatureTapee(object? expediteur, TappedEventArgs arguments)
    {
        if (expediteur is BindableObject { BindingContext: LigneCandidature ligne })
        {
            modeleVue.OuvrirCandidatureCommand.Execute(ligne.Identifiant);
        }
    }

    private void QuandModifierAdresseClique(object? expediteur, EventArgs arguments)
    {
        if (expediteur is BindableObject { BindingContext: LigneAdresseEntreprise ligne })
        {
            modeleVue.Adresses.ModifierAdresseCommand.Execute(ligne);
        }
    }

    private void QuandSupprimerAdresseClique(object? expediteur, EventArgs arguments)
    {
        if (expediteur is BindableObject { BindingContext: LigneAdresseEntreprise ligne })
        {
            modeleVue.Adresses.SupprimerAdresseCommand.Execute(ligne);
        }
    }

    private void QuandModifierContactClique(object? expediteur, EventArgs arguments)
    {
        if (expediteur is BindableObject { BindingContext: LigneContactEntreprise ligne })
        {
            modeleVue.Contacts.ModifierContactCommand.Execute(ligne);
        }
    }

    private void QuandSupprimerContactClique(object? expediteur, EventArgs arguments)
    {
        if (expediteur is BindableObject { BindingContext: LigneContactEntreprise ligne })
        {
            modeleVue.Contacts.SupprimerContactCommand.Execute(ligne);
        }
    }
}
