using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SuiviCandidatures.Coeur.Services;
using SuiviCandidatures.Coeur.Transferts;
using SuiviCandidatures.Presentation.Services;

namespace SuiviCandidatures.Presentation.ViewModels;

/// <summary>Carte « Adresses » de la page de détail d'une entreprise.</summary>
public partial class AdressesEntrepriseViewModel(
    IServiceAdresses serviceAdresses,
    IServiceDialogues dialogues) : ObservableObject
{
    private int identifiantEntreprise;
    private int identifiantAdresseEnEdition;
    private Func<Task>? rechargerPageParente;

    public ObservableCollection<LigneAdresseEntreprise> Adresses { get; } = [];

    public bool AucuneAdresse => Adresses.Count == 0;

    [ObservableProperty]
    private bool formulaireVisible;

    [ObservableProperty]
    private string titreFormulaire = string.Empty;

    [ObservableProperty]
    private string rue = string.Empty;

    [ObservableProperty]
    private string codePostal = string.Empty;

    [ObservableProperty]
    private string ville = string.Empty;

    [ObservableProperty]
    private string mail = string.Empty;

    [ObservableProperty]
    private string telephone = string.Empty;

    [ObservableProperty]
    private string messageErreur = string.Empty;

    /// <summary>Appelée par la page de détail à chaque chargement. N'affecte pas le formulaire s'il est ouvert.</summary>
    public void Charger(int identifiantEntreprise, IReadOnlyList<ElementAdresse> adresses, Func<Task> rechargerPageParente)
    {
        this.identifiantEntreprise = identifiantEntreprise;
        this.rechargerPageParente = rechargerPageParente;

        Adresses.Clear();
        foreach (ElementAdresse adresse in adresses)
        {
            Adresses.Add(LigneAdresseEntreprise.Depuis(adresse));
        }

        OnPropertyChanged(nameof(AucuneAdresse));
    }

    [RelayCommand]
    private void AjouterAdresse()
    {
        identifiantAdresseEnEdition = 0;
        TitreFormulaire = "Nouvelle adresse";
        Rue = CodePostal = Ville = Mail = Telephone = string.Empty;
        MessageErreur = string.Empty;
        FormulaireVisible = true;
    }

    [RelayCommand]
    private void ModifierAdresse(LigneAdresseEntreprise ligne)
    {
        identifiantAdresseEnEdition = ligne.Identifiant;
        TitreFormulaire = "Modifier l'adresse";
        Rue = ligne.Adresse ?? string.Empty;
        CodePostal = ligne.CodePostal ?? string.Empty;
        Ville = ligne.Ville ?? string.Empty;
        Mail = ligne.Mail ?? string.Empty;
        Telephone = ligne.Telephone ?? string.Empty;
        MessageErreur = string.Empty;
        FormulaireVisible = true;
    }

    [RelayCommand]
    private void AnnulerAdresse() => FormulaireVisible = false;

    [RelayCommand]
    private async Task EnregistrerAdresseAsync()
    {
        MessageErreur = string.Empty;

        DonneesAdresseEntreprise donnees = new()
        {
            Identifiant = identifiantAdresseEnEdition,
            IdentifiantEntreprise = identifiantEntreprise,
            Adresse = Rue,
            CodePostal = CodePostal,
            Ville = Ville,
            Mail = Mail,
            Telephone = Telephone
        };

        try
        {
            await serviceAdresses.EnregistrerAsync(donnees);
            FormulaireVisible = false;
            await RechargerAsync();
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
    private async Task SupprimerAdresseAsync(LigneAdresseEntreprise ligne)
    {
        bool confirme = await dialogues.ConfirmerAsync(
            "Supprimer l'adresse",
            $"Supprimer l'adresse « {ligne.Libelle} » ? Les candidatures qui y sont rattachées sont conservées, sans adresse.",
            "Supprimer",
            "Annuler");
        if (!confirme)
        {
            return;
        }

        try
        {
            await serviceAdresses.SupprimerAsync(ligne.Identifiant);
            await RechargerAsync();
        }
        catch (Exception exception)
        {
            MessageErreur = $"Suppression impossible : {exception.Message}";
        }
    }

    private Task RechargerAsync() => rechargerPageParente?.Invoke() ?? Task.CompletedTask;
}
