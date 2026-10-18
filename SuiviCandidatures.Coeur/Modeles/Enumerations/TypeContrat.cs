using System.ComponentModel;

namespace SuiviCandidatures.Coeur.Modeles.Enumerations;

public enum TypeContrat
{
    [Description("Alternance")]
    Alternance,

    [Description("CDD")]
    ContratDureeDeterminee,

    [Description("CDI")]
    ContratDureeIndeterminee,

    [Description("Intérim")]
    Interim
}
