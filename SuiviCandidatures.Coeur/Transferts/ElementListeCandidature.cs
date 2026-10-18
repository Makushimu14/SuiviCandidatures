using SuiviCandidatures.Coeur.Modeles.Enumerations;

namespace SuiviCandidatures.Coeur.Transferts;

public sealed record ElementListeCandidature(
    int Identifiant,
    string TitreOffre,
    string NomEntreprise,
    DateTime DateCandidature,
    TypeContrat TypeContrat,
    StatutCandidature Statut);
