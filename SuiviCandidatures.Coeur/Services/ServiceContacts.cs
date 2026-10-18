using Microsoft.EntityFrameworkCore;
using SuiviCandidatures.Coeur.Donnees;
using SuiviCandidatures.Coeur.Modeles;
using SuiviCandidatures.Coeur.Transferts;

namespace SuiviCandidatures.Coeur.Services;

public sealed class ServiceContacts(IDbContextFactory<ContexteBaseDeDonnees> fabrique) : IServiceContacts
{
    public async Task<IReadOnlyList<ElementContact>> ListerDisponiblesAsync(int identifiantCandidature, CancellationToken jeton = default)
    {
        await using ContexteBaseDeDonnees contexte = await fabrique.CreateDbContextAsync(jeton);

        int? identifiantEntreprise = await contexte.Candidatures
            .Where(c => c.Identifiant == identifiantCandidature)
            .Select(c => (int?)c.IdentifiantEntreprise)
            .FirstOrDefaultAsync(jeton);
        if (identifiantEntreprise is null)
        {
            return [];
        }

        List<Contact> contacts = await contexte.Contacts
            .AsNoTracking()
            .Where(c => c.IdentifiantEntreprise == identifiantEntreprise
                && !c.Candidatures.Any(candidature => candidature.Identifiant == identifiantCandidature))
            .OrderBy(c => c.Nom).ThenBy(c => c.Prenom)
            .ToListAsync(jeton);

        return contacts
            .Select(c => new ElementContact(c.Identifiant, c.Nom, c.Prenom, c.Role, c.Mail, c.Telephone, c.Notes))
            .ToList();
    }

    public async Task EnregistrerAsync(DonneesContact donnees, CancellationToken jeton = default)
    {
        string nom = ValiderChamps(donnees.Nom, donnees.Mail, donnees.Telephone);
        string? mail = Nettoyer(donnees.Mail);

        await using ContexteBaseDeDonnees contexte = await fabrique.CreateDbContextAsync(jeton);

        Candidature candidature = await contexte.Candidatures
            .Include(c => c.Contacts)
            .FirstOrDefaultAsync(c => c.Identifiant == donnees.IdentifiantCandidature, jeton)
            ?? throw new ExceptionValidationMetier("Cette candidature n'existe plus.");

        Contact contact;
        if (donnees.Identifiant == 0)
        {
            contact = new Contact { IdentifiantEntreprise = candidature.IdentifiantEntreprise };
            candidature.Contacts.Add(contact);
        }
        else
        {
            contact = candidature.Contacts.FirstOrDefault(c => c.Identifiant == donnees.Identifiant)
                ?? throw new ExceptionValidationMetier("Ce contact n'est pas lié à cette candidature.");
        }

        contact.Nom = nom;
        contact.Prenom = Nettoyer(donnees.Prenom);
        contact.Role = Nettoyer(donnees.Role);
        contact.Mail = mail;
        contact.Telephone = Nettoyer(donnees.Telephone);
        contact.Notes = Nettoyer(donnees.Notes);

        await contexte.SaveChangesAsync(jeton);
    }

    public async Task EnregistrerPourEntrepriseAsync(DonneesContactEntreprise donnees, CancellationToken jeton = default)
    {
        string nom = ValiderChamps(donnees.Nom, donnees.Mail, donnees.Telephone);

        await using ContexteBaseDeDonnees contexte = await fabrique.CreateDbContextAsync(jeton);

        bool entrepriseExiste = await contexte.Entreprises.AnyAsync(e => e.Identifiant == donnees.IdentifiantEntreprise, jeton);
        if (!entrepriseExiste)
        {
            throw new ExceptionValidationMetier("Cette entreprise n'existe plus.");
        }

        Contact contact;
        if (donnees.Identifiant == 0)
        {
            contact = new Contact { IdentifiantEntreprise = donnees.IdentifiantEntreprise };
            contexte.Contacts.Add(contact);
        }
        else
        {
            contact = await contexte.Contacts.FirstOrDefaultAsync(
                    c => c.Identifiant == donnees.Identifiant && c.IdentifiantEntreprise == donnees.IdentifiantEntreprise, jeton)
                ?? throw new ExceptionValidationMetier("Ce contact n'existe plus.");
        }

        contact.Nom = nom;
        contact.Prenom = Nettoyer(donnees.Prenom);
        contact.Role = Nettoyer(donnees.Role);
        contact.Mail = Nettoyer(donnees.Mail);
        contact.Telephone = Nettoyer(donnees.Telephone);
        contact.Notes = Nettoyer(donnees.Notes);

        await contexte.SaveChangesAsync(jeton);
    }

    public async Task SupprimerAsync(int identifiantContact, CancellationToken jeton = default)
    {
        await using ContexteBaseDeDonnees contexte = await fabrique.CreateDbContextAsync(jeton);

        // Les liaisons avec les candidatures sont supprimées en cascade par la base.
        await contexte.Contacts.Where(c => c.Identifiant == identifiantContact).ExecuteDeleteAsync(jeton);
    }

    public async Task LierAsync(int identifiantCandidature, int identifiantContact, CancellationToken jeton = default)
    {
        await using ContexteBaseDeDonnees contexte = await fabrique.CreateDbContextAsync(jeton);

        Candidature candidature = await contexte.Candidatures
            .Include(c => c.Contacts)
            .FirstOrDefaultAsync(c => c.Identifiant == identifiantCandidature, jeton)
            ?? throw new ExceptionValidationMetier("Cette candidature n'existe plus.");

        Contact contact = await contexte.Contacts.FirstOrDefaultAsync(c => c.Identifiant == identifiantContact, jeton)
            ?? throw new ExceptionValidationMetier("Ce contact n'existe plus.");

        if (contact.IdentifiantEntreprise != candidature.IdentifiantEntreprise)
        {
            throw new ExceptionValidationMetier("Ce contact appartient à une autre entreprise.");
        }

        if (candidature.Contacts.All(c => c.Identifiant != contact.Identifiant))
        {
            candidature.Contacts.Add(contact);
            await contexte.SaveChangesAsync(jeton);
        }
    }

    public async Task DelierAsync(int identifiantCandidature, int identifiantContact, CancellationToken jeton = default)
    {
        await using ContexteBaseDeDonnees contexte = await fabrique.CreateDbContextAsync(jeton);

        Candidature candidature = await contexte.Candidatures
            .Include(c => c.Contacts)
            .FirstOrDefaultAsync(c => c.Identifiant == identifiantCandidature, jeton)
            ?? throw new ExceptionValidationMetier("Cette candidature n'existe plus.");

        Contact? contact = candidature.Contacts.FirstOrDefault(c => c.Identifiant == identifiantContact);
        if (contact is not null)
        {
            candidature.Contacts.Remove(contact);
            await contexte.SaveChangesAsync(jeton);
        }
    }

    /// <summary>Valide les champs communs à tous les formulaires de contact et retourne le nom nettoyé.</summary>
    private static string ValiderChamps(string nom, string? mail, string? telephone)
    {
        string nomNettoye = nom.Trim();
        if (nomNettoye.Length == 0)
        {
            throw new ExceptionValidationMetier("Le nom du contact est obligatoire.");
        }

        ValidationAdresse.ValiderMail(mail);
        ValidationAdresse.ValiderTelephone(telephone);
        return nomNettoye;
    }

    private static string? Nettoyer(string? valeur) => string.IsNullOrWhiteSpace(valeur) ? null : valeur.Trim();
}
