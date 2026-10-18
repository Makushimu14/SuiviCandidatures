namespace SuiviCandidatures.Coeur.Services;

/// <summary>Construction de motifs pour l'opérateur LIKE de SQLite, avec échappement des caractères spéciaux.</summary>
internal static class MotifsRecherche
{
    public const string CaractereEchappement = "\\";

    public static string Contient(string texte)
    {
        string echappe = texte
            .Replace(CaractereEchappement, CaractereEchappement + CaractereEchappement)
            .Replace("%", CaractereEchappement + "%")
            .Replace("_", CaractereEchappement + "_");
        return $"%{echappe}%";
    }
}
