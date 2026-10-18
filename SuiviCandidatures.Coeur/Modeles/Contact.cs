namespace SuiviCandidatures.Coeur.Modeles;

public class Contact
{
    public int Identifiant { get; set; }

    public string Nom { get; set; } = string.Empty;

    public string? Prenom { get; set; }

    public string? Role { get; set; }

    public string? Mail { get; set; }

    public string? Telephone { get; set; }

    public string? Notes { get; set; }

    public int IdentifiantEntreprise { get; set; }

    public Entreprise Entreprise { get; set; } = null!;

    /// <summary>Un contact peut être lié à plusieurs candidatures de son entreprise (relation plusieurs à plusieurs).</summary>
    public List<Candidature> Candidatures { get; set; } = [];
}
