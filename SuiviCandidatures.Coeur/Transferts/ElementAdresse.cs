using SuiviCandidatures.Coeur.Services;

namespace SuiviCandidatures.Coeur.Transferts;

public sealed record ElementAdresse(
    int Identifiant,
    string? Adresse,
    string? CodePostal,
    string? Ville,
    string? Mail,
    string? Telephone)
{
    public string Libelle => FormatageAdresse.ConstruireLibelle(Adresse, CodePostal, Ville);
}
