using SuiviCandidatures.Presentation.ViewModels;

namespace SuiviCandidatures.Presentation.Vues;

public partial class PageDetailCandidature : ContentPage
{
    private readonly DetailCandidatureViewModel modeleVue;

    public PageDetailCandidature(DetailCandidatureViewModel modeleVue)
    {
        InitializeComponent();
        this.modeleVue = modeleVue;
        BindingContext = modeleVue;
    }

    // Rechargement à chaque affichage : la page est à jour au retour du formulaire de modification.
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await modeleVue.ChargerCommand.ExecuteAsync(null);
    }

    private void QuandModifierOffreClique(object? expediteur, EventArgs arguments)
    {
        if (expediteur is BindableObject { BindingContext: LigneOffre ligne })
        {
            modeleVue.Offres.ModifierOffreCommand.Execute(ligne);
        }
    }

    private void QuandSupprimerOffreClique(object? expediteur, EventArgs arguments)
    {
        if (expediteur is BindableObject { BindingContext: LigneOffre ligne })
        {
            modeleVue.Offres.SupprimerOffreCommand.Execute(ligne);
        }
    }

    private void QuandLienOffreTape(object? expediteur, TappedEventArgs arguments)
    {
        if (expediteur is BindableObject { BindingContext: LigneOffre ligne })
        {
            modeleVue.Offres.OuvrirLienCommand.Execute(ligne);
        }
    }

    private void QuandModifierContactClique(object? expediteur, EventArgs arguments)
    {
        if (expediteur is BindableObject { BindingContext: LigneContact ligne })
        {
            modeleVue.Contacts.ModifierContactCommand.Execute(ligne);
        }
    }

    private void QuandDelierContactClique(object? expediteur, EventArgs arguments)
    {
        if (expediteur is BindableObject { BindingContext: LigneContact ligne })
        {
            modeleVue.Contacts.DelierCommand.Execute(ligne);
        }
    }

    private void QuandModifierActionClique(object? expediteur, EventArgs arguments)
    {
        if (expediteur is BindableObject { BindingContext: LigneAction ligne })
        {
            modeleVue.ModifierActionCommand.Execute(ligne);
        }
    }

    private void QuandSupprimerActionClique(object? expediteur, EventArgs arguments)
    {
        if (expediteur is BindableObject { BindingContext: LigneAction ligne })
        {
            modeleVue.SupprimerActionCommand.Execute(ligne);
        }
    }
}
