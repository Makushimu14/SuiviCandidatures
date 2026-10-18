using System.Globalization;
using SuiviCandidatures.Coeur.Extensions;
using SuiviCandidatures.Coeur.Transferts;

namespace SuiviCandidatures.Presentation.ViewModels;

public sealed record LigneCandidature(
    int Identifiant,
    string TitreOffre,
    string NomEntreprise,
    string DateAffichee,
    string TypeContratLibelle,
    string StatutLibelle,
    Color CouleurStatut)
{
    private static readonly CultureInfo Francais = CultureInfo.GetCultureInfo("fr-FR");

    public static LigneCandidature Depuis(ElementListeCandidature element) => new(
        element.Identifiant,
        element.TitreOffre,
        element.NomEntreprise,
        element.DateCandidature.ToString("d", Francais),
        element.TypeContrat.ObtenirLibelle(),
        element.Statut.ObtenirLibelle(),
        PaletteStatuts.ObtenirCouleur(element.Statut));
}
