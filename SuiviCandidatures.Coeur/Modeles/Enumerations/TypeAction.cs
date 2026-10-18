using System.ComponentModel;

namespace SuiviCandidatures.Coeur.Modeles.Enumerations;

public enum TypeAction
{
    [Description("Candidature envoyée")]
    CandidatureEnvoyee,

    [Description("Relance par mail")]
    RelanceParMail,

    [Description("Relance par téléphone")]
    RelanceParTelephone,

    [Description("Entretien")]
    Entretien,

    [Description("Changement de statut")]
    ChangementStatut,

    [Description("Autre")]
    Autre
}
