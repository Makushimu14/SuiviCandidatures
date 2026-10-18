namespace SuiviCandidatures.Coeur.Modeles;

public class OffreCandidature
{
    public int Identifiant { get; set; }

    public string? Lien { get; set; }

    public string? NomSite { get; set; }

    public string? Reference { get; set; }

    public int IdentifiantCandidature { get; set; }

    public Candidature Candidature { get; set; } = null!;
}
