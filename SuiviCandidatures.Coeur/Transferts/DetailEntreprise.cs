namespace SuiviCandidatures.Coeur.Transferts;

public sealed record DetailEntreprise(
    int Identifiant,
    string Nom,
    string? SiteWeb,
    string? Secteur,
    string? Notes,
    IReadOnlyList<ElementAdresse> Adresses,
    IReadOnlyList<ElementContactEntreprise> Contacts,
    IReadOnlyList<ElementListeCandidature> Candidatures);
