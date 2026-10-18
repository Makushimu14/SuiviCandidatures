namespace SuiviCandidatures.Presentation.Services;

public sealed class ServiceNavigation : IServiceNavigation
{
    public Task AllerAsync(string route, int? identifiant = null)
    {
        string destination = identifiant is null
            ? route
            : $"{route}?{NomsRoutes.ParametreIdentifiant}={identifiant}";
        return Shell.Current.GoToAsync(destination);
    }

    public Task AllerALaRacineAsync(string route) => Shell.Current.GoToAsync($"//{route}");

    public Task RetourAsync() => Shell.Current.GoToAsync("..");
}
