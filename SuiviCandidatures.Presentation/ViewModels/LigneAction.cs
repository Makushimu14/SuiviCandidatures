using System.Globalization;
using SuiviCandidatures.Coeur.Extensions;
using SuiviCandidatures.Coeur.Modeles.Enumerations;
using SuiviCandidatures.Coeur.Transferts;

namespace SuiviCandidatures.Presentation.ViewModels;

public sealed record LigneAction(
    int Identifiant,
    DateTime DateHeure,
    TypeAction Type,
    string? Note,
    string DateAffichee,
    string TypeLibelle,
    string Detail,
    bool EstModifiable)
{
    private static readonly CultureInfo Francais = CultureInfo.GetCultureInfo("fr-FR");

    public bool DetailVisible => !string.IsNullOrWhiteSpace(Detail);

    public static LigneAction Depuis(ElementAction action)
    {
        // L'heure n'est affichée que si elle a un sens (les actions automatiques initiales n'en ont pas).
        string date = action.Date.TimeOfDay == TimeSpan.Zero
            ? action.Date.ToString("d", Francais)
            : action.Date.ToString("g", Francais);

        string detail = action.Type == TypeAction.ChangementStatut
            && action.AncienStatut is { } ancien
            && action.NouveauStatut is { } nouveau
                ? $"{ancien.ObtenirLibelle()} → {nouveau.ObtenirLibelle()}"
                : action.Note ?? string.Empty;

        return new LigneAction(
            action.Identifiant,
            action.Date,
            action.Type,
            action.Note,
            date,
            action.Type.ObtenirLibelle(),
            detail,
            action.EstModifiable);
    }
}
