using SuiviCandidatures.Coeur.Services;

namespace SuiviCandidatures.Presentation.Services;

/// <summary>Implémentation utilisée sur les plateformes sans notifications système (pour l'instant tout sauf Windows).</summary>
public sealed class ServiceNotificationsNonPrisesEnCharge : IServiceNotificationsSysteme
{
    public bool Disponible => false;

    public string? MessageIndisponibilite => "Les notifications système ne sont pas prises en charge sur cette plateforme.";

    public void Initialiser()
    {
    }

    public void Terminer()
    {
    }

    public void Afficher(string titre, string message) =>
        throw new InvalidOperationException(MessageIndisponibilite);
}
