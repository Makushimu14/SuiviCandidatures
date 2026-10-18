using SuiviCandidatures.Coeur.Transferts;

namespace SuiviCandidatures.Coeur.Services;

public interface IServiceEntreprises
{
    /// <summary>Toutes les entreprises (identifiant et nom), triées par nom. Sert aux suggestions du formulaire de candidature.</summary>
    Task<IReadOnlyList<ResumeEntreprise>> ListerAsync(CancellationToken jeton = default);

    Task<IReadOnlyList<ResumeAdresse>> ObtenirAdressesAsync(int identifiantEntreprise, CancellationToken jeton = default);

    /// <summary>Recherche (nom, secteur, ville), tri et pagination côté base de données.</summary>
    Task<ResultatPagine<ElementListeEntreprise>> RechercherAsync(CritereRechercheEntreprises critere, CancellationToken jeton = default);

    Task<DetailEntreprise?> ObtenirDetailAsync(int identifiant, CancellationToken jeton = default);

    Task<DonneesEntreprise?> ObtenirPourModificationAsync(int identifiant, CancellationToken jeton = default);

    /// <summary>
    /// Crée ou modifie une entreprise et retourne son identifiant. Refuse un nom déjà utilisé
    /// (sans tenir compte de la casse ni des accents). Lève ExceptionValidationMetier si les données sont invalides.
    /// </summary>
    Task<int> EnregistrerAsync(DonneesEntreprise donnees, CancellationToken jeton = default);

    /// <summary>Supprime l'entreprise avec ses adresses, ses contacts et ses candidatures (et leur historique).</summary>
    Task SupprimerAsync(int identifiant, CancellationToken jeton = default);
}
