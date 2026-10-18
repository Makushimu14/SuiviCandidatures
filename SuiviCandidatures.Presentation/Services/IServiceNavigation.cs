namespace SuiviCandidatures.Presentation.Services;

public interface IServiceNavigation
{
    /// <summary>Navigue vers une route enregistrée, en passant éventuellement un identifiant.</summary>
    Task AllerAsync(string route, int? identifiant = null);

    /// <summary>Navigue vers une page principale du menu latéral (Accueil, Candidatures...), en quittant la pile courante.</summary>
    Task AllerALaRacineAsync(string route);

    Task RetourAsync();
}
