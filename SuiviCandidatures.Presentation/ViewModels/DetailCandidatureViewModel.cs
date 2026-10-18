using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SuiviCandidatures.Coeur.Extensions;
using SuiviCandidatures.Coeur.Modeles;
using SuiviCandidatures.Coeur.Modeles.Enumerations;
using SuiviCandidatures.Coeur.Services;
using SuiviCandidatures.Coeur.Transferts;
using SuiviCandidatures.Presentation.Services;

namespace SuiviCandidatures.Presentation.ViewModels;

[QueryProperty(nameof(IdentifiantCandidature), NomsRoutes.ParametreIdentifiant)]
public partial class DetailCandidatureViewModel(
    IServiceSuiviCandidature serviceSuivi,
    IServiceParametres serviceParametres,
    IServiceNavigation navigation,
    IServiceDialogues dialogues,
    OffresCandidatureViewModel offres,
    ContactsCandidatureViewModel contacts) : ViewModelDeBase
{
    private static readonly CultureInfo Francais = CultureInfo.GetCultureInfo("fr-FR");

    private StatutCandidature statutActuel;
    private int identifiantEntreprise;
    private int identifiantActionEnEdition;

    public override string Titre => string.IsNullOrEmpty(TitreOffre) ? "Détail de la candidature" : TitreOffre;

    public IReadOnlyList<ChoixEnumeration> StatutsDisponibles { get; } = ChoixEnumeration.Pour<StatutCandidature>();

    public IReadOnlyList<ChoixEnumeration> TypesActionDisponibles { get; } =
        Enum.GetValues<TypeAction>()
            .Where(type => type.EstManuelle())
            .Select(type => new ChoixEnumeration(type, type.ObtenirLibelle()))
            .ToList();

    public ObservableCollection<LigneAction> Actions { get; } = [];

    public OffresCandidatureViewModel Offres { get; } = offres;

    public ContactsCandidatureViewModel Contacts { get; } = contacts;

    // ----- En-tête et informations -----

    [ObservableProperty]
    private int identifiantCandidature;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Titre))]
    private string titreOffre = string.Empty;

    [ObservableProperty]
    private string nomEntreprise = string.Empty;

    [ObservableProperty]
    private string statutLibelle = string.Empty;

    [ObservableProperty]
    private Color couleurStatut = Colors.Gray;

    [ObservableProperty]
    private string dateAffichee = string.Empty;

    [ObservableProperty]
    private string typeCandidatureLibelle = string.Empty;

    [ObservableProperty]
    private string typeContratLibelle = string.Empty;

    [ObservableProperty]
    private string adresseAffichee = string.Empty;

    [ObservableProperty]
    private string commentairesAffiches = string.Empty;

    [ObservableProperty]
    private string messageErreur = string.Empty;

    // ----- Changement de statut -----

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ChangerStatutCommand))]
    private ChoixEnumeration? statutSelectionne;

    // ----- Formulaire d'action -----

    [ObservableProperty]
    private bool formulaireActionVisible;

    [ObservableProperty]
    private string titreFormulaireAction = string.Empty;

    [ObservableProperty]
    private ChoixEnumeration? typeActionSelectionne;

    [ObservableProperty]
    private DateTime dateAction = DateTime.Today;

    [ObservableProperty]
    private TimeSpan heureAction;

    [ObservableProperty]
    private string noteAction = string.Empty;

    [ObservableProperty]
    private string messageErreurAction = string.Empty;

    // ----- Relance -----

    [ObservableProperty]
    private string libelleRelance = string.Empty;

    [ObservableProperty]
    private bool relanceEnRetard;

    [ObservableProperty]
    private string delaiRelanceSaisi = string.Empty;

    [ObservableProperty]
    private string indicationDelaiRelance = string.Empty;

    [ObservableProperty]
    private bool relancesDesactivees;

    [ObservableProperty]
    private string messageRelance = string.Empty;

    [RelayCommand]
    private async Task ChargerAsync()
    {
        try
        {
            Parametres parametres = await serviceParametres.ObtenirAsync();
            DetailCandidature? detail = await serviceSuivi.ObtenirDetailAsync(IdentifiantCandidature);
            if (detail is null)
            {
                MessageErreur = "Cette candidature est introuvable.";
                return;
            }

            identifiantEntreprise = detail.IdentifiantEntreprise;
            statutActuel = detail.Statut;

            TitreOffre = detail.TitreOffre;
            NomEntreprise = detail.NomEntreprise;
            StatutLibelle = detail.Statut.ObtenirLibelle();
            CouleurStatut = PaletteStatuts.ObtenirCouleur(detail.Statut);
            DateAffichee = detail.DateCandidature.ToString("d", Francais);
            TypeCandidatureLibelle = detail.TypeCandidature.ObtenirLibelle();
            TypeContratLibelle = detail.TypeContrat.ObtenirLibelle();
            AdresseAffichee = detail.LibelleAdresse ?? "Non renseignée";
            CommentairesAffiches = string.IsNullOrWhiteSpace(detail.Commentaires) ? "Aucun" : detail.Commentaires;
            StatutSelectionne = StatutsDisponibles.First(choix => Equals(choix.Valeur, detail.Statut));

            Actions.Clear();
            foreach (ElementAction action in detail.Actions)
            {
                Actions.Add(LigneAction.Depuis(action));
            }

            DelaiRelanceSaisi = detail.DelaiRelanceJours?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            IndicationDelaiRelance = $"Par défaut : {parametres.DelaiRelanceParDefautJours} jours";
            RelancesDesactivees = detail.RelancesDesactivees;
            MettreAJourRelance(CalculRelance.Calculer(detail, parametres.DelaiRelanceParDefautJours, DateTime.Today));

            Offres.Charger(detail.Identifiant, detail.Offres, ChargerAsync);
            await Contacts.ChargerAsync(detail.Identifiant, detail.Contacts, ChargerAsync);

            MessageErreur = string.Empty;
        }
        catch (Exception exception)
        {
            MessageErreur = $"Chargement impossible : {exception.Message}";
        }
    }

    [RelayCommand]
    private Task ModifierAsync() => navigation.AllerAsync(NomsRoutes.FormulaireCandidature, IdentifiantCandidature);

    [RelayCommand]
    private Task OuvrirEntrepriseAsync() => navigation.AllerAsync(NomsRoutes.DetailEntreprise, identifiantEntreprise);

    [RelayCommand]
    private async Task SupprimerAsync()
    {
        bool confirme = await dialogues.ConfirmerAsync(
            "Supprimer la candidature",
            $"Supprimer définitivement « {TitreOffre} » avec son historique et ses offres ? Cette action est irréversible.",
            "Supprimer",
            "Annuler");
        if (!confirme)
        {
            return;
        }

        try
        {
            await serviceSuivi.SupprimerAsync(IdentifiantCandidature);
            await navigation.RetourAsync();
        }
        catch (Exception exception)
        {
            MessageErreur = $"Suppression impossible : {exception.Message}";
        }
    }

    // ----- Statut -----

    [RelayCommand(CanExecute = nameof(PeutChangerStatut))]
    private async Task ChangerStatutAsync()
    {
        if (StatutSelectionne?.Valeur is not StatutCandidature nouveauStatut)
        {
            return;
        }

        try
        {
            await serviceSuivi.ChangerStatutAsync(IdentifiantCandidature, nouveauStatut);
            await ChargerAsync();
        }
        catch (Exception exception)
        {
            MessageErreur = $"Changement de statut impossible : {exception.Message}";
        }
    }

    private bool PeutChangerStatut() => StatutSelectionne?.Valeur is StatutCandidature statut && statut != statutActuel;

    // ----- Actions -----

    [RelayCommand]
    private void AjouterAction()
    {
        identifiantActionEnEdition = 0;
        TitreFormulaireAction = "Nouvelle action";
        TypeActionSelectionne = TypesActionDisponibles[0];
        DateAction = DateTime.Today;
        HeureAction = TimeSpan.FromMinutes(Math.Floor(DateTime.Now.TimeOfDay.TotalMinutes));
        NoteAction = string.Empty;
        MessageErreurAction = string.Empty;
        FormulaireActionVisible = true;
    }

    [RelayCommand]
    private void ModifierAction(LigneAction ligne)
    {
        identifiantActionEnEdition = ligne.Identifiant;
        TitreFormulaireAction = "Modifier l'action";
        TypeActionSelectionne = TypesActionDisponibles.First(choix => Equals(choix.Valeur, ligne.Type));
        DateAction = ligne.DateHeure.Date;
        HeureAction = ligne.DateHeure.TimeOfDay;
        NoteAction = ligne.Note ?? string.Empty;
        MessageErreurAction = string.Empty;
        FormulaireActionVisible = true;
    }

    [RelayCommand]
    private void AnnulerAction() => FormulaireActionVisible = false;

    [RelayCommand]
    private async Task EnregistrerActionAsync()
    {
        MessageErreurAction = string.Empty;

        DonneesAction donnees = new()
        {
            Identifiant = identifiantActionEnEdition,
            IdentifiantCandidature = IdentifiantCandidature,
            Type = TypeActionSelectionne?.Valeur is TypeAction type ? type : TypeAction.RelanceParMail,
            Date = DateAction.Date + HeureAction,
            Note = NoteAction
        };

        try
        {
            await serviceSuivi.EnregistrerActionAsync(donnees);
            FormulaireActionVisible = false;
            await ChargerAsync();
        }
        catch (ExceptionValidationMetier exception)
        {
            MessageErreurAction = exception.Message;
        }
        catch (Exception exception)
        {
            MessageErreurAction = $"Enregistrement impossible : {exception.Message}";
        }
    }

    [RelayCommand]
    private async Task SupprimerActionAsync(LigneAction ligne)
    {
        bool confirme = await dialogues.ConfirmerAsync(
            "Supprimer l'action",
            $"Supprimer l'action « {ligne.TypeLibelle} » du {ligne.DateAffichee} ?",
            "Supprimer",
            "Annuler");
        if (!confirme)
        {
            return;
        }

        try
        {
            await serviceSuivi.SupprimerActionAsync(ligne.Identifiant);
            await ChargerAsync();
        }
        catch (Exception exception)
        {
            MessageErreur = $"Suppression impossible : {exception.Message}";
        }
    }

    // ----- Relance -----

    [RelayCommand]
    private async Task EnregistrerRelanceAsync()
    {
        MessageRelance = string.Empty;

        int? delai = null;
        if (!string.IsNullOrWhiteSpace(DelaiRelanceSaisi))
        {
            if (!int.TryParse(DelaiRelanceSaisi.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out int valeur))
            {
                MessageRelance = "Le délai doit être un nombre entier de jours.";
                return;
            }

            delai = valeur;
        }

        try
        {
            await serviceSuivi.EnregistrerParametresRelanceAsync(IdentifiantCandidature, delai, RelancesDesactivees);
            await ChargerAsync();
            MessageRelance = "Enregistré.";
        }
        catch (ExceptionValidationMetier exception)
        {
            MessageRelance = exception.Message;
        }
        catch (Exception exception)
        {
            MessageRelance = $"Enregistrement impossible : {exception.Message}";
        }
    }

    private void MettreAJourRelance(EtatRelance etat)
    {
        RelanceEnRetard = etat.EstActive && etat.JoursRestants < 0;

        if (!etat.EstActive)
        {
            LibelleRelance = etat.Raison == RaisonRelanceInactive.StatutTerminal
                ? "Relances désactivées : la candidature est terminée."
                : "Relances désactivées pour cette candidature.";
            return;
        }

        string date = etat.DateRelance!.Value.ToString("d", Francais);
        int jours = etat.JoursRestants!.Value;
        string echeance = FormatageRelance.DecrireEcheance(jours);

        LibelleRelance = $"Prochaine relance : {date} ({echeance})";
    }
}
