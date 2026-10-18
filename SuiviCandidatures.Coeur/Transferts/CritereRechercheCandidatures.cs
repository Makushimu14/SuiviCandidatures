using SuiviCandidatures.Coeur.Modeles.Enumerations;

namespace SuiviCandidatures.Coeur.Transferts;

public sealed class CritereRechercheCandidatures
{
    public string? TexteRecherche { get; set; }

    public StatutCandidature? Statut { get; set; }

    public CritereTriCandidatures Tri { get; set; } = CritereTriCandidatures.Date;

    public bool Decroissant { get; set; } = true;

    /// <summary>Numéro de page, à partir de 1.</summary>
    public int NumeroPage { get; set; } = 1;

    public int TaillePage { get; set; } = 10;
}
