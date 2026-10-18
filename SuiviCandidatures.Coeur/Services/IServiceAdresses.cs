using SuiviCandidatures.Coeur.Transferts;

namespace SuiviCandidatures.Coeur.Services;

public interface IServiceAdresses
{
    /// <summary>Crée ou modifie une adresse d'entreprise. Lève ExceptionValidationMetier si les données sont invalides.</summary>
    Task EnregistrerAsync(DonneesAdresseEntreprise donnees, CancellationToken jeton = default);

    /// <summary>Supprime l'adresse. Les candidatures qui y étaient rattachées sont conservées, sans adresse.</summary>
    Task SupprimerAsync(int identifiantAdresse, CancellationToken jeton = default);
}
