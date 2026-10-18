using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SuiviCandidatures.Coeur.Services;
using SuiviCandidatures.Coeur.Transferts;
using SuiviCandidatures.Presentation.Services;

namespace SuiviCandidatures.Presentation.ViewModels;

[QueryProperty(nameof(IdentifiantEntreprise), NomsRoutes.ParametreIdentifiant)]
public partial class FormulaireEntrepriseViewModel(
    IServiceEntreprises serviceEntreprises,
    IServiceNavigation navigation) : ViewModelDeBase
{
    private bool initialise;

    public override string Titre => IdentifiantEntreprise == 0 ? "Nouvelle entreprise" : "Modifier l'entreprise";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Titre))]
    private int identifiantEntreprise;

    [ObservableProperty]
    private string nom = string.Empty;

    [ObservableProperty]
    private string siteWeb = string.Empty;

    [ObservableProperty]
    private string secteur = string.Empty;

    [ObservableProperty]
    private string notes = string.Empty;

    [ObservableProperty]
    private string messageErreur = string.Empty;

    [RelayCommand]
    private async Task ChargerAsync()
    {
        // La page peut réapparaître : on ne réinitialise le formulaire qu'une seule fois.
        if (initialise || IdentifiantEntreprise == 0)
        {
            initialise = true;
            return;
        }

        initialise = true;

        try
        {
            DonneesEntreprise? donnees = await serviceEntreprises.ObtenirPourModificationAsync(IdentifiantEntreprise);
            if (donnees is null)
            {
                MessageErreur = "Cette entreprise est introuvable.";
                return;
            }

            Nom = donnees.Nom;
            SiteWeb = donnees.SiteWeb ?? string.Empty;
            Secteur = donnees.Secteur ?? string.Empty;
            Notes = donnees.Notes ?? string.Empty;
        }
        catch (Exception exception)
        {
            MessageErreur = $"Chargement impossible : {exception.Message}";
        }
    }

    [RelayCommand]
    private async Task EnregistrerAsync()
    {
        MessageErreur = string.Empty;

        DonneesEntreprise donnees = new()
        {
            Identifiant = IdentifiantEntreprise,
            Nom = Nom,
            SiteWeb = SiteWeb,
            Secteur = Secteur,
            Notes = Notes
        };

        try
        {
            await serviceEntreprises.EnregistrerAsync(donnees);
            await navigation.RetourAsync();
        }
        catch (ExceptionValidationMetier exception)
        {
            MessageErreur = exception.Message;
        }
        catch (Exception exception)
        {
            MessageErreur = $"Enregistrement impossible : {exception.Message}";
        }
    }

    [RelayCommand]
    private Task AnnulerAsync() => navigation.RetourAsync();
}
