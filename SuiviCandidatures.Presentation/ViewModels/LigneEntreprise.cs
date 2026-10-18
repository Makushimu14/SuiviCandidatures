using SuiviCandidatures.Coeur.Transferts;

namespace SuiviCandidatures.Presentation.ViewModels;

public sealed record LigneEntreprise(
    int Identifiant,
    string Nom,
    string Details,
    string NombreCandidaturesAffiche)
{
    public bool DetailsVisibles => !string.IsNullOrWhiteSpace(Details);

    public static LigneEntreprise Depuis(ElementListeEntreprise entreprise) => new(
        entreprise.Identifiant,
        entreprise.Nom,
        string.Join(" · ", new[] { entreprise.Secteur, entreprise.Villes }.Where(v => !string.IsNullOrWhiteSpace(v))),
        entreprise.NombreCandidatures switch
        {
            0 => "Aucune candidature",
            1 => "1 candidature",
            var nombre => $"{nombre} candidatures"
        });
}
