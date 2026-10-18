using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SuiviCandidatures.Coeur.Services;
using SuiviCandidatures.Coeur.Transferts;
using SuiviCandidatures.Presentation.Services;

namespace SuiviCandidatures.Presentation.ViewModels;

/// <summary>Carte « Offres » de la page de détail d'une candidature.</summary>
public partial class OffresCandidatureViewModel(
    IServiceOffres serviceOffres,
    IServiceDialogues dialogues,
    IServiceLiensExternes liensExternes) : ObservableObject
{
    private int identifiantCandidature;
    private int identifiantOffreEnEdition;
    private Func<Task>? rechargerPageParente;

    public ObservableCollection<LigneOffre> Offres { get; } = [];

    public bool AucuneOffre => Offres.Count == 0;

    [ObservableProperty]
    private bool formulaireVisible;

    [ObservableProperty]
    private string titreFormulaire = string.Empty;

    [ObservableProperty]
    private string nomSite = string.Empty;

    [ObservableProperty]
    private string reference = string.Empty;

    [ObservableProperty]
    private string lien = string.Empty;

    [ObservableProperty]
    private string messageErreur = string.Empty;

    /// <summary>Appelée par la page de détail à chaque chargement. N'affecte pas le formulaire s'il est ouvert.</summary>
    public void Charger(int identifiantCandidature, IReadOnlyList<ElementOffre> offres, Func<Task> rechargerPageParente)
    {
        this.identifiantCandidature = identifiantCandidature;
        this.rechargerPageParente = rechargerPageParente;

        Offres.Clear();
        foreach (ElementOffre offre in offres)
        {
            Offres.Add(LigneOffre.Depuis(offre));
        }

        OnPropertyChanged(nameof(AucuneOffre));
    }

    [RelayCommand]
    private void AjouterOffre()
    {
        identifiantOffreEnEdition = 0;
        TitreFormulaire = "Nouvelle offre";
        NomSite = string.Empty;
        Reference = string.Empty;
        Lien = string.Empty;
        MessageErreur = string.Empty;
        FormulaireVisible = true;
    }

    [RelayCommand]
    private void ModifierOffre(LigneOffre ligne)
    {
        identifiantOffreEnEdition = ligne.Identifiant;
        TitreFormulaire = "Modifier l'offre";
        NomSite = ligne.NomSite ?? string.Empty;
        Reference = ligne.Reference ?? string.Empty;
        Lien = ligne.Lien ?? string.Empty;
        MessageErreur = string.Empty;
        FormulaireVisible = true;
    }

    [RelayCommand]
    private void AnnulerOffre() => FormulaireVisible = false;

    [RelayCommand]
    private async Task EnregistrerOffreAsync()
    {
        MessageErreur = string.Empty;

        DonneesOffre donnees = new()
        {
            Identifiant = identifiantOffreEnEdition,
            IdentifiantCandidature = identifiantCandidature,
            NomSite = NomSite,
            Reference = Reference,
            Lien = Lien
        };

        try
        {
            await serviceOffres.EnregistrerAsync(donnees);
            FormulaireVisible = false;
            await RechargerAsync();
        }
        catch (ExceptionValidationMetier exception)
        {
            MessageErreur = exception.Message;
        }
        catch (Exception exception)
        {
            MessageErreur = $"Enregistrement impossible : {exception.Message}";
        }
    }

    [RelayCommand]
    private async Task SupprimerOffreAsync(LigneOffre ligne)
    {
        bool confirme = await dialogues.ConfirmerAsync(
            "Supprimer l'offre",
            $"Supprimer l'offre « {ligne.TitreAffiche} » ?",
            "Supprimer",
            "Annuler");
        if (!confirme)
        {
            return;
        }

        try
        {
            await serviceOffres.SupprimerAsync(ligne.Identifiant);
            await RechargerAsync();
        }
        catch (Exception exception)
        {
            MessageErreur = $"Suppression impossible : {exception.Message}";
        }
    }

    [RelayCommand]
    private async Task OuvrirLienAsync(LigneOffre ligne)
    {
        if (ligne.Lien is null)
        {
            return;
        }

        try
        {
            await liensExternes.OuvrirAsync(ligne.Lien);
        }
        catch (Exception exception)
        {
            MessageErreur = $"Ouverture du lien impossible : {exception.Message}";
        }
    }

    private Task RechargerAsync() => rechargerPageParente?.Invoke() ?? Task.CompletedTask;
}
