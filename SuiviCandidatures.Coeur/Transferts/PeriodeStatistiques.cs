using System.ComponentModel;

namespace SuiviCandidatures.Coeur.Transferts;

public enum PeriodeStatistiques
{
    [Description("Toutes les candidatures")]
    Toutes,

    [Description("30 derniers jours")]
    TrenteJours,

    [Description("90 derniers jours")]
    QuatreVingtDixJours,

    [Description("12 derniers mois")]
    DouzeMois
}
