namespace SuiviCandidatures.Coeur.Transferts;

/// <summary>
/// Contact saisi depuis une candidature. Identifiant = 0 : le contact est créé pour l'entreprise de la candidature
/// et lié à celle-ci. Sinon le contact existant est modifié.
/// </summary>
public sealed class DonneesContact
{
    public int Identifiant { get; set; }

    public int IdentifiantCandidature { get; set; }

    public string Nom { get; set; } = string.Empty;

    public string? Prenom { get; set; }

    public string? Role { get; set; }

    public string? Mail { get; set; }

    public string? Telephone { get; set; }

    public string? Notes { get; set; }
}
