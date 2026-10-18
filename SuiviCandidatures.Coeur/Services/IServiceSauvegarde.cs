namespace SuiviCandidatures.Coeur.Services;

public sealed record ResumeSauvegarde(int NombreEntreprises, int NombreCandidatures, int NombreContacts);

public interface IServiceSauvegarde
{
    /// <summary>Écrit toutes les données (entreprises, adresses, contacts, candidatures, offres, historique) en JSON.</summary>
    Task ExporterAsync(Stream destination, CancellationToken jeton = default);

    /// <summary>
    /// Vérifie qu'un fichier de sauvegarde est valide et retourne son contenu résumé, sans toucher à la base de données.
    /// Lève ExceptionValidationMetier si le fichier est invalide.
    /// </summary>
    Task<ResumeSauvegarde> AnalyserAsync(Stream source, CancellationToken jeton = default);

    /// <summary>
    /// Remplace toutes les données par celles du fichier, dans une transaction : en cas d'échec, rien n'est modifié.
    /// Les paramètres de l'application sont conservés. Lève ExceptionValidationMetier si le fichier est invalide.
    /// </summary>
    Task<ResumeSauvegarde> ImporterAsync(Stream source, CancellationToken jeton = default);
}
