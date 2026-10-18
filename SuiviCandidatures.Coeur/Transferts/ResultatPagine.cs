namespace SuiviCandidatures.Coeur.Transferts;

public sealed record ResultatPagine<TElement>(
    IReadOnlyList<TElement> Elements,
    int NombreTotal,
    int NumeroPage,
    int TaillePage)
{
    public int NombrePages => Math.Max(1, (int)Math.Ceiling(NombreTotal / (double)TaillePage));
}
