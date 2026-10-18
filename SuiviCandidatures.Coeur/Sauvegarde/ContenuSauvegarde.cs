using SuiviCandidatures.Coeur.Modeles.Enumerations;

namespace SuiviCandidatures.Coeur.Sauvegarde;

/// <summary>
/// Contenu d'un fichier de sauvegarde. Les identifiants ne servent qu'à relier les éléments entre eux dans le fichier :
/// à l'import, de nouveaux identifiants sont attribués par la base de données.
/// Les paramètres de l'application ne sont pas sauvegardés.
/// </summary>
public sealed class ContenuSauvegarde
{
    public const int VersionActuelle = 1;

    public int Version { get; set; } = VersionActuelle;

    public DateTime DateExport { get; set; } = DateTime.Now;

    public List<EntrepriseSauvegarde> Entreprises { get; set; } = [];

    public List<CandidatureSauvegarde> Candidatures { get; set; } = [];
}

public sealed class EntrepriseSauvegarde
{
    public int Identifiant { get; set; }

    public string Nom { get; set; } = string.Empty;

    public string? SiteWeb { get; set; }

    public string? Secteur { get; set; }

    public string? Notes { get; set; }

    public List<AdresseSauvegarde> Adresses { get; set; } = [];

    public List<ContactSauvegarde> Contacts { get; set; } = [];
}

public sealed class AdresseSauvegarde
{
    public int Identifiant { get; set; }

    public string? Adresse { get; set; }

    public string? CodePostal { get; set; }

    public string? Ville { get; set; }

    public string? Mail { get; set; }

    public string? Telephone { get; set; }
}

public sealed class ContactSauvegarde
{
    public int Identifiant { get; set; }

    public string Nom { get; set; } = string.Empty;

    public string? Prenom { get; set; }

    public string? Role { get; set; }

    public string? Mail { get; set; }

    public string? Telephone { get; set; }

    public string? Notes { get; set; }
}

public sealed class CandidatureSauvegarde
{
    public int Identifiant { get; set; }

    public int IdentifiantEntreprise { get; set; }

    public int? IdentifiantAdresse { get; set; }

    public string TitreOffre { get; set; } = string.Empty;

    public DateTime DateCandidature { get; set; }

    public StatutCandidature Statut { get; set; }

    public TypeCandidature TypeCandidature { get; set; }

    public TypeContrat TypeContrat { get; set; }

    public string? Commentaires { get; set; }

    public int? DelaiRelanceJours { get; set; }

    public bool RelancesDesactivees { get; set; }

    public List<OffreSauvegarde> Offres { get; set; } = [];

    public List<ActionSauvegarde> Actions { get; set; } = [];

    /// <summary>Identifiants (dans le fichier) des contacts liés, tous issus de la même entreprise.</summary>
    public List<int> IdentifiantsContacts { get; set; } = [];
}

public sealed class OffreSauvegarde
{
    public string? NomSite { get; set; }

    public string? Reference { get; set; }

    public string? Lien { get; set; }
}

public sealed class ActionSauvegarde
{
    public DateTime Date { get; set; }

    public TypeAction Type { get; set; }

    public string? Note { get; set; }

    public StatutCandidature? AncienStatut { get; set; }

    public StatutCandidature? NouveauStatut { get; set; }
}
