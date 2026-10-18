using SuiviCandidatures.Coeur.Extensions;

namespace SuiviCandidatures.Presentation.ViewModels;

/// <summary>Entrée d'une liste déroulante basée sur une énumération. Valeur nulle = « aucun filtre ».</summary>
public sealed record ChoixEnumeration(Enum? Valeur, string Libelle)
{
    public static IReadOnlyList<ChoixEnumeration> Pour<TEnumeration>() where TEnumeration : struct, Enum =>
        Enum.GetValues<TEnumeration>()
            .Select(valeur => new ChoixEnumeration(valeur, valeur.ObtenirLibelle()))
            .ToList();
}
