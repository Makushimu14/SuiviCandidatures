using SuiviCandidatures.Coeur.Modeles.Enumerations;
using SuiviCandidatures.Coeur.Transferts;

namespace SuiviCandidatures.Coeur.Services;

public interface IServiceCandidatures
{
    /// <summary>Recherche, filtre, trie et pagine côté base de données.</summary>
    Task<ResultatPagine<ElementListeCandidature>> RechercherAsync(CritereRechercheCandidatures critere, CancellationToken jeton = default);

    /// <summary>Nombre de candidatures par statut. Les statuts sans candidature sont absents.</summary>
    Task<IReadOnlyDictionary<StatutCandidature, int>> CompterParStatutAsync(CancellationToken jeton = default);

    Task<DonneesCandidature?> ObtenirPourModificationAsync(int identifiant, CancellationToken jeton = default);

    /// <summary>
    /// Crée ou modifie une candidature (Identifiant = 0 pour une création) et retourne son identifiant.
    /// Crée l'entreprise et l'adresse si nécessaire. Lève ExceptionValidationMetier si les données sont invalides.
    /// </summary>
    Task<int> EnregistrerAsync(DonneesCandidature donnees, CancellationToken jeton = default);
}
