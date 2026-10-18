namespace SuiviCandidatures.Presentation.Services;

public interface IServiceDialogues
{
    /// <summary>Affiche une confirmation et retourne vrai si l'utilisateur accepte.</summary>
    Task<bool> ConfirmerAsync(string titre, string message, string libelleAccepter, string libelleAnnuler);
}
