namespace SuiviCandidatures.Coeur.Transferts;

/// <summary>Villes : les villes distinctes des adresses de l'entreprise, séparées par des virgules.</summary>
public sealed record ElementListeEntreprise(int Identifiant, string Nom, string? Secteur, string Villes, int NombreCandidatures);
