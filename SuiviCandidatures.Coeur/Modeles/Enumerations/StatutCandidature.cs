using System.ComponentModel;

namespace SuiviCandidatures.Coeur.Modeles.Enumerations;

public enum StatutCandidature
{
    [Description("CV vu")]
    CurriculumVitaeVu,

    [Description("En attente de réponse")]
    EnAttenteReponse,

    [Description("En cours d'étude")]
    EnCoursEtude,

    [Description("Entretien planifié")]
    EntretienPlanifie,

    [Description("Entretien effectué")]
    EntretienEffectue,

    [Description("Offre reçue")]
    OffreRecue,

    [Description("Offre terminée sans réponse reçue")]
    OffreTermineeSansReponse,

    [Description("Refusée")]
    Refusee,

    [Description("Candidature retirée")]
    Retiree
}
