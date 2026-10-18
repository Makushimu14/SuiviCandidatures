using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using SuiviCandidatures.Coeur.Modeles.Enumerations;
using SuiviCandidatures.Coeur.Services;
using SuiviCandidatures.Coeur.Transferts;

namespace SuiviCandidatures.Presentation.ViewModels;

public partial class StatistiquesViewModel : ViewModelDeBase
{
    private const string Absent = "—";
    private const float TailleTexteGraphique = 12;
    private const double HauteurParSite = 34;
    private const double HauteurMinimaleGraphiqueSites = 140;

    private static readonly CultureInfo Francais = CultureInfo.GetCultureInfo("fr-FR");

    private static readonly SKColor CouleurPrincipale = SKColor.Parse("#2563EB");

    private static readonly SKColor[] PaletteCategories =
    [
        SKColor.Parse("#2563EB"),
        SKColor.Parse("#7C3AED"),
        SKColor.Parse("#15803D"),
        SKColor.Parse("#B45309"),
        SKColor.Parse("#DC2626"),
        SKColor.Parse("#0891B2")
    ];

    private readonly IServiceStatistiques serviceStatistiques;
    private int versionChargement;

    public StatistiquesViewModel(IServiceStatistiques serviceStatistiques)
    {
        this.serviceStatistiques = serviceStatistiques;

        PeriodesDisponibles = ChoixEnumeration.Pour<PeriodeStatistiques>();

        // Affectation directe du champ : pas de rechargement avant le premier affichage.
        periodeSelectionnee = PeriodesDisponibles[0];
    }

    public override string Titre => "Statistiques";

    public IReadOnlyList<ChoixEnumeration> PeriodesDisponibles { get; }

    public string TitreSemaines => $"Candidatures par semaine ({CalculStatistiques.NombreSemaines} dernières semaines)";

    [ObservableProperty]
    private ChoixEnumeration? periodeSelectionnee;

    [ObservableProperty]
    private string nombreCandidatures = "0";

    [ObservableProperty]
    private string tauxReponse = Absent;

    [ObservableProperty]
    private string detailTauxReponse = string.Empty;

    [ObservableProperty]
    private string delaiMoyenReponse = Absent;

    [ObservableProperty]
    private string detailDelaiMoyen = string.Empty;

    [ObservableProperty]
    private string tauxEntretien = Absent;

    [ObservableProperty]
    private string detailTauxEntretien = string.Empty;

    [ObservableProperty]
    private string nombreOffresRecues = "0";

    [ObservableProperty]
    private bool aucuneDonnee = true;

    [ObservableProperty]
    private string messageErreur = string.Empty;

    // ----- Graphiques LiveCharts -----

    [ObservableProperty]
    private ISeries[] seriesStatut = [];

    [ObservableProperty]
    private ISeries[] seriesTypeContrat = [];

    [ObservableProperty]
    private ISeries[] seriesTypeCandidature = [];

    [ObservableProperty]
    private ISeries[] seriesSites = [];

    [ObservableProperty]
    private Axis[] axesXSites = [];

    [ObservableProperty]
    private Axis[] axesYSites = [];

    [ObservableProperty]
    private double hauteurGraphiqueSites = HauteurMinimaleGraphiqueSites;

    [ObservableProperty]
    private ISeries[] seriesSemaines = [];

    [ObservableProperty]
    private Axis[] axesXSemaines = [];

    [ObservableProperty]
    private Axis[] axesYSemaines = [];

    [ObservableProperty]
    private SolidColorPaint peintureTexte = new(SKColors.Black);

    partial void OnPeriodeSelectionneeChanged(ChoixEnumeration? value) => _ = ChargerAvecGestionErreurAsync();

    [RelayCommand]
    private Task ChargerAsync() => ChargerAvecGestionErreurAsync();

    private async Task ChargerAvecGestionErreurAsync()
    {
        try
        {
            await ChargerStatistiquesAsync();
        }
        catch (Exception exception)
        {
            MessageErreur = $"Chargement impossible : {exception.Message}";
        }
    }

    private async Task ChargerStatistiquesAsync()
    {
        int version = ++versionChargement;

        PeriodeStatistiques periode = PeriodeSelectionnee?.Valeur is PeriodeStatistiques valeur
            ? valeur
            : PeriodeStatistiques.Toutes;

        Statistiques statistiques = await serviceStatistiques.CalculerAsync(periode, DateTime.Today);

        // Une période plus récente a été choisie entre-temps : ce résultat est périmé.
        if (version != versionChargement)
        {
            return;
        }

        AucuneDonnee = statistiques.NombreCandidatures == 0;
        NombreCandidatures = statistiques.NombreCandidatures.ToString(Francais);
        NombreOffresRecues = statistiques.NombreOffresRecues.ToString(Francais);

        TauxReponse = FormaterTaux(statistiques.TauxReponse);
        DetailTauxReponse = statistiques.TauxReponse is null
            ? string.Empty
            : $"{statistiques.NombreReponses} réponse(s) reçue(s)";

        TauxEntretien = FormaterTaux(statistiques.TauxEntretien);
        DetailTauxEntretien = statistiques.TauxEntretien is null
            ? string.Empty
            : $"{statistiques.NombreEntretiens} candidature(s) avec entretien";

        DelaiMoyenReponse = statistiques.DelaiMoyenReponseJours is { } delai
            ? $"{Math.Round(delai, 1).ToString(Francais)} jours"
            : Absent;
        DetailDelaiMoyen = statistiques.NombreDelaisMesures == 0
            ? "Aucune réponse datée dans l'historique"
            : $"Mesuré sur {statistiques.NombreDelaisMesures} candidature(s)";

        ConstruireGraphiques(statistiques);

        MessageErreur = string.Empty;
    }

    private void ConstruireGraphiques(Statistiques statistiques)
    {
        // Les couleurs des graphiques ne suivent pas automatiquement le thème : on les recalcule à chaque chargement.
        bool sombre = Application.Current?.RequestedTheme == AppTheme.Dark;
        SKColor texte = SKColor.Parse(sombre ? "#F1F5F9" : "#0F172A");
        SKColor grille = SKColor.Parse(sombre ? "#334155" : "#E2E8F0");

        PeintureTexte = new SolidColorPaint(texte);

        SeriesStatut = ConstruireSecteurs(statistiques.ParStatut, CouleurDuStatut);
        SeriesTypeContrat = ConstruireSecteurs(statistiques.ParTypeContrat, null);
        SeriesTypeCandidature = ConstruireSecteurs(statistiques.ParTypeCandidature, null);

        // Sites : barres horizontales, le site le plus fréquent en haut.
        SeriesSites =
        [
            new RowSeries<int>
            {
                Values = statistiques.ParSite.Select(p => p.Valeur).ToArray(),
                Fill = new SolidColorPaint(CouleurPrincipale),
                DataLabelsPaint = new SolidColorPaint(texte),
                DataLabelsSize = TailleTexteGraphique,
                DataLabelsPosition = DataLabelsPosition.End
            }
        ];
        AxesYSites =
        [
            new Axis
            {
                Labels = statistiques.ParSite.Select(p => p.Libelle).ToArray(),
                LabelsPaint = new SolidColorPaint(texte),
                TextSize = TailleTexteGraphique,
                IsInverted = true
            }
        ];
        AxesXSites =
        [
            new Axis
            {
                LabelsPaint = new SolidColorPaint(texte),
                TextSize = TailleTexteGraphique,
                MinLimit = 0,
                MinStep = 1,
                SeparatorsPaint = new SolidColorPaint(grille) { StrokeThickness = 1 }
            }
        ];
        HauteurGraphiqueSites = Math.Max(HauteurMinimaleGraphiqueSites, (statistiques.ParSite.Count * HauteurParSite) + 50);

        // Semaines : colonnes verticales.
        SeriesSemaines =
        [
            new ColumnSeries<int>
            {
                Values = statistiques.ParSemaine.Select(p => p.Valeur).ToArray(),
                Fill = new SolidColorPaint(CouleurPrincipale),
                DataLabelsPaint = new SolidColorPaint(texte),
                DataLabelsSize = TailleTexteGraphique,
                DataLabelsPosition = DataLabelsPosition.End
            }
        ];
        AxesXSemaines =
        [
            new Axis
            {
                Labels = statistiques.ParSemaine.Select(p => p.Libelle).ToArray(),
                LabelsPaint = new SolidColorPaint(texte),
                TextSize = TailleTexteGraphique
            }
        ];
        AxesYSemaines =
        [
            new Axis
            {
                LabelsPaint = new SolidColorPaint(texte),
                TextSize = TailleTexteGraphique,
                MinLimit = 0,
                MinStep = 1,
                SeparatorsPaint = new SolidColorPaint(grille) { StrokeThickness = 1 }
            }
        ];
    }

    /// <summary>Un secteur par catégorie, avec une couleur propre. Sans fonction de couleur, la palette générale est utilisée.</summary>
    private static ISeries[] ConstruireSecteurs(IReadOnlyList<PointStatistique> points, Func<PointStatistique, SKColor>? couleur) =>
        points
            .Select((point, indice) => (ISeries)new PieSeries<int>
            {
                Values = [point.Valeur],
                Name = point.Libelle,
                Fill = new SolidColorPaint(couleur?.Invoke(point) ?? PaletteCategories[indice % PaletteCategories.Length]),
                InnerRadius = 50,
                DataLabelsPaint = new SolidColorPaint(SKColors.White),
                DataLabelsSize = 14,
                DataLabelsPosition = PolarLabelsPosition.Middle
            })
            .ToArray();

    private static SKColor CouleurDuStatut(PointStatistique point)
    {
        if (!Enum.TryParse(point.Cle, out StatutCandidature statut))
        {
            return CouleurPrincipale;
        }

        Color couleur = PaletteStatuts.ObtenirCouleur(statut);
        return new SKColor(
            (byte)Math.Round(couleur.Red * 255),
            (byte)Math.Round(couleur.Green * 255),
            (byte)Math.Round(couleur.Blue * 255));
    }

    private static string FormaterTaux(double? taux) => taux is { } valeur ? valeur.ToString("P0", Francais) : Absent;
}
