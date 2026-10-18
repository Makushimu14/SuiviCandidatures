using SuiviCandidatures.Coeur.Transferts;

namespace SuiviCandidatures.Coeur.Services;

public interface IServiceOffres
{
    /// <summary>Crée ou modifie une offre liée à une candidature. Lève ExceptionValidationMetier si les données sont invalides.</summary>
    Task EnregistrerAsync(DonneesOffre donnees, CancellationToken jeton = default);

    Task SupprimerAsync(int identifiantOffre, CancellationToken jeton = default);
}
