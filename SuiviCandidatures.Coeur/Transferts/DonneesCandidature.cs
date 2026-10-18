using SuiviCandidatures.Coeur.Modeles.Enumerations;

namespace SuiviCandidatures.Coeur.Transferts;

/// <summary>Contenu du formulaire de candidature, pour la création (Identifiant = 0) comme pour la modification.</summary>
public sealed class DonneesCandidature
{
    public int Identifiant { get; set; }

    public string TitreOffre { get; set; } = string.Empty;

    public DateTime DateCandidature { get; set; } = DateTime.Today;

    public StatutCandidature Statut { get; set; } = StatutCandidature.EnAttenteReponse;

    public TypeCandidature TypeCandidature { get; set; } = TypeCandidature.ARepondreAUneOffre;

    public TypeContrat TypeContrat { get; set; } = TypeContrat.ContratDureeIndeterminee;

    public string? Commentaires { get; set; }

    /// <summary>Nom saisi. Si une entreprise de ce nom existe déjà, elle est utilisée ; sinon elle est créée.</summary>
    public string NomEntreprise { get; set; } = string.Empty;

    /// <summary>Utilisé uniquement si l'entreprise doit être créée.</summary>
    public DonneesNouvelleEntreprise? NouvelleEntreprise { get; set; }

    /// <summary>Adresse existante de l'entreprise choisie.</summary>
    public int? IdentifiantAdresse { get; set; }

    /// <summary>Adresse à créer pour l'entreprise. Prioritaire sur IdentifiantAdresse si elle n'est pas vide.</summary>
    public DonneesAdresse? NouvelleAdresse { get; set; }
}
