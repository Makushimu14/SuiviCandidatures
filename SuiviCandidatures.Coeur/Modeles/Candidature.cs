using SuiviCandidatures.Coeur.Modeles.Enumerations;

namespace SuiviCandidatures.Coeur.Modeles;

public class Candidature
{
    public int Identifiant { get; set; }

    public string TitreOffre { get; set; } = string.Empty;

    public DateTime DateCandidature { get; set; } = DateTime.Today;

    public StatutCandidature Statut { get; set; } = StatutCandidature.EnAttenteReponse;

    public TypeCandidature TypeCandidature { get; set; } = TypeCandidature.ARepondreAUneOffre;

    public TypeContrat TypeContrat { get; set; } = TypeContrat.ContratDureeIndeterminee;

    public string? Commentaires { get; set; }

    /// <summary>Délai de relance propre à cette candidature. Si null, le délai des paramètres s'applique.</summary>
    public int? DelaiRelanceJours { get; set; }

    public bool RelancesDesactivees { get; set; }

    public int IdentifiantEntreprise { get; set; }

    public Entreprise Entreprise { get; set; } = null!;

    public int? IdentifiantAdresse { get; set; }

    public AdresseEntreprise? Adresse { get; set; }

    public List<OffreCandidature> Offres { get; set; } = [];

    public List<ActionCandidature> Actions { get; set; } = [];

    public List<Contact> Contacts { get; set; } = [];
}
