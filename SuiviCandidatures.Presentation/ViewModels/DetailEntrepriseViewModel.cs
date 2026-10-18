using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SuiviCandidatures.Coeur.Services;
using SuiviCandidatures.Coeur.Transferts;
using SuiviCandidatures.Presentation.Services;

namespace SuiviCandidatures.Presentation.ViewModels;

[QueryProperty(nameof(IdentifiantEntreprise), NomsRoutes.ParametreIdentifiant)]
public partial class DetailEntrepriseViewModel(
    IServiceEntreprises serviceEntreprises,
    IServiceNavigation navigation,
    IServiceDialogues dialogues,
    IServiceLiensExternes liensExternes,
    AdressesEntrepriseViewModel adresses,
    ContactsEntrepriseViewModel contacts) : ViewModelDeBase
{
    private DetailEntreprise? detail;

    public override string Titre => string.IsNullOrEmpty(NomEntreprise) ? "Détail de l'entreprise" : NomEntreprise;

    public ObservableCollection<LigneCandidature> Candidatures { get; } = [];

    public AdressesEntrepriseViewModel Adresses { get; } = adresses;

    public ContactsEntrepriseViewModel Contacts { get; } = contacts;

    public bool AucuneCandidature => Candidatures.Count == 0;

    public bool SiteWebAbsent => !SiteWebVisible;

    [ObservableProperty]
    private int identifiantEntreprise;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Titre))]
    private string nomEntreprise = string.Empty;

    [ObservableProperty]
    private string siteWebAffiche = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SiteWebAbsent))]
    private bool siteWebVisible;

    [ObservableProperty]
    private string secteurAffiche = string.Empty;

    [ObservableProperty]
    private string notesAffichees = string.Empty;

    [ObservableProperty]
    private string messageErreur = string.Empty;

    [RelayCommand]
    private async Task ChargerAsync()
    {
        try
        {
            DetailEntreprise? donnees = await serviceEntreprises.ObtenirDetailAsync(IdentifiantEntreprise);
            if (donnees is null)
            {
                MessageErreur = "Cette entreprise est introuvable.";
                return;
            }

            detail = donnees;

            NomEntreprise = donnees.Nom;
            SiteWebVisible = donnees.SiteWeb is not null;
            SiteWebAffiche = donnees.SiteWeb ?? string.Empty;
            SecteurAffiche = string.IsNullOrWhiteSpace(donnees.Secteur) ? "Non renseigné" : donnees.Secteur;
            NotesAffichees = string.IsNullOrWhiteSpace(donnees.Notes) ? "Aucune" : donnees.Notes;

            Candidatures.Clear();
            foreach (ElementListeCandidature candidature in donnees.Candidatures)
            {
                Candidatures.Add(LigneCandidature.Depuis(candidature));
            }

            OnPropertyChanged(nameof(AucuneCandidature));

            Adresses.Charger(donnees.Identifiant, donnees.Adresses, ChargerAsync);
            Contacts.Charger(donnees.Identifiant, donnees.Contacts, ChargerAsync);

            MessageErreur = string.Empty;
        }
        catch (Exception exception)
        {
            MessageErreur = $"Chargement impossible : {exception.Message}";
        }
    }

    [RelayCommand]
    private Task ModifierAsync() => navigation.AllerAsync(NomsRoutes.FormulaireEntreprise, IdentifiantEntreprise);

    [RelayCommand]
    private Task OuvrirCandidatureAsync(int identifiant) => navigation.AllerAsync(NomsRoutes.DetailCandidature, identifiant);

    [RelayCommand]
    private async Task OuvrirSiteAsync()
    {
        if (!SiteWebVisible)
        {
            return;
        }

        try
        {
            await liensExternes.OuvrirAsync(SiteWebAffiche);
        }
        catch (Exception exception)
        {
            MessageErreur = $"Ouverture du site impossible : {exception.Message}";
        }
    }

    [RelayCommand]
    private async Task SupprimerAsync()
    {
        int nombreCandidatures = detail?.Candidatures.Count ?? 0;
        int nombreContacts = detail?.Contacts.Count ?? 0;
        int nombreAdresses = detail?.Adresses.Count ?? 0;

        bool confirme = await dialogues.ConfirmerAsync(
            "Supprimer l'entreprise",
            $"Supprimer définitivement « {NomEntreprise} » ? Seront aussi supprimés : {nombreCandidatures} candidature(s) " +
            $"avec leur historique et leurs offres, {nombreContacts} contact(s) et {nombreAdresses} adresse(s). " +
            "Cette action est irréversible.",
            "Supprimer",
            "Annuler");
        if (!confirme)
        {
            return;
        }

        try
        {
            await serviceEntreprises.SupprimerAsync(IdentifiantEntreprise);
            await navigation.RetourAsync();
        }
        catch (Exception exception)
        {
            MessageErreur = $"Suppression impossible : {exception.Message}";
        }
    }
}
