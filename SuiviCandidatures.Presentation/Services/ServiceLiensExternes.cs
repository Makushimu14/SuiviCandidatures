using Microsoft.Maui.ApplicationModel;
using SuiviCandidatures.Coeur.Services;

namespace SuiviCandidatures.Presentation.Services;

public sealed class ServiceLiensExternes : IServiceLiensExternes
{
    public async Task OuvrirAsync(string lien)
    {
        // Vérifié aussi à l'ouverture : la valeur vient de la base de données.
        if (!LiensWeb.EstValide(lien))
        {
            return;
        }

        await Launcher.Default.OpenAsync(new Uri(lien));
    }
}
