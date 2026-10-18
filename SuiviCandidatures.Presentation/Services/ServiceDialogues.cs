namespace SuiviCandidatures.Presentation.Services;

public sealed class ServiceDialogues : IServiceDialogues
{
    public Task<bool> ConfirmerAsync(string titre, string message, string libelleAccepter, string libelleAnnuler)
    {
        Page? page = Shell.Current?.CurrentPage;
        return page is null
            ? Task.FromResult(false)
            : page.DisplayAlertAsync(titre, message, libelleAccepter, libelleAnnuler);
    }
}
