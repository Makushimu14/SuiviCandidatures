namespace SuiviCandidatures.Coeur.Transferts;

public sealed record ElementContactEntreprise(
    int Identifiant,
    string Nom,
    string? Prenom,
    string? Role,
    string? Mail,
    string? Telephone,
    string? Notes,
    int NombreCandidatures)
{
    public string NomComplet => $"{Prenom} {Nom}".Trim();
}
