namespace SuiviCandidatures.Coeur.Transferts;

/// <summary>Contact saisi depuis la fiche d'une entreprise, sans lien avec une candidature. Identifiant = 0 pour une création.</summary>
public sealed class DonneesContactEntreprise
{
    public int Identifiant { get; set; }

    public int IdentifiantEntreprise { get; set; }

    public string Nom { get; set; } = string.Empty;

    public string? Prenom { get; set; }

    public string? Role { get; set; }

    public string? Mail { get; set; }

    public string? Telephone { get; set; }

    public string? Notes { get; set; }
}
