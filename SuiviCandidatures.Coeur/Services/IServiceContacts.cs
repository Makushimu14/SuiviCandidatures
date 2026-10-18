using SuiviCandidatures.Coeur.Transferts;

namespace SuiviCandidatures.Coeur.Services;

public interface IServiceContacts
{
    /// <summary>Contacts de l'entreprise de la candidature qui ne sont pas encore liés à cette candidature.</summary>
    Task<IReadOnlyList<ElementContact>> ListerDisponiblesAsync(int identifiantCandidature, CancellationToken jeton = default);

    /// <summary>
    /// Crée un contact pour l'entreprise de la candidature et le lie à celle-ci (Identifiant = 0),
    /// ou modifie un contact déjà lié à la candidature. Lève ExceptionValidationMetier si les données sont invalides.
    /// </summary>
    Task EnregistrerAsync(DonneesContact donnees, CancellationToken jeton = default);

    /// <summary>
    /// Crée un contact pour l'entreprise (Identifiant = 0) ou modifie un contact de cette entreprise,
    /// sans toucher à ses liaisons avec les candidatures.
    /// </summary>
    Task EnregistrerPourEntrepriseAsync(DonneesContactEntreprise donnees, CancellationToken jeton = default);

    /// <summary>Supprime le contact, qui est retiré de toutes ses candidatures.</summary>
    Task SupprimerAsync(int identifiantContact, CancellationToken jeton = default);

    /// <summary>Lie un contact existant de la même entreprise à la candidature.</summary>
    Task LierAsync(int identifiantCandidature, int identifiantContact, CancellationToken jeton = default);

    /// <summary>Retire la liaison. Le contact reste enregistré pour l'entreprise et ses autres candidatures.</summary>
    Task DelierAsync(int identifiantCandidature, int identifiantContact, CancellationToken jeton = default);
}
