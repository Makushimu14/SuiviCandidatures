namespace SuiviCandidatures.Coeur.Transferts;

public sealed record ElementContact(
    int Identifiant,
    string Nom,
    string? Prenom,
    string? Role,
    string? Mail,
    string? Telephone,
    string? Notes)
{
    public string NomComplet => $"{Prenom} {Nom}".Trim();
}
