namespace SuiviCandidatures.Coeur.Transferts;

/// <summary>
/// Les taux et le délai valent null lorsqu'il n'y a aucune donnée pour les calculer.
/// Les candidatures retirées sont exclues des taux.
/// </summary>
public sealed record Statistiques(
    int NombreCandidatures,
    int NombreReponses,
    double? TauxReponse,
    int NombreEntretiens,
    double? TauxEntretien,
    int NombreOffresRecues,
    double? DelaiMoyenReponseJours,
    int NombreDelaisMesures,
    IReadOnlyList<PointStatistique> ParStatut,
    IReadOnlyList<PointStatistique> ParTypeContrat,
    IReadOnlyList<PointStatistique> ParTypeCandidature,
    IReadOnlyList<PointStatistique> ParSite,
    IReadOnlyList<PointStatistique> ParSemaine);
