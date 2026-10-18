using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SuiviCandidatures.Coeur.Modeles.Enumerations;
using SuiviCandidatures.Coeur.Services;
using SuiviCandidatures.Coeur.Transferts;
using SuiviCandidatures.Presentation.Services;

namespace SuiviCandidatures.Presentation.ViewModels;

[QueryProperty(nameof(IdentifiantCandidature), NomsRoutes.ParametreIdentifiant)]
public partial class FormulaireCandidatureViewModel(
    IServiceCandidatures serviceCandidatures,
    IServiceEntreprises serviceEntreprises,
    IServiceNavigation navigation) : ViewModelDeBase
{
    private const int NombreMaximumSuggestions = 6;

    private IReadOnlyList<ResumeEntreprise> toutesEntreprises = [];
    private bool initialise;
    private bool miseAJourEntrepriseSuspendue;

    public override string Titre => IdentifiantCandidature == 0 ? "Nouvelle candidature" : "Modifier la candidature";

    public IReadOnlyList<ChoixEnumeration> StatutsDisponibles { get; } = ChoixEnumeration.Pour<StatutCandidature>();

    public IReadOnlyList<ChoixEnumeration> TypesCandidatureDisponibles { get; } = ChoixEnumeration.Pour<TypeCandidature>();

    public IReadOnlyList<ChoixEnumeration> TypesContratDisponibles { get; } = ChoixEnumeration.Pour<TypeContrat>();

    public ObservableCollection<ResumeEntreprise> Suggestions { get; } = [];

    // ----- Candidature -----

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Titre))]
    private int identifiantCandidature;

    [ObservableProperty]
    private string titreOffre = string.Empty;

    [ObservableProperty]
    private DateTime dateCandidature = DateTime.Today;

    [ObservableProperty]
    private ChoixEnumeration? statutSelectionne;

    [ObservableProperty]
    private ChoixEnumeration? typeCandidatureSelectionne;

    [ObservableProperty]
    private ChoixEnumeration? typeContratSelectionne;

    [ObservableProperty]
    private string commentaires = string.Empty;

    // ----- Entreprise -----

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EstNouvelleEntreprise))]
    [NotifyPropertyChangedFor(nameof(SaisieAdresseVisible))]
    [NotifyPropertyChangedFor(nameof(MessageEntreprise))]
    [NotifyPropertyChangedFor(nameof(MessageEntrepriseVisible))]
    private string nomEntreprise = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EstNouvelleEntreprise))]
    [NotifyPropertyChangedFor(nameof(SelecteurAdresseVisible))]
    [NotifyPropertyChangedFor(nameof(SaisieAdresseVisible))]
    [NotifyPropertyChangedFor(nameof(MessageEntreprise))]
    private ResumeEntreprise? entrepriseExistante;

    [ObservableProperty]
    private string siteWebEntreprise = string.Empty;

    [ObservableProperty]
    private string secteurEntreprise = string.Empty;

    [ObservableProperty]
    private string notesEntreprise = string.Empty;

    // ----- Adresse -----

    [ObservableProperty]
    private IReadOnlyList<ChoixAdresse> adressesDisponibles = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SaisieAdresseVisible))]
    private ChoixAdresse? adresseSelectionnee;

    [ObservableProperty]
    private string rueAdresse = string.Empty;

    [ObservableProperty]
    private string codePostalAdresse = string.Empty;

    [ObservableProperty]
    private string villeAdresse = string.Empty;

    [ObservableProperty]
    private string mailAdresse = string.Empty;

    [ObservableProperty]
    private string telephoneAdresse = string.Empty;

    [ObservableProperty]
    private string messageErreur = string.Empty;

    public bool EstNouvelleEntreprise => !string.IsNullOrWhiteSpace(NomEntreprise) && EntrepriseExistante is null;

    public bool SelecteurAdresseVisible => EntrepriseExistante is not null;

    public bool SaisieAdresseVisible =>
        EstNouvelleEntreprise || (EntrepriseExistante is not null && AdresseSelectionnee?.EstNouvelle == true);

    public bool SuggestionsVisibles => Suggestions.Count > 0;

    public bool MessageEntrepriseVisible => !string.IsNullOrEmpty(MessageEntreprise);

    public string MessageEntreprise
    {
        get
        {
            if (string.IsNullOrWhiteSpace(NomEntreprise))
            {
                return string.Empty;
            }

            return EntrepriseExistante is null
                ? "Nouvelle entreprise : renseignez ses informations ci-dessous."
                : "Entreprise déjà enregistrée.";
        }
    }

    partial void OnNomEntrepriseChanged(string value)
    {
        if (!miseAJourEntrepriseSuspendue)
        {
            _ = MettreAJourEntrepriseSecuriseAsync();
        }
    }

    [RelayCommand]
    private async Task ChargerAsync()
    {
        try
        {
            toutesEntreprises = await serviceEntreprises.ListerAsync();

            // La page peut réapparaître : on ne réinitialise le formulaire qu'une seule fois.
            if (initialise)
            {
                return;
            }

            initialise = true;

            if (IdentifiantCandidature == 0)
            {
                StatutSelectionne = Choisir(StatutsDisponibles, StatutCandidature.EnAttenteReponse);
                TypeCandidatureSelectionne = Choisir(TypesCandidatureDisponibles, TypeCandidature.ARepondreAUneOffre);
                TypeContratSelectionne = Choisir(TypesContratDisponibles, TypeContrat.ContratDureeIndeterminee);
                return;
            }

            await ChargerCandidatureExistanteAsync();
        }
        catch (Exception exception)
        {
            MessageErreur = $"Chargement impossible : {exception.Message}";
        }
    }

    [RelayCommand]
    private void ChoisirSuggestion(ResumeEntreprise suggestion) => NomEntreprise = suggestion.Nom;

    [RelayCommand]
    private async Task EnregistrerAsync()
    {
        MessageErreur = string.Empty;

        DonneesCandidature donnees = new()
        {
            Identifiant = IdentifiantCandidature,
            TitreOffre = TitreOffre,
            DateCandidature = DateCandidature,
            Statut = Valeur(StatutSelectionne, StatutCandidature.EnAttenteReponse),
            TypeCandidature = Valeur(TypeCandidatureSelectionne, TypeCandidature.ARepondreAUneOffre),
            TypeContrat = Valeur(TypeContratSelectionne, TypeContrat.ContratDureeIndeterminee),
            Commentaires = Commentaires,
            NomEntreprise = NomEntreprise,
            NouvelleEntreprise = EstNouvelleEntreprise
                ? new DonneesNouvelleEntreprise(SiteWebEntreprise, SecteurEntreprise, NotesEntreprise)
                : null,
            IdentifiantAdresse = EntrepriseExistante is not null ? AdresseSelectionnee?.Identifiant : null,
            NouvelleAdresse = SaisieAdresseVisible
                ? new DonneesAdresse(RueAdresse, CodePostalAdresse, VilleAdresse, MailAdresse, TelephoneAdresse)
                : null
        };

        try
        {
            await serviceCandidatures.EnregistrerAsync(donnees);
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

    private async Task ChargerCandidatureExistanteAsync()
    {
        DonneesCandidature? donnees = await serviceCandidatures.ObtenirPourModificationAsync(IdentifiantCandidature);
        if (donnees is null)
        {
            MessageErreur = "Cette candidature est introuvable.";
            return;
        }

        TitreOffre = donnees.TitreOffre;
        DateCandidature = donnees.DateCandidature;
        StatutSelectionne = Choisir(StatutsDisponibles, donnees.Statut);
        TypeCandidatureSelectionne = Choisir(TypesCandidatureDisponibles, donnees.TypeCandidature);
        TypeContratSelectionne = Choisir(TypesContratDisponibles, donnees.TypeContrat);
        Commentaires = donnees.Commentaires ?? string.Empty;

        // Le nom est posé sans déclencher la mise à jour automatique : on l'attend explicitement,
        // afin de pouvoir ensuite sélectionner l'adresse enregistrée.
        miseAJourEntrepriseSuspendue = true;
        NomEntreprise = donnees.NomEntreprise;
        miseAJourEntrepriseSuspendue = false;

        await MettreAJourEntrepriseAsync();

        AdresseSelectionnee =
            AdressesDisponibles.FirstOrDefault(a => donnees.IdentifiantAdresse is not null && a.Identifiant == donnees.IdentifiantAdresse)
            ?? AdressesDisponibles.FirstOrDefault();
    }

    private async Task MettreAJourEntrepriseSecuriseAsync()
    {
        try
        {
            await MettreAJourEntrepriseAsync();
        }
        catch (Exception exception)
        {
            MessageErreur = $"Recherche de l'entreprise impossible : {exception.Message}";
        }
    }

    private async Task MettreAJourEntrepriseAsync()
    {
        string nom = NomEntreprise.Trim();
        ResumeEntreprise? trouvee = CorrespondanceNomsEntreprises.TrouverExacte(toutesEntreprises, nom);

        MettreAJourSuggestions(nom, trouvee);

        if (Equals(trouvee, EntrepriseExistante))
        {
            return;
        }

        EntrepriseExistante = trouvee;
        await ChargerAdressesAsync(trouvee);
    }

    private void MettreAJourSuggestions(string nom, ResumeEntreprise? trouvee)
    {
        Suggestions.Clear();

        // Aucune suggestion quand le nom correspond déjà exactement à une entreprise.
        if (trouvee is null)
        {
            foreach (ResumeEntreprise suggestion in
                CorrespondanceNomsEntreprises.Suggerer(toutesEntreprises, nom, NombreMaximumSuggestions))
            {
                Suggestions.Add(suggestion);
            }
        }

        OnPropertyChanged(nameof(SuggestionsVisibles));
    }

    private async Task ChargerAdressesAsync(ResumeEntreprise? entreprise)
    {
        if (entreprise is null)
        {
            AdressesDisponibles = [];
            AdresseSelectionnee = null;
            return;
        }

        IReadOnlyList<ResumeAdresse> adresses = await serviceEntreprises.ObtenirAdressesAsync(entreprise.Identifiant);

        // L'utilisateur a modifié le nom pendant le chargement : ce résultat est périmé.
        if (!Equals(entreprise, EntrepriseExistante))
        {
            return;
        }

        List<ChoixAdresse> choix =
        [
            new ChoixAdresse(null, "Aucune adresse"),
            .. adresses.Select(a => new ChoixAdresse(a.Identifiant, a.Libelle)),
            new ChoixAdresse(null, "Nouvelle adresse…", EstNouvelle: true)
        ];

        AdressesDisponibles = choix;
        AdresseSelectionnee = choix[0];
    }

    private static ChoixEnumeration Choisir(IReadOnlyList<ChoixEnumeration> liste, Enum valeur) =>
        liste.First(choix => Equals(choix.Valeur, valeur));

    private static TEnumeration Valeur<TEnumeration>(ChoixEnumeration? choix, TEnumeration parDefaut)
        where TEnumeration : struct, Enum =>
        choix?.Valeur is TEnumeration valeur ? valeur : parDefaut;
}
