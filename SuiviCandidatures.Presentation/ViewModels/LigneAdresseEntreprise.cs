using SuiviCandidatures.Coeur.Transferts;

namespace SuiviCandidatures.Presentation.ViewModels;

public sealed record LigneAdresseEntreprise(
    int Identifiant,
    string? Adresse,
    string? CodePostal,
    string? Ville,
    string? Mail,
    string? Telephone,
    string Libelle,
    string Coordonnees)
{
    public bool CoordonneesVisibles => !string.IsNullOrWhiteSpace(Coordonnees);

    public static LigneAdresseEntreprise Depuis(ElementAdresse adresse) => new(
        adresse.Identifiant,
        adresse.Adresse,
        adresse.CodePostal,
        adresse.Ville,
        adresse.Mail,
        adresse.Telephone,
        adresse.Libelle,
        string.Join(" · ", new[] { adresse.Mail, adresse.Telephone }.Where(v => !string.IsNullOrWhiteSpace(v))));
}
