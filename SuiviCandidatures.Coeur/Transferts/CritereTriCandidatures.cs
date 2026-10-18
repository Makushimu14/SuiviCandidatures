using System.ComponentModel;

namespace SuiviCandidatures.Coeur.Transferts;

public enum CritereTriCandidatures
{
    [Description("Date de candidature")]
    Date,

    [Description("Statut")]
    Statut,

    [Description("Type de contrat")]
    TypeContrat,

    [Description("Entreprise")]
    Entreprise
}
