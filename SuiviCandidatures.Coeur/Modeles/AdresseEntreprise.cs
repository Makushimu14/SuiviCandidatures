namespace SuiviCandidatures.Coeur.Modeles;

public class AdresseEntreprise
{
    public int Identifiant { get; set; }

    public string? Adresse { get; set; }

    public string? CodePostal { get; set; }

    public string? Ville { get; set; }

    public string? Mail { get; set; }

    public string? Telephone { get; set; }

    public int IdentifiantEntreprise { get; set; }

    public Entreprise Entreprise { get; set; } = null!;

    public List<Candidature> Candidatures { get; set; } = [];
}
