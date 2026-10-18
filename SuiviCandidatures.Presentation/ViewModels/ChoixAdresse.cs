namespace SuiviCandidatures.Presentation.ViewModels;

/// <summary>Entrée du sélecteur d'adresse : une adresse existante, « aucune adresse » ou « nouvelle adresse ».</summary>
public sealed record ChoixAdresse(int? Identifiant, string Libelle, bool EstNouvelle = false);
