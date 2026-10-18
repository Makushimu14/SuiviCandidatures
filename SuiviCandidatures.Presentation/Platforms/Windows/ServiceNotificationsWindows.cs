using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using SuiviCandidatures.Coeur.Services;

namespace SuiviCandidatures.Presentation.Services;

/// <summary>
/// Notifications Windows via le Windows App SDK, pour une application non packagée.
/// Pour une application non packagée, Register() configure lui-même l'enregistrement nécessaire.
/// Non prises en charge si l'application est exécutée en administrateur.
/// </summary>
public sealed class ServiceNotificationsWindows : IServiceNotificationsSysteme
{
    private bool enregistre;

    public bool Disponible { get; private set; }

    public string? MessageIndisponibilite { get; private set; } = "Le service de notifications n'est pas encore initialisé.";

    public void Initialiser()
    {
        if (enregistre)
        {
            return;
        }

        try
        {
            // Le gestionnaire doit être abonné avant l'appel à Register().
            AppNotificationManager.Default.NotificationInvoked += QuandNotificationActivee;
            AppNotificationManager.Default.Register();

            enregistre = true;
            Disponible = true;
            MessageIndisponibilite = null;
        }
        catch (Exception exception)
        {
            Disponible = false;
            MessageIndisponibilite = $"Enregistrement des notifications impossible : {exception.Message}";
        }
    }

    public void Terminer()
    {
        if (!enregistre)
        {
            return;
        }

        try
        {
            AppNotificationManager.Default.Unregister();
        }
        catch
        {
            // Fermeture de l'application : rien d'utile à faire en cas d'échec.
        }

        enregistre = false;
        Disponible = false;
    }

    public void Afficher(string titre, string message)
    {
        // Enregistrement différé : fait au premier affichage, et non au démarrage de l'application.
        Initialiser();

        if (!Disponible)
        {
            throw new InvalidOperationException(MessageIndisponibilite);
        }

        AppNotification notification = new AppNotificationBuilder()
            .AddText(titre)
            .AddText(message)
            .BuildNotification();

        AppNotificationManager.Default.Show(notification);
    }

    // Prototype : le clic sur la notification n'a pas encore d'action (ramener la fenêtre au premier plan viendra ensuite).
    private void QuandNotificationActivee(AppNotificationManager expediteur, AppNotificationActivatedEventArgs arguments)
    {
    }
}
