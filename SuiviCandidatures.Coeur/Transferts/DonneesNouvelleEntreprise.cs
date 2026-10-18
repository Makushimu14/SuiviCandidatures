namespace SuiviCandidatures.Coeur.Transferts;

/// <summary>Informations d'une entreprise à créer. Ignorées si l'entreprise existe déjà.</summary>
public sealed record DonneesNouvelleEntreprise(string? SiteWeb, string? Secteur, string? Notes);
