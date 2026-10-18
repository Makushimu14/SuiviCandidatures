using Microsoft.EntityFrameworkCore;
using SuiviCandidatures.Coeur.Donnees;
using SuiviCandidatures.Coeur.Modeles;

namespace SuiviCandidatures.Coeur.Services;

public sealed class ServiceParametres(IDbContextFactory<ContexteBaseDeDonnees> fabrique) : IServiceParametres
{
    public async Task<Parametres> ObtenirAsync(CancellationToken jeton = default)
    {
        await using ContexteBaseDeDonnees contexte = await fabrique.CreateDbContextAsync(jeton);

        Parametres? existants = await contexte.Parametres
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Identifiant == Parametres.IdentifiantUnique, jeton);
        if (existants is not null)
        {
            return existants;
        }

        Parametres parDefaut = new();
        contexte.Parametres.Add(parDefaut);
        await contexte.SaveChangesAsync(jeton);
        return parDefaut;
    }

    public async Task EnregistrerAsync(Parametres parametres, CancellationToken jeton = default)
    {
        parametres.Identifiant = Parametres.IdentifiantUnique;
        parametres.DelaiRelanceParDefautJours = Math.Clamp(
            parametres.DelaiRelanceParDefautJours,
            Parametres.DelaiRelanceMinimumJours,
            Parametres.DelaiRelanceMaximumJours);
        if (!Parametres.TaillesPageAutorisees.Contains(parametres.TaillePage))
        {
            parametres.TaillePage = Parametres.TaillesPageAutorisees[0];
        }

        await using ContexteBaseDeDonnees contexte = await fabrique.CreateDbContextAsync(jeton);

        bool existe = await contexte.Parametres.AnyAsync(p => p.Identifiant == Parametres.IdentifiantUnique, jeton);
        if (existe)
        {
            contexte.Parametres.Update(parametres);
        }
        else
        {
            contexte.Parametres.Add(parametres);
        }

        await contexte.SaveChangesAsync(jeton);
    }
}
