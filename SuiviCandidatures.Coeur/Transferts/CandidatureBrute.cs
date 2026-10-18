using SuiviCandidatures.Coeur.Modeles.Enumerations;

namespace SuiviCandidatures.Coeur.Transferts;

public sealed record ActionBrute(TypeAction Type, DateTime Date, StatutCandidature? NouveauStatut);

/// <summary>Données minimales d'une candidature pour calculer les statistiques, indépendantes de la base de données.</summary>
public sealed record CandidatureBrute(
    DateTime DateCandidature,
    StatutCandidature Statut,
    TypeContrat TypeContrat,
    TypeCandidature TypeCandidature,
    IReadOnlyList<ActionBrute> Actions,
    IReadOnlyList<string> Sites);
