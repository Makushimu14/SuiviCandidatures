using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SuiviCandidatures.Coeur.Services;
using SuiviCandidatures.Coeur.Transferts;
using SuiviCandidatures.Presentation.Services;

namespace SuiviCandidatures.Presentation.ViewModels;

/// <summary>Carte « Contacts » de la page de détail d'une candidature.</summary>
public partial class ContactsCandidatureViewModel(
    IServiceContacts serviceContacts,
    IServiceDialogues dialogues) : ObservableObject
{
    private int identifiantCandidature;
    private int identifiantContactEnEdition;
    private Func<Task>? rechargerPageParente;

    public ObservableCollection<LigneContact> Contacts { get; } = [];

    public bool AucunContact => Contacts.Count == 0;

    public bool DisponiblesVisibles => Disponibles.Count > 0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisponiblesVisibles))]
    private IReadOnlyList<ElementContact> disponibles = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LierCommand))]
    private ElementContact? contactDisponibleSelectionne;

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
    public async Task ChargerAsync(int identifiantCandidature, IReadOnlyList<ElementContact> contacts, Func<Task> rechargerPageParente)
    {
        this.identifiantCandidature = identifiantCandidature;
        this.rechargerPageParente = rechargerPageParente;

        Contacts.Clear();
        foreach (ElementContact contact in contacts)
        {
            Contacts.Add(LigneContact.Depuis(contact));
        }

        OnPropertyChanged(nameof(AucunContact));

        try
        {
            Disponibles = await serviceContacts.ListerDisponiblesAsync(identifiantCandidature);
            ContactDisponibleSelectionne = null;
        }
        catch (Exception exception)
        {
            MessageErreur = $"Chargement des contacts impossible : {exception.Message}";
        }
    }

    [RelayCommand(CanExecute = nameof(PeutLier))]
    private async Task LierAsync()
    {
        if (ContactDisponibleSelectionne is null)
        {
            return;
        }

        try
        {
            await serviceContacts.LierAsync(identifiantCandidature, ContactDisponibleSelectionne.Identifiant);
            await RechargerAsync();
        }
        catch (ExceptionValidationMetier exception)
        {
            MessageErreur = exception.Message;
        }
        catch (Exception exception)
        {
            MessageErreur = $"Liaison impossible : {exception.Message}";
        }
    }

    private bool PeutLier() => ContactDisponibleSelectionne is not null;

    [RelayCommand]
    private async Task DelierAsync(LigneContact ligne)
    {
        bool confirme = await dialogues.ConfirmerAsync(
            "Délier le contact",
            $"Retirer « {ligne.NomComplet} » de cette candidature ? Le contact reste enregistré pour l'entreprise.",
            "Délier",
            "Annuler");
        if (!confirme)
        {
            return;
        }

        try
        {
            await serviceContacts.DelierAsync(identifiantCandidature, ligne.Identifiant);
            await RechargerAsync();
        }
        catch (Exception exception)
        {
            MessageErreur = $"Suppression de la liaison impossible : {exception.Message}";
        }
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
    private void ModifierContact(LigneContact ligne)
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

        DonneesContact donnees = new()
        {
            Identifiant = identifiantContactEnEdition,
            IdentifiantCandidature = identifiantCandidature,
            Nom = Nom,
            Prenom = Prenom,
            Role = Role,
            Mail = Mail,
            Telephone = Telephone,
            Notes = Notes
        };

        try
        {
            await serviceContacts.EnregistrerAsync(donnees);
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

    private Task RechargerAsync() => rechargerPageParente?.Invoke() ?? Task.CompletedTask;
}
