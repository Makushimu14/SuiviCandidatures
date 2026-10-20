using SuiviCandidatures.Coeur.Services;
using SuiviCandidatures.Coeur.Transferts;

namespace SuiviCandidatures.Tests.Outils;

/// <summary>Notifications système simulées : enregistre les messages, ou échoue sur demande.</summary>
public sealed class NotificationsFactices : IServiceNotificationsSysteme
{
    public List<(string Titre, string Message)> Notifications { get; } = [];

    public bool Echouer { get; set; }

    public bool Disponible => true;

    public string? MessageIndisponibilite => null;

    public void Initialiser()
    {
    }

    public void Terminer()
    {
    }

    public void Afficher(string titre, string message)
    {
        if (Echouer)
        {
            throw new InvalidOperationException("Notifications indisponibles.");
        }

        Notifications.Add((titre, message));
    }
}

/// <summary>Relances fournies directement par le test, sans base de données.</summary>
public sealed class RelancesFactices : IServiceRelances
{
    public List<ElementRelance> Relances { get; } = [];

    public Task<IReadOnlyList<ElementRelance>> ListerAsync(DateTime aujourdhui, CancellationToken jeton = default) =>
        Task.FromResult<IReadOnlyList<ElementRelance>>(Relances.ToList());
}
