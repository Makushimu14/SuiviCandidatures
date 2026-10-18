using SuiviCandidatures.Presentation.ViewModels;

namespace SuiviCandidatures.Presentation.Vues;

public partial class PageFormulaireEntreprise : ContentPage
{
    private readonly FormulaireEntrepriseViewModel modeleVue;

    public PageFormulaireEntreprise(FormulaireEntrepriseViewModel modeleVue)
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
}
