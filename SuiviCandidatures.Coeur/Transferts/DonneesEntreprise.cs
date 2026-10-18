namespace SuiviCandidatures.Coeur.Transferts;

/// <summary>Entreprise saisie dans le formulaire. Identifiant = 0 pour une création.</summary>
public sealed class DonneesEntreprise
{
    public int Identifiant { get; set; }

    public string Nom { get; set; } = string.Empty;

    public string? SiteWeb { get; set; }

    public string? Secteur { get; set; }

    public string? Notes { get; set; }
}
