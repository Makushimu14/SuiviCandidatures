using SuiviCandidatures.Coeur.Modeles.Enumerations;
using SuiviCandidatures.Coeur.Transferts;

namespace SuiviCandidatures.Coeur.Services;

/// <summary>Opérations de la page de détail d'une candidature : statut, historique, relance, suppression.</summary>
public interface IServiceSuiviCandidature
{
    Task<DetailCandidature?> ObtenirDetailAsync(int identifiant, CancellationToken jeton = default);

    /// <summary>Change le statut et ajoute l'action « Changement de statut » à l'historique. Sans effet si le statut est identique.</summary>
    Task ChangerStatutAsync(int identifiant, StatutCandidature nouveauStatut, CancellationToken jeton = default);

    /// <summary>Crée ou modifie une action manuelle. Lève ExceptionValidationMetier si les données sont invalides.</summary>
    Task EnregistrerActionAsync(DonneesAction donnees, CancellationToken jeton = default);

    /// <summary>Supprime une action manuelle. Les actions automatiques ne peuvent pas être supprimées.</summary>
    Task SupprimerActionAsync(int identifiantAction, CancellationToken jeton = default);

    /// <summary>Délai propre à la candidature (null = délai des paramètres) et interrupteur « ne plus relancer ».</summary>
    Task EnregistrerParametresRelanceAsync(int identifiant, int? delaiRelanceJours, bool relancesDesactivees, CancellationToken jeton = default);

    /// <summary>Supprime la candidature, ses actions et ses offres. Les contacts sont conservés, simplement déliés.</summary>
    Task SupprimerAsync(int identifiant, CancellationToken jeton = default);
}
