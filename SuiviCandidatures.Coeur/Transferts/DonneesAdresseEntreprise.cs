namespace SuiviCandidatures.Coeur.Transferts;

/// <summary>Adresse d'une entreprise. Identifiant = 0 pour une création.</summary>
public sealed class DonneesAdresseEntreprise
{
    public int Identifiant { get; set; }

    public int IdentifiantEntreprise { get; set; }

    public string? Adresse { get; set; }

    public string? CodePostal { get; set; }

    public string? Ville { get; set; }

    public string? Mail { get; set; }

    public string? Telephone { get; set; }
}
