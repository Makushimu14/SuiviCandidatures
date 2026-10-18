namespace SuiviCandidatures.Coeur.Services;

/// <summary>Affichage de notifications du système d'exploitation, avec une implémentation par plateforme.</summary>
public interface IServiceNotificationsSysteme
{
    /// <summary>Vrai si les notifications système sont utilisables sur cette plateforme et dans ce contexte d'exécution.</summary>
    bool Disponible { get; }

    /// <summary>Raison de l'indisponibilité, lorsque Disponible est faux.</summary>
    string? MessageIndisponibilite { get; }

    /// <summary>Enregistre l'application auprès du système. Ne lève jamais d'exception : un échec rend simplement le service indisponible.</summary>
    void Initialiser();

    /// <summary>Libère l'enregistrement auprès du système. À appeler à la fermeture de l'application.</summary>
    void Terminer();

    /// <summary>Affiche une notification. Lève une exception si le service est indisponible ou si l'affichage échoue.</summary>
    void Afficher(string titre, string message);
}
