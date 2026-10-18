using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SuiviCandidatures.Coeur.Extensions;
using SuiviCandidatures.Coeur.Modeles;
using SuiviCandidatures.Coeur.Modeles.Enumerations;
using SuiviCandidatures.Coeur.Services;
using SuiviCandidatures.Presentation.Services;

namespace SuiviCandidatures.Presentation.ViewModels;

public partial class ParametresViewModel(
    IServiceParametres serviceParametres,
    ServiceTheme serviceTheme,
    IServiceNotificationsSysteme serviceNotifications,
    SauvegardeViewModel sauvegarde) : ViewModelDeBase
{
    public override string Titre => "Paramètres";

    public SauvegardeViewModel Sauvegarde { get; } = sauvegarde;

    public IReadOnlyList<int> TaillesPageDisponibles { get; } = Parametres.TaillesPageAutorisees;

    public IReadOnlyList<ChoixTheme> ThemesDisponibles { get; } =
        Enum.GetValues<ThemeApplication>().Select(theme => new ChoixTheme(theme, theme.ObtenirLibelle())).ToList();

    public string LibelleDelaiRelance =>
        $"Délai de relance par défaut, en jours ({Parametres.DelaiRelanceMinimumJours} à {Parametres.DelaiRelanceMaximumJours})";

    [ObservableProperty]
    private int delaiRelanceParDefautJours = 7;

    [ObservableProperty]
    private int taillePage = Parametres.TaillesPageAutorisees[0];

    [ObservableProperty]
    private ChoixTheme? themeSelectionne;

    [ObservableProperty]
    private bool notificationsActivees = true;

    [ObservableProperty]
    private string messageConfirmation = string.Empty;

    // Permet de vérifier que les notifications système fonctionnent sur ce poste.
    [ObservableProperty]
    private string messageNotification = string.Empty;

    [RelayCommand]
    private void TesterNotification()
    {
        try
        {
            serviceNotifications.Afficher(
                "Suivi des candidatures",
                "Ceci est une notification de test. Si vous la voyez, les rappels fonctionneront.");
            MessageNotification = "Notification envoyée. Si rien ne s'affiche, vérifiez le mode « Ne pas déranger » de Windows.";
        }
        catch (Exception exception)
        {
            MessageNotification = $"Échec : {exception.Message}";
        }
    }

    [RelayCommand]
    private async Task ChargerAsync()
    {
        Parametres parametres = await serviceParametres.ObtenirAsync();
        DelaiRelanceParDefautJours = parametres.DelaiRelanceParDefautJours;
        TaillePage = parametres.TaillePage;
        NotificationsActivees = parametres.NotificationsActivees;
        ThemeSelectionne = ThemesDisponibles.First(choix => choix.Valeur == parametres.Theme);
        MessageConfirmation = string.Empty;
    }

    [RelayCommand]
    private async Task EnregistrerAsync()
    {
        Parametres parametres = new()
        {
            DelaiRelanceParDefautJours = DelaiRelanceParDefautJours,
            TaillePage = TaillePage,
            NotificationsActivees = NotificationsActivees,
            Theme = ThemeSelectionne?.Valeur ?? ThemeApplication.Systeme
        };

        await serviceParametres.EnregistrerAsync(parametres);
        serviceTheme.Appliquer(parametres.Theme);

        // Relecture : l'affichage reflète les valeurs réellement enregistrées (après correction éventuelle).
        await ChargerAsync();
        MessageConfirmation = "Paramètres enregistrés.";
    }
}
