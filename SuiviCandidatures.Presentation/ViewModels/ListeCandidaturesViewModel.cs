using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SuiviCandidatures.Coeur.Modeles;
using SuiviCandidatures.Coeur.Modeles.Enumerations;
using SuiviCandidatures.Coeur.Services;
using SuiviCandidatures.Coeur.Transferts;
using SuiviCandidatures.Presentation.Services;

namespace SuiviCandidatures.Presentation.ViewModels;

public partial class ListeCandidaturesViewModel : ViewModelDeBase
{
    private const int DelaiRechercheMillisecondes = 300;

    private readonly IServiceCandidatures serviceCandidatures;
    private readonly IServiceParametres serviceParametres;
    private readonly IServiceNavigation navigation;

    private int taillePage = Parametres.TaillesPageAutorisees[0];
    private int versionChargement;
    private CancellationTokenSource? rechercheEnAttente;

    public ListeCandidaturesViewModel(
        IServiceCandidatures serviceCandidatures,
        IServiceParametres serviceParametres,
        IServiceNavigation navigation)
    {
        this.serviceCandidatures = serviceCandidatures;
        this.serviceParametres = serviceParametres;
        this.navigation = navigation;

        StatutsFiltre = [new ChoixEnumeration(null, "Tous les statuts"), .. ChoixEnumeration.Pour<StatutCandidature>()];
        TrisDisponibles = ChoixEnumeration.Pour<CritereTriCandidatures>();

        // Affectation directe des champs : pas de rechargement avant le premier affichage.
        statutFiltreSelectionne = StatutsFiltre[0];
        triSelectionne = TrisDisponibles[0];
    }

    public override string Titre => "Candidatures";

    public ObservableCollection<LigneCandidature> Lignes { get; } = [];

    public IReadOnlyList<ChoixEnumeration> StatutsFiltre { get; }

    public IReadOnlyList<ChoixEnumeration> TrisDisponibles { get; }

    [ObservableProperty]
    private string texteRecherche = string.Empty;

    [ObservableProperty]
    private ChoixEnumeration? statutFiltreSelectionne;

    [ObservableProperty]
    private ChoixEnumeration? triSelectionne;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LibelleSens))]
    private bool triDecroissant = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LibellePagination))]
    [NotifyCanExecuteChangedFor(nameof(PagePrecedenteCommand))]
    [NotifyCanExecuteChangedFor(nameof(PageSuivanteCommand))]
    private int numeroPage = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LibellePagination))]
    [NotifyCanExecuteChangedFor(nameof(PageSuivanteCommand))]
    private int nombrePages = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LibellePagination))]
    private int nombreTotal;

    [ObservableProperty]
    private bool aucunResultat;

    [ObservableProperty]
    private string messageErreur = string.Empty;

    public string LibelleSens => TriDecroissant ? "↓ Décroissant" : "↑ Croissant";

    public string LibellePagination => $"Page {NumeroPage} sur {NombrePages} · {NombreTotal} résultat(s)";

    partial void OnTexteRechercheChanged(string value) => PlanifierRechargement();

    partial void OnStatutFiltreSelectionneChanged(ChoixEnumeration? value) => _ = RechargerDepuisPremierePageAsync();

    partial void OnTriSelectionneChanged(ChoixEnumeration? value) => _ = RechargerDepuisPremierePageAsync();

    partial void OnTriDecroissantChanged(bool value) => _ = RechargerDepuisPremierePageAsync();

    [RelayCommand]
    private async Task ChargerAsync()
    {
        try
        {
            Parametres parametres = await serviceParametres.ObtenirAsync();
            taillePage = parametres.TaillePage;
            await ChargerPageAsync();
        }
        catch (Exception exception)
        {
            MessageErreur = $"Chargement impossible : {exception.Message}";
        }
    }

    [RelayCommand]
    private void InverserTri() => TriDecroissant = !TriDecroissant;

    [RelayCommand(CanExecute = nameof(PeutAllerPagePrecedente))]
    private async Task PagePrecedenteAsync()
    {
        NumeroPage--;
        await ChargerPageAvecGestionErreurAsync();
    }

    [RelayCommand(CanExecute = nameof(PeutAllerPageSuivante))]
    private async Task PageSuivanteAsync()
    {
        NumeroPage++;
        await ChargerPageAvecGestionErreurAsync();
    }

    [RelayCommand]
    private Task AjouterAsync() => navigation.AllerAsync(NomsRoutes.FormulaireCandidature);

    [RelayCommand]
    private Task OuvrirAsync(LigneCandidature ligne) => navigation.AllerAsync(NomsRoutes.DetailCandidature, ligne.Identifiant);

    private bool PeutAllerPagePrecedente() => NumeroPage > 1;

    private bool PeutAllerPageSuivante() => NumeroPage < NombrePages;

    private void PlanifierRechargement()
    {
        rechercheEnAttente?.Cancel();
        CancellationTokenSource source = new();
        rechercheEnAttente = source;
        _ = AttendreEtRechargerAsync(source.Token);
    }

    private async Task AttendreEtRechargerAsync(CancellationToken jeton)
    {
        try
        {
            await Task.Delay(DelaiRechercheMillisecondes, jeton);
            await RechargerDepuisPremierePageAsync();
        }
        catch (OperationCanceledException)
        {
            // Une saisie plus récente a remplacé celle-ci.
        }
    }

    private Task RechargerDepuisPremierePageAsync()
    {
        NumeroPage = 1;
        return ChargerPageAvecGestionErreurAsync();
    }

    private async Task ChargerPageAvecGestionErreurAsync()
    {
        try
        {
            await ChargerPageAsync();
        }
        catch (Exception exception)
        {
            MessageErreur = $"Chargement impossible : {exception.Message}";
        }
    }

    private async Task ChargerPageAsync()
    {
        int version = ++versionChargement;

        CritereRechercheCandidatures critere = new()
        {
            TexteRecherche = TexteRecherche,
            Statut = StatutFiltreSelectionne?.Valeur is StatutCandidature statut ? statut : null,
            Tri = TriSelectionne?.Valeur is CritereTriCandidatures tri ? tri : CritereTriCandidatures.Date,
            Decroissant = TriDecroissant,
            NumeroPage = NumeroPage,
            TaillePage = taillePage
        };

        ResultatPagine<ElementListeCandidature> resultat = await serviceCandidatures.RechercherAsync(critere);

        // Une recherche plus récente a été lancée entre-temps : ce résultat est périmé.
        if (version != versionChargement)
        {
            return;
        }

        Lignes.Clear();
        foreach (ElementListeCandidature element in resultat.Elements)
        {
            Lignes.Add(LigneCandidature.Depuis(element));
        }

        NombreTotal = resultat.NombreTotal;
        NombrePages = resultat.NombrePages;
        NumeroPage = resultat.NumeroPage;
        AucunResultat = resultat.NombreTotal == 0;
        MessageErreur = string.Empty;
    }
}
