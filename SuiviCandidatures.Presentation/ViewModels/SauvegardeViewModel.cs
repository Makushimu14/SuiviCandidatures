using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using SuiviCandidatures.Coeur.Services;
using SuiviCandidatures.Presentation.Services;

namespace SuiviCandidatures.Presentation.ViewModels;

/// <summary>Section « Données » de la page Paramètres : export et import des données.</summary>
public partial class SauvegardeViewModel(
    IServiceSauvegarde serviceSauvegarde,
    ServiceFichiersSauvegarde fichiers,
    IServiceDialogues dialogues) : ObservableObject
{
    private const string MentionDonneesIntactes = " Vos données actuelles n'ont pas été modifiées.";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ExporterCommand))]
    [NotifyCanExecuteChangedFor(nameof(ImporterCommand))]
    private bool occupe;

    [ObservableProperty]
    private string message = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CheminDisponible))]
    private string cheminDernierFichier = string.Empty;

    public bool CheminDisponible => !string.IsNullOrEmpty(CheminDernierFichier);

    [RelayCommand(CanExecute = nameof(PeutAgir))]
    private async Task ExporterAsync()
    {
        Occupe = true;
        Message = string.Empty;

        try
        {
            string chemin = await fichiers.ExporterAsync("suivi-candidatures");
            CheminDernierFichier = chemin;
            Message = $"Données exportées dans : {chemin}";
        }
        catch (Exception exception)
        {
            Message = $"Export impossible : {exception.Message}";
        }
        finally
        {
            Occupe = false;
        }
    }

    [RelayCommand(CanExecute = nameof(PeutAgir))]
    private async Task ImporterAsync()
    {
        Occupe = true;
        Message = string.Empty;

        try
        {
            using MemoryStream? contenu = await fichiers.ChoisirFichierAsync();
            if (contenu is null)
            {
                return;
            }

            // Le fichier est entièrement vérifié avant de demander confirmation.
            ResumeSauvegarde resume = await serviceSauvegarde.AnalyserAsync(contenu);
            contenu.Position = 0;

            bool confirme = await dialogues.ConfirmerAsync(
                "Importer des données",
                $"Le fichier contient {resume.NombreCandidatures} candidature(s), {resume.NombreEntreprises} entreprise(s) " +
                $"et {resume.NombreContacts} contact(s). Toutes vos données actuelles seront remplacées. " +
                "Une sauvegarde automatique de vos données actuelles sera créée juste avant.",
                "Remplacer",
                "Annuler");
            if (!confirme)
            {
                return;
            }

            string sauvegardeAvantImport = await fichiers.ExporterAsync("avant-import");
            ResumeSauvegarde importe = await serviceSauvegarde.ImporterAsync(contenu);

            CheminDernierFichier = sauvegardeAvantImport;
            Message = $"Import terminé : {importe.NombreCandidatures} candidature(s), {importe.NombreEntreprises} entreprise(s). " +
                      $"Vos anciennes données sont sauvegardées dans : {sauvegardeAvantImport}";
        }
        catch (ExceptionValidationMetier exception)
        {
            Message = exception.Message + MentionDonneesIntactes;
        }
        catch (Exception exception)
        {
            Message = $"Import impossible : {exception.Message}.{MentionDonneesIntactes}";
        }
        finally
        {
            Occupe = false;
        }
    }

    [RelayCommand]
    private async Task CopierCheminAsync()
    {
        if (!CheminDisponible)
        {
            return;
        }

        await Clipboard.Default.SetTextAsync(CheminDernierFichier);
    }

    private bool PeutAgir() => !Occupe;
}
