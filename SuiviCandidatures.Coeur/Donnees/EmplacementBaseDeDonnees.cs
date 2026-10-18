namespace SuiviCandidatures.Coeur.Donnees;

public static class EmplacementBaseDeDonnees
{
    public const string NomDossier = "SuiviCandidatures";
    public const string NomFichier = "suivi-candidatures.sqlite";

    /// <summary>Chemin du fichier de base de données dans le dossier de données locales de l'utilisateur. Le dossier est créé au besoin.</summary>
    public static string ObtenirChemin()
    {
        string dossierDonnees = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string dossier = Path.Combine(dossierDonnees, NomDossier);
        Directory.CreateDirectory(dossier);
        return Path.Combine(dossier, NomFichier);
    }
}
