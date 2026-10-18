using SuiviCandidatures.Coeur.Modeles.Enumerations;

namespace SuiviCandidatures.Presentation.Services;

public sealed class ServiceTheme
{
    public void Appliquer(ThemeApplication theme)
    {
        if (Application.Current is null)
        {
            return;
        }

        Application.Current.UserAppTheme = theme switch
        {
            ThemeApplication.Clair => AppTheme.Light,
            ThemeApplication.Sombre => AppTheme.Dark,
            _ => AppTheme.Unspecified
        };
    }
}
