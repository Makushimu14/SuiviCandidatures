using SuiviCandidatures.Coeur.Extensions;
using SuiviCandidatures.Coeur.Modeles.Enumerations;

namespace SuiviCandidatures.Coeur.Transferts;

public sealed record ElementAction(
    int Identifiant,
    DateTime Date,
    TypeAction Type,
    string? Note,
    StatutCandidature? AncienStatut,
    StatutCandidature? NouveauStatut)
{
    public bool EstModifiable => Type.EstManuelle();
}
