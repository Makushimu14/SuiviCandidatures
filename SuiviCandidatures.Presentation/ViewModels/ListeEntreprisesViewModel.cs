using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SuiviCandidatures.Coeur.Modeles;
using SuiviCandidatures.Coeur.Services;
using SuiviCandidatures.Coeur.Transferts;
using SuiviCandidatures.Presentation.Services;

namespace SuiviCandidatures.Presentation.ViewModels;

public partial class ListeEntreprisesViewModel : ViewModelDeBase
{
    private const int DelaiRechercheMillisecondes = 300;

    private readonly IServiceEntreprises serviceEntreprises;
    private readonly IServiceParametres serviceParametres;
    private readonly IServiceNavigation navigation;

    private int taillePage = Parametres.TaillesPageAutorisees[0];
    private int versionChargement;
    private CancellationTokenSource? rechercheEnAttente;

    public ListeEntreprisesViewModel(
        IServiceEntreprises serviceEntreprises,
        IServiceParametres serviceParametres,
        IServiceNavigation navigation)
    {
        this.serviceEntreprises = serviceEntreprises;
        this.serviceParametres = serviceParametres;
        this.navigation = navigation;

        TrisDisponibles = ChoixEnumeration.Pour<CritereTriEntreprises>();

        // Affectation directe du champ : pas de rechargement avant le premier affichage.
        triSelectionne = TrisDisponibles[0];
    }

    public override string Titre => "Entreprises";

    public ObservableCollection<LigneEntreprise> Lignes { get; } = [];

    public IReadOnlyList<ChoixEnumeration> TrisDisponibles { get; }

    [ObservableProperty]
    private string texteRecherche = string.Empty;

    [ObservableProperty]
    private ChoixEnumeration? triSelectionne;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LibelleSens))]
    private bool triDecroissant;

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
    private Task AjouterAsync() => navigation.AllerAsync(NomsRoutes.FormulaireEntreprise);

    [RelayCommand]
    private Task OuvrirAsync(LigneEntreprise ligne) => navigation.AllerAsync(NomsRoutes.DetailEntreprise, ligne.Identifiant);

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

        CritereRechercheEntreprises critere = new()
        {
            TexteRecherche = TexteRecherche,
            Tri = TriSelectionne?.Valeur is CritereTriEntreprises tri ? tri : CritereTriEntreprises.Nom,
            Decroissant = TriDecroissant,
            NumeroPage = NumeroPage,
            TaillePage = taillePage
        };

        ResultatPagine<ElementListeEntreprise> resultat = await serviceEntreprises.RechercherAsync(critere);

        // Une recherche plus récente a été lancée entre-temps : ce résultat est périmé.
        if (version != versionChargement)
        {
            return;
        }

        Lignes.Clear();
        foreach (ElementListeEntreprise element in resultat.Elements)
        {
            Lignes.Add(LigneEntreprise.Depuis(element));
        }

        NombreTotal = resultat.NombreTotal;
        NombrePages = resultat.NombrePages;
        NumeroPage = resultat.NumeroPage;
        AucunResultat = resultat.NombreTotal == 0;
        MessageErreur = string.Empty;
    }
}
