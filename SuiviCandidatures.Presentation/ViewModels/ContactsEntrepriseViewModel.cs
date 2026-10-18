using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SuiviCandidatures.Coeur.Services;
using SuiviCandidatures.Coeur.Transferts;
using SuiviCandidatures.Presentation.Services;

namespace SuiviCandidatures.Presentation.ViewModels;

/// <summary>Carte « Contacts » de la page de détail d'une entreprise.</summary>
public partial class ContactsEntrepriseViewModel(
    IServiceContacts serviceContacts,
    IServiceDialogues dialogues) : ObservableObject
{
    private int identifiantEntreprise;
    private int identifiantContactEnEdition;
    private Func<Task>? rechargerPageParente;

    public ObservableCollection<LigneContactEntreprise> Contacts { get; } = [];

    public bool AucunContact => Contacts.Count == 0;

    [ObservableProperty]
    private bool formulaireVisible;

    [ObservableProperty]
    private string titreFormulaire = string.Empty;

    [ObservableProperty]
    private string nom = string.Empty;

    [ObservableProperty]
    private string prenom = string.Empty;

    [ObservableProperty]
    private string role = string.Empty;

    [ObservableProperty]
    private string mail = string.Empty;

    [ObservableProperty]
    private string telephone = string.Empty;

    [ObservableProperty]
    private string notes = string.Empty;

    [ObservableProperty]
    private string messageErreur = string.Empty;

    /// <summary>Appelée par la page de détail à chaque chargement. N'affecte pas le formulaire s'il est ouvert.</summary>
    public void Charger(int identifiantEntreprise, IReadOnlyList<ElementContactEntreprise> contacts, Func<Task> rechargerPageParente)
    {
        this.identifiantEntreprise = identifiantEntreprise;
        this.rechargerPageParente = rechargerPageParente;

        Contacts.Clear();
        foreach (ElementContactEntreprise contact in contacts)
        {
            Contacts.Add(LigneContactEntreprise.Depuis(contact));
        }

        OnPropertyChanged(nameof(AucunContact));
    }

    [RelayCommand]
    private void AjouterContact()
    {
        identifiantContactEnEdition = 0;
        TitreFormulaire = "Nouveau contact";
        Nom = Prenom = Role = Mail = Telephone = Notes = string.Empty;
        MessageErreur = string.Empty;
        FormulaireVisible = true;
    }

    [RelayCommand]
    private void ModifierContact(LigneContactEntreprise ligne)
    {
        identifiantContactEnEdition = ligne.Identifiant;
        TitreFormulaire = "Modifier le contact";
        Nom = ligne.Nom;
        Prenom = ligne.Prenom ?? string.Empty;
        Role = ligne.Role ?? string.Empty;
        Mail = ligne.Mail ?? string.Empty;
        Telephone = ligne.Telephone ?? string.Empty;
        Notes = ligne.Notes ?? string.Empty;
        MessageErreur = string.Empty;
        FormulaireVisible = true;
    }

    [RelayCommand]
    private void AnnulerContact() => FormulaireVisible = false;

    [RelayCommand]
    private async Task EnregistrerContactAsync()
    {
        MessageErreur = string.Empty;

        DonneesContactEntreprise donnees = new()
        {
            Identifiant = identifiantContactEnEdition,
            IdentifiantEntreprise = identifiantEntreprise,
            Nom = Nom,
            Prenom = Prenom,
            Role = Role,
            Mail = Mail,
            Telephone = Telephone,
            Notes = Notes
        };

        try
        {
            await serviceContacts.EnregistrerPourEntrepriseAsync(donnees);
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
    private async Task SupprimerContactAsync(LigneContactEntreprise ligne)
    {
        string consequence = ligne.NombreCandidatures == 0
            ? string.Empty
            : $" Il sera retiré de {ligne.NombreCandidatures} candidature(s).";

        bool confirme = await dialogues.ConfirmerAsync(
            "Supprimer le contact",
            $"Supprimer définitivement « {ligne.NomComplet} » ?{consequence}",
            "Supprimer",
            "Annuler");
        if (!confirme)
        {
            return;
        }

        try
        {
            await serviceContacts.SupprimerAsync(ligne.Identifiant);
            await RechargerAsync();
        }
        catch (Exception exception)
        {
            MessageErreur = $"Suppression impossible : {exception.Message}";
        }
    }

    private Task RechargerAsync() => rechargerPageParente?.Invoke() ?? Task.CompletedTask;
}
