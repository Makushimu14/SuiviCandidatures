namespace SuiviCandidatures.Coeur.Transferts;

/// <summary>Offre liée à une candidature. Identifiant = 0 pour une création.</summary>
public sealed class DonneesOffre
{
    public int Identifiant { get; set; }

    public int IdentifiantCandidature { get; set; }

    public string? NomSite { get; set; }

    public string? Reference { get; set; }

    public string? Lien { get; set; }
}
