using SuiviCandidatures.Coeur.Modeles.Enumerations;

namespace SuiviCandidatures.Presentation.ViewModels;

public static class PaletteStatuts
{
    public static Color ObtenirCouleur(StatutCandidature statut) => Color.FromArgb(statut switch
    {
        StatutCandidature.CurriculumVitaeVu => "#64748B",
        StatutCandidature.EnAttenteReponse => "#B45309",
        StatutCandidature.EnCoursEtude => "#2563EB",
        StatutCandidature.EntretienPlanifie => "#7C3AED",
        StatutCandidature.EntretienEffectue => "#4F46E5",
        StatutCandidature.OffreRecue => "#15803D",
        StatutCandidature.OffreTermineeSansReponse => "#6B7280",
        StatutCandidature.Refusee => "#DC2626",
        StatutCandidature.Retiree => "#78716C",
        _ => "#6B7280"
    });
}
