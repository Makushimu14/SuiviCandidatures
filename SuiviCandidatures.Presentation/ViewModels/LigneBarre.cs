namespace SuiviCandidatures.Presentation.ViewModels;

/// <summary>Barre d'un graphique : Largeur sert aux barres horizontales, Hauteur aux barres verticales.</summary>
public sealed record LigneBarre(string Libelle, int Valeur, double Largeur, double Hauteur, Color Couleur);
