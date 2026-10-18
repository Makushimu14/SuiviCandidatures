using SuiviCandidatures.Coeur.Transferts;

namespace SuiviCandidatures.Presentation.ViewModels;

public sealed record LigneOffre(
    int Identifiant,
    string? NomSite,
    string? Reference,
    string? Lien,
    string TitreAffiche,
    string ReferenceAffichee)
{
    public bool ReferenceVisible => Reference is not null;

    public bool LienVisible => Lien is not null;

    public static LigneOffre Depuis(ElementOffre offre) => new(
        offre.Identifiant,
        offre.NomSite,
        offre.Reference,
        offre.Lien,
        offre.NomSite ?? "Site non précisé",
        offre.Reference is null ? string.Empty : $"Référence : {offre.Reference}");
}
