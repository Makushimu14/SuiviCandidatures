using SuiviCandidatures.Coeur.Modeles.Enumerations;

namespace SuiviCandidatures.Coeur.Modeles;

/// <summary>Paramètres de l'application. Une seule ligne en base, d'identifiant 1.</summary>
public class Parametres
{
    public const int IdentifiantUnique = 1;
    public const int DelaiRelanceMinimumJours = 1;
    public const int DelaiRelanceMaximumJours = 90;

    public static readonly IReadOnlyList<int> TaillesPageAutorisees = [10, 25, 50, 100];

    public int Identifiant { get; set; } = IdentifiantUnique;

    public int DelaiRelanceParDefautJours { get; set; } = 7;

    public int TaillePage { get; set; } = 10;

    public ThemeApplication Theme { get; set; } = ThemeApplication.Systeme;

    /// <summary>Notifications système des relances dues. Sans effet sur la page d'accueil, qui affiche toujours les relances.</summary>
    public bool NotificationsActivees { get; set; } = true;
}
