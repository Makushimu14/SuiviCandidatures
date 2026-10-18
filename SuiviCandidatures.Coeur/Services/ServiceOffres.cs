using Microsoft.EntityFrameworkCore;
using SuiviCandidatures.Coeur.Donnees;
using SuiviCandidatures.Coeur.Modeles;
using SuiviCandidatures.Coeur.Transferts;

namespace SuiviCandidatures.Coeur.Services;

public sealed class ServiceOffres(IDbContextFactory<ContexteBaseDeDonnees> fabrique) : IServiceOffres
{
    public async Task EnregistrerAsync(DonneesOffre donnees, CancellationToken jeton = default)
    {
        string? nomSite = Nettoyer(donnees.NomSite);
        string? reference = Nettoyer(donnees.Reference);
        string? lien = LiensWeb.Normaliser(donnees.Lien);

        if (nomSite is null && reference is null && lien is null)
        {
            throw new ExceptionValidationMetier("Renseignez au moins le site, la référence ou le lien de l'offre.");
        }

        if (lien is not null && !LiensWeb.EstValide(lien))
        {
            throw new ExceptionValidationMetier("Le lien doit être une adresse web valide (http ou https).");
        }

        await using ContexteBaseDeDonnees contexte = await fabrique.CreateDbContextAsync(jeton);

        bool candidatureExiste = await contexte.Candidatures.AnyAsync(c => c.Identifiant == donnees.IdentifiantCandidature, jeton);
        if (!candidatureExiste)
        {
            throw new ExceptionValidationMetier("Cette candidature n'existe plus.");
        }

        OffreCandidature offre;
        if (donnees.Identifiant == 0)
        {
            offre = new OffreCandidature { IdentifiantCandidature = donnees.IdentifiantCandidature };
            contexte.Offres.Add(offre);
        }
        else
        {
            offre = await contexte.Offres.FirstOrDefaultAsync(
                    o => o.Identifiant == donnees.Identifiant && o.IdentifiantCandidature == donnees.IdentifiantCandidature, jeton)
                ?? throw new ExceptionValidationMetier("Cette offre n'existe plus.");
        }

        offre.NomSite = nomSite;
        offre.Reference = reference;
        offre.Lien = lien;

        await contexte.SaveChangesAsync(jeton);
    }

    public async Task SupprimerAsync(int identifiantOffre, CancellationToken jeton = default)
    {
        await using ContexteBaseDeDonnees contexte = await fabrique.CreateDbContextAsync(jeton);
        await contexte.Offres.Where(o => o.Identifiant == identifiantOffre).ExecuteDeleteAsync(jeton);
    }

    private static string? Nettoyer(string? valeur) => string.IsNullOrWhiteSpace(valeur) ? null : valeur.Trim();
}
