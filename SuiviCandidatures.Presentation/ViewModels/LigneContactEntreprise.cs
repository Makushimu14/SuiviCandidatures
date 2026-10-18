using SuiviCandidatures.Coeur.Transferts;

namespace SuiviCandidatures.Presentation.ViewModels;

public sealed record LigneContactEntreprise(
    int Identifiant,
    string Nom,
    string? Prenom,
    string? Role,
    string? Mail,
    string? Telephone,
    string? Notes,
    int NombreCandidatures,
    string NomComplet,
    string Details)
{
    public bool DetailsVisibles => !string.IsNullOrWhiteSpace(Details);

    public static LigneContactEntreprise Depuis(ElementContactEntreprise contact)
    {
        string candidatures = contact.NombreCandidatures switch
        {
            0 => "aucune candidature liée",
            1 => "1 candidature liée",
            var nombre => $"{nombre} candidatures liées"
        };

        return new LigneContactEntreprise(
            contact.Identifiant,
            contact.Nom,
            contact.Prenom,
            contact.Role,
            contact.Mail,
            contact.Telephone,
            contact.Notes,
            contact.NombreCandidatures,
            contact.NomComplet,
            string.Join(" · ", new[] { contact.Role, contact.Mail, contact.Telephone, candidatures }
                .Where(v => !string.IsNullOrWhiteSpace(v))));
    }
}
