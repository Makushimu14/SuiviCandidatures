namespace SuiviCandidatures.Presentation.Services;

public interface IServiceLiensExternes
{
    /// <summary>Ouvre une adresse web dans le navigateur. Les adresses qui ne sont pas en http ou https sont ignorées.</summary>
    Task OuvrirAsync(string lien);
}
