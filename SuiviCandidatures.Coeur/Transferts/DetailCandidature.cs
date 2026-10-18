using SuiviCandidatures.Coeur.Modeles.Enumerations;

namespace SuiviCandidatures.Coeur.Transferts;

public sealed record DetailCandidature(
    int Identifiant,
    string TitreOffre,
    int IdentifiantEntreprise,
    string NomEntreprise,
    DateTime DateCandidature,
    StatutCandidature Statut,
    TypeCandidature TypeCandidature,
    TypeContrat TypeContrat,
    string? LibelleAdresse,
    string? Commentaires,
    int? DelaiRelanceJours,
    bool RelancesDesactivees,
    IReadOnlyList<ElementAction> Actions,
    IReadOnlyList<ElementOffre> Offres,
    IReadOnlyList<ElementContact> Contacts);
