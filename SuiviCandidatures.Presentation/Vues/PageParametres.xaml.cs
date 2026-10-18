using SuiviCandidatures.Presentation.ViewModels;

namespace SuiviCandidatures.Presentation.Vues;

public partial class PageParametres : ContentPage
{
    private readonly ParametresViewModel modeleVue;

    public PageParametres(ParametresViewModel modeleVue)
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
