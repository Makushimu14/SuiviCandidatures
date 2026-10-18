namespace SuiviCandidatures.Coeur.Transferts;

public sealed class CritereRechercheEntreprises
{
    public string? TexteRecherche { get; set; }

    public CritereTriEntreprises Tri { get; set; } = CritereTriEntreprises.Nom;

    public bool Decroissant { get; set; }

    /// <summary>Numéro de page, à partir de 1.</summary>
    public int NumeroPage { get; set; } = 1;

    public int TaillePage { get; set; } = 10;
}
