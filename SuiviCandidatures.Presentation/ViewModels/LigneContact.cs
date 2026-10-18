using SuiviCandidatures.Coeur.Transferts;

namespace SuiviCandidatures.Presentation.ViewModels;

public sealed record LigneContact(
    int Identifiant,
    string Nom,
    string? Prenom,
    string? Role,
    string? Mail,
    string? Telephone,
    string? Notes,
    string NomComplet,
    string Details)
{
    public bool DetailsVisibles => !string.IsNullOrWhiteSpace(Details);

    public static LigneContact Depuis(ElementContact contact) => new(
        contact.Identifiant,
        contact.Nom,
        contact.Prenom,
        contact.Role,
        contact.Mail,
        contact.Telephone,
        contact.Notes,
        contact.NomComplet,
        string.Join(" · ", new[] { contact.Role, contact.Mail, contact.Telephone }.Where(v => !string.IsNullOrWhiteSpace(v))));
}
