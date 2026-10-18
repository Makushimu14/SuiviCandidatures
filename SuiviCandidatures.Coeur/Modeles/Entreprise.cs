namespace SuiviCandidatures.Coeur.Modeles;

public class Entreprise
{
    public int Identifiant { get; set; }

    public string Nom { get; set; } = string.Empty;

    public string? SiteWeb { get; set; }

    public string? Secteur { get; set; }

    public string? Notes { get; set; }

    public List<AdresseEntreprise> Adresses { get; set; } = [];

    public List<Contact> Contacts { get; set; } = [];

    public List<Candidature> Candidatures { get; set; } = [];
}
