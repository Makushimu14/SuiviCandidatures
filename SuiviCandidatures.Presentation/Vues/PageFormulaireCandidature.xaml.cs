using SuiviCandidatures.Coeur.Transferts;
using SuiviCandidatures.Presentation.ViewModels;

namespace SuiviCandidatures.Presentation.Vues;

public partial class PageFormulaireCandidature : ContentPage
{
    private readonly FormulaireCandidatureViewModel modeleVue;

    public PageFormulaireCandidature(FormulaireCandidatureViewModel modeleVue)
    {
        InitializeComponent();
        this.modeleVue = modeleVue;
        BindingContext = modeleVue;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await modeleVue.ChargerCommand.ExecuteAsync(null);
    }

    private void QuandSuggestionTapee(object? expediteur, TappedEventArgs arguments)
    {
        if (expediteur is BindableObject { BindingContext: ResumeEntreprise suggestion })
        {
            modeleVue.ChoisirSuggestionCommand.Execute(suggestion);
        }
    }
}
