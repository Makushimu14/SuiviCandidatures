using System.ComponentModel;
using System.Reflection;
using SuiviCandidatures.Coeur.Modeles.Enumerations;

namespace SuiviCandidatures.Coeur.Extensions;

public static class ExtensionsEnumerations
{
    /// <summary>Retourne le libellé français porté par l'attribut Description, sinon le nom de la valeur.</summary>
    public static string ObtenirLibelle(this Enum valeur)
    {
        FieldInfo? champ = valeur.GetType().GetField(valeur.ToString());
        DescriptionAttribute? attribut = champ?.GetCustomAttribute<DescriptionAttribute>();
        return attribut?.Description ?? valeur.ToString();
    }

    /// <summary>Actions saisies par l'utilisateur. Les autres sont créées automatiquement et ne sont ni modifiables ni supprimables.</summary>
    public static bool EstManuelle(this TypeAction type) => type is
        TypeAction.RelanceParMail or
        TypeAction.RelanceParTelephone or
        TypeAction.Entretien or
        TypeAction.Autre;

    /// <summary>Un statut terminal désactive les relances.</summary>
    public static bool EstTerminal(this StatutCandidature statut) => statut is
        StatutCandidature.OffreRecue or
        StatutCandidature.OffreTermineeSansReponse or
        StatutCandidature.Refusee or
        StatutCandidature.Retiree;
}
