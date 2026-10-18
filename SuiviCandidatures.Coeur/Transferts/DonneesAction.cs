using SuiviCandidatures.Coeur.Modeles.Enumerations;

namespace SuiviCandidatures.Coeur.Transferts;

/// <summary>Action saisie manuellement : relance, entretien ou autre. Identifiant = 0 pour une création.</summary>
public sealed class DonneesAction
{
    public int Identifiant { get; set; }

    public int IdentifiantCandidature { get; set; }

    public TypeAction Type { get; set; } = TypeAction.RelanceParMail;

    public DateTime Date { get; set; } = DateTime.Now;

    public string? Note { get; set; }
}
