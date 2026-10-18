using System.Globalization;
using SuiviCandidatures.Coeur.Transferts;
using SuiviCandidatures.Presentation.Services;

namespace SuiviCandidatures.Presentation.ViewModels;

public sealed record LigneRelance(
    int Identifiant,
    string TitreOffre,
    string NomEntreprise,
    string DateAffichee,
    string Echeance,
    bool EnRetard)
{
    private static readonly CultureInfo Francais = CultureInfo.GetCultureInfo("fr-FR");

    public static LigneRelance Depuis(ElementRelance relance)
    {
        string echeance = FormatageRelance.DecrireEcheance(relance.JoursRestants);

        return new LigneRelance(
            relance.IdentifiantCandidature,
            relance.TitreOffre,
            relance.NomEntreprise,
            relance.DateRelance.ToString("d", Francais),
            char.ToUpper(echeance[0], Francais) + echeance[1..],
            relance.JoursRestants < 0);
    }
}
