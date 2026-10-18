using SuiviCandidatures.Coeur.Services;

namespace SuiviCandidatures.Presentation.Services;

/// <summary>Écriture des sauvegardes dans le dossier Documents de l'utilisateur, et choix d'un fichier à importer.</summary>
public sealed class ServiceFichiersSauvegarde(IServiceSauvegarde serviceSauvegarde)
{
    private const string NomDossierApplication = "SuiviCandidatures";
    private const string NomDossierSauvegardes = "Sauvegardes";

    public static string DossierSauvegardes => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        NomDossierApplication,
        NomDossierSauvegardes);

    /// <summary>Exporte toutes les données dans un nouveau fichier horodaté et retourne son chemin complet.</summary>
    public async Task<string> ExporterAsync(string prefixeNomFichier)
    {
        Directory.CreateDirectory(DossierSauvegardes);
        string chemin = Path.Combine(DossierSauvegardes, $"{prefixeNomFichier}-{DateTime.Now:yyyy-MM-dd-HHmmss}.json");

        try
        {
            await using FileStream flux = new(chemin, FileMode.CreateNew, FileAccess.Write);
            await serviceSauvegarde.ExporterAsync(flux);
        }
        catch
        {
            // Pas de fichier incomplet laissé sur le disque.
            if (File.Exists(chemin))
            {
                File.Delete(chemin);
            }

            throw;
        }

        return chemin;
    }

    /// <summary>Ouvre le sélecteur de fichier. Retourne le contenu en mémoire, ou null si l'utilisateur annule.</summary>
    public async Task<MemoryStream?> ChoisirFichierAsync()
    {
        FilePickerFileType typeJson = new(new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.WinUI, [".json"] },
            { DevicePlatform.MacCatalyst, ["public.json"] }
        });

        FileResult? fichier = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Choisir une sauvegarde à importer",
            FileTypes = typeJson
        });
        if (fichier is null)
        {
            return null;
        }

        await using Stream flux = await fichier.OpenReadAsync();
        MemoryStream contenu = new();
        await flux.CopyToAsync(contenu);
        contenu.Position = 0;
        return contenu;
    }
}
