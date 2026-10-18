namespace SuiviCandidatures.Coeur.Transferts;

/// <summary>Une valeur d'un graphique. Cle identifie la catégorie (nom de la valeur d'énumération, par exemple).</summary>
public sealed record PointStatistique(string Cle, string Libelle, int Valeur);
