namespace SuiviCandidatures.Coeur.Transferts;

public sealed record DonneesAdresse(string? Adresse, string? CodePostal, string? Ville, string? Mail, string? Telephone)
{
    public bool EstVide =>
        string.IsNullOrWhiteSpace(Adresse) &&
        string.IsNullOrWhiteSpace(CodePostal) &&
        string.IsNullOrWhiteSpace(Ville) &&
        string.IsNullOrWhiteSpace(Mail) &&
        string.IsNullOrWhiteSpace(Telephone);
}
