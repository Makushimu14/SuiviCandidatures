using Microsoft.EntityFrameworkCore;
using SuiviCandidatures.Coeur.Donnees;
using SuiviCandidatures.Coeur.Modeles;
using SuiviCandidatures.Coeur.Transferts;

namespace SuiviCandidatures.Coeur.Services;

public sealed class ServiceAdresses(IDbContextFactory<ContexteBaseDeDonnees> fabrique) : IServiceAdresses
{
    public async Task EnregistrerAsync(DonneesAdresseEntreprise donnees, CancellationToken jeton = default)
    {
        DonneesAdresse saisie = new(donnees.Adresse, donnees.CodePostal, donnees.Ville, donnees.Mail, donnees.Telephone);
        if (saisie.EstVide)
        {
            throw new ExceptionValidationMetier("Renseignez au moins un champ de l'adresse.");
        }

        ValidationAdresse.Valider(saisie);

        await using ContexteBaseDeDonnees contexte = await fabrique.CreateDbContextAsync(jeton);

        bool entrepriseExiste = await contexte.Entreprises.AnyAsync(e => e.Identifiant == donnees.IdentifiantEntreprise, jeton);
        if (!entrepriseExiste)
        {
            throw new ExceptionValidationMetier("Cette entreprise n'existe plus.");
        }

        AdresseEntreprise adresse;
        if (donnees.Identifiant == 0)
        {
            adresse = new AdresseEntreprise { IdentifiantEntreprise = donnees.IdentifiantEntreprise };
            contexte.Adresses.Add(adresse);
        }
        else
        {
            adresse = await contexte.Adresses.FirstOrDefaultAsync(
                    a => a.Identifiant == donnees.Identifiant && a.IdentifiantEntreprise == donnees.IdentifiantEntreprise, jeton)
                ?? throw new ExceptionValidationMetier("Cette adresse n'existe plus.");
        }

        adresse.Adresse = Nettoyer(saisie.Adresse);
        adresse.CodePostal = Nettoyer(saisie.CodePostal);
        adresse.Ville = Nettoyer(saisie.Ville);
        adresse.Mail = Nettoyer(saisie.Mail);
        adresse.Telephone = Nettoyer(saisie.Telephone);

        await contexte.SaveChangesAsync(jeton);
    }

    public async Task SupprimerAsync(int identifiantAdresse, CancellationToken jeton = default)
    {
        await using ContexteBaseDeDonnees contexte = await fabrique.CreateDbContextAsync(jeton);

        // Les candidatures rattachées perdent leur adresse (SET NULL), par la base.
        await contexte.Adresses.Where(a => a.Identifiant == identifiantAdresse).ExecuteDeleteAsync(jeton);
    }

    private static string? Nettoyer(string? valeur) => string.IsNullOrWhiteSpace(valeur) ? null : valeur.Trim();
}
