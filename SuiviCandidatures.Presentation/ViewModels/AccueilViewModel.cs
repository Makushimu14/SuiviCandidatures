using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SuiviCandidatures.Coeur.Extensions;
using SuiviCandidatures.Coeur.Modeles.Enumerations;
using SuiviCandidatures.Coeur.Services;
using SuiviCandidatures.Coeur.Transferts;
using SuiviCandidatures.Presentation.Services;

namespace SuiviCandidatures.Presentation.ViewModels;

public partial class AccueilViewModel(
    IServiceRelances serviceRelances,
    IServiceCandidatures serviceCandidatures,
    IServiceNavigation navigation) : ViewModelDeBase
{
    private const int NombreJoursAVenir = 7;
    private const int NombreCandidaturesRecentes = 5;

    public override string Titre => "Accueil";

    public ObservableCollection<LigneRelance> RelancesDues { get; } = [];

    public ObservableCollection<LigneRelance> RelancesAVenir { get; } = [];

    public ObservableCollection<LigneCandidature> CandidaturesRecentes { get; } = [];

    public string TitreRelancesAVenir => $"À venir (dans les {NombreJoursAVenir} jours)";

    [ObservableProperty]
    private int nombreRelancesDues;

    [ObservableProperty]
    private int nombreEnCours;

    [ObservableProperty]
    private int nombreEntretiensPlanifies;

    [ObservableProperty]
    private bool aucuneRelanceDue;

    [ObservableProperty]
    private bool aucuneRelanceAVenir;

    [ObservableProperty]
    private bool aucuneCandidature;

    [ObservableProperty]
    private string messageErreur = string.Empty;

    [RelayCommand]
    private async Task ChargerAsync()
    {
        try
        {
            DateTime aujourdhui = DateTime.Today;

            IReadOnlyList<ElementRelance> relances = await serviceRelances.ListerAsync(aujourdhui);
            ResultatPagine<ElementListeCandidature> recentes = await serviceCandidatures.RechercherAsync(
                new CritereRechercheCandidatures { NumeroPage = 1, TaillePage = NombreCandidaturesRecentes });
            IReadOnlyDictionary<StatutCandidature, int> comptes = await serviceCandidatures.CompterParStatutAsync();

            Remplacer(RelancesDues, relances.Where(r => r.EstDue).Select(LigneRelance.Depuis));
            Remplacer(
                RelancesAVenir,
                relances.Where(r => !r.EstDue && r.JoursRestants <= NombreJoursAVenir).Select(LigneRelance.Depuis));
            Remplacer(CandidaturesRecentes, recentes.Elements.Select(LigneCandidature.Depuis));

            NombreRelancesDues = RelancesDues.Count;
            NombreEnCours = comptes.Where(c => !c.Key.EstTerminal()).Sum(c => c.Value);
            NombreEntretiensPlanifies = comptes.GetValueOrDefault(StatutCandidature.EntretienPlanifie);

            AucuneRelanceDue = RelancesDues.Count == 0;
            AucuneRelanceAVenir = RelancesAVenir.Count == 0;
            AucuneCandidature = CandidaturesRecentes.Count == 0;

            MessageErreur = string.Empty;
        }
        catch (Exception exception)
        {
            MessageErreur = $"Chargement impossible : {exception.Message}";
        }
    }

    [RelayCommand]
    private Task AjouterCandidatureAsync() => navigation.AllerAsync(NomsRoutes.FormulaireCandidature);

    [RelayCommand]
    private Task VoirToutesAsync() => navigation.AllerALaRacineAsync(NomsRoutes.Candidatures);

    [RelayCommand]
    private Task OuvrirCandidatureAsync(int identifiant) => navigation.AllerAsync(NomsRoutes.DetailCandidature, identifiant);

    private static void Remplacer<TLigne>(ObservableCollection<TLigne> collection, IEnumerable<TLigne> lignes)
    {
        collection.Clear();
        foreach (TLigne ligne in lignes)
        {
            collection.Add(ligne);
        }
    }
}
