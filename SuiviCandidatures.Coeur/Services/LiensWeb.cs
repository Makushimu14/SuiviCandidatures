namespace SuiviCandidatures.Coeur.Services;

public static class LiensWeb
{
    private const int LongueurMaximale = 2000;

    /// <summary>Nettoie la saisie et ajoute « https:// » si aucun schéma n'est indiqué. Retourne null si la saisie est vide.</summary>
    public static string? Normaliser(string? saisie)
    {
        if (string.IsNullOrWhiteSpace(saisie))
        {
            return null;
        }

        string lien = saisie.Trim();
        return lien.Contains("://", StringComparison.Ordinal) ? lien : $"https://{lien}";
    }

    /// <summary>Vrai pour une adresse absolue en http ou https, avec un nom d'hôte. Tout autre schéma est refusé.</summary>
    public static bool EstValide(string? lien) =>
        lien is { Length: > 0 and <= LongueurMaximale }
        && Uri.TryCreate(lien, UriKind.Absolute, out Uri? adresse)
        && (adresse.Scheme == Uri.UriSchemeHttp || adresse.Scheme == Uri.UriSchemeHttps)
        && !string.IsNullOrEmpty(adresse.Host);
}
