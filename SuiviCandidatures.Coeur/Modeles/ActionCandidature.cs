using SuiviCandidatures.Coeur.Modeles.Enumerations;

namespace SuiviCandidatures.Coeur.Modeles;

public class ActionCandidature
{
    public int Identifiant { get; set; }

    public DateTime Date { get; set; } = DateTime.Now;

    public TypeAction Type { get; set; }

    public string? Note { get; set; }

    /// <summary>Renseigné uniquement pour le type ChangementStatut.</summary>
    public StatutCandidature? AncienStatut { get; set; }

    /// <summary>Renseigné uniquement pour le type ChangementStatut.</summary>
    public StatutCandidature? NouveauStatut { get; set; }

    public int IdentifiantCandidature { get; set; }

    public Candidature Candidature { get; set; } = null!;
}
