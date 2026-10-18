namespace SuiviCandidatures.Coeur.Services;

public static class FormatageAdresse
{
    /// <summary>Construit un libellé lisible « rue, code postal ville », ou « Adresse sans détail » si tout est vide.</summary>
    public static string ConstruireLibelle(string? adresse, string? codePostal, string? ville)
    {
        string codePostalEtVille = $"{codePostal} {ville}".Trim();
        string libelle = string.Join(", ", new[] { adresse?.Trim(), codePostalEtVille }.Where(p => !string.IsNullOrWhiteSpace(p)));
        return libelle.Length == 0 ? "Adresse sans détail" : libelle;
    }
}
