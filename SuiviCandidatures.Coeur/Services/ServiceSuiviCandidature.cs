using Microsoft.EntityFrameworkCore;
using SuiviCandidatures.Coeur.Donnees;
using SuiviCandidatures.Coeur.Extensions;
using SuiviCandidatures.Coeur.Modeles;
using SuiviCandidatures.Coeur.Modeles.Enumerations;
using SuiviCandidatures.Coeur.Transferts;

namespace SuiviCandidatures.Coeur.Services;

public sealed class ServiceSuiviCandidature(IDbContextFactory<ContexteBaseDeDonnees> fabrique) : IServiceSuiviCandidature
{
    public async Task<DetailCandidature?> ObtenirDetailAsync(int identifiant, CancellationToken jeton = default)
    {
        await using ContexteBaseDeDonnees contexte = await fabrique.CreateDbContextAsync(jeton);

        Candidature? candidature = await contexte.Candidatures
            .AsNoTracking()
            .Include(c => c.Entreprise)
            .Include(c => c.Adresse)
            .Include(c => c.Actions)
            .Include(c => c.Offres)
            .Include(c => c.Contacts)
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Identifiant == identifiant, jeton);

        if (candidature is null)
        {
            return null;
        }

        List<ElementAction> actions = candidature.Actions
            .OrderByDescending(a => a.Date)
            .ThenByDescending(a => a.Identifiant)
            .Select(a => new ElementAction(a.Identifiant, a.Date, a.Type, a.Note, a.AncienStatut, a.NouveauStatut))
            .ToList();

        List<ElementOffre> offres = candidature.Offres
            .OrderBy(o => o.Identifiant)
            .Select(o => new ElementOffre(o.Identifiant, o.NomSite, o.Reference, o.Lien))
            .ToList();

        List<ElementContact> contacts = candidature.Contacts
            .OrderBy(c => c.Nom).ThenBy(c => c.Prenom)
            .Select(c => new ElementContact(c.Identifiant, c.Nom, c.Prenom, c.Role, c.Mail, c.Telephone, c.Notes))
            .ToList();

        string? libelleAdresse = candidature.Adresse is null
            ? null
            : FormatageAdresse.ConstruireLibelle(
                candidature.Adresse.Adresse, candidature.Adresse.CodePostal, candidature.Adresse.Ville);

        return new DetailCandidature(
            candidature.Identifiant,
            candidature.TitreOffre,
            candidature.IdentifiantEntreprise,
            candidature.Entreprise.Nom,
            candidature.DateCandidature,
            candidature.Statut,
            candidature.TypeCandidature,
            candidature.TypeContrat,
            libelleAdresse,
            candidature.Commentaires,
            candidature.DelaiRelanceJours,
            candidature.RelancesDesactivees,
            actions,
            offres,
            contacts);
    }

    public async Task ChangerStatutAsync(int identifiant, StatutCandidature nouveauStatut, CancellationToken jeton = default)
    {
        await using ContexteBaseDeDonnees contexte = await fabrique.CreateDbContextAsync(jeton);

        Candidature candidature = await contexte.Candidatures.FirstOrDefaultAsync(c => c.Identifiant == identifiant, jeton)
            ?? throw new ExceptionValidationMetier("Cette candidature n'existe plus.");

        if (candidature.Statut == nouveauStatut)
        {
            return;
        }

        contexte.Actions.Add(new ActionCandidature
        {
            IdentifiantCandidature = candidature.Identifiant,
            Type = TypeAction.ChangementStatut,
            Date = DateTime.Now,
            AncienStatut = candidature.Statut,
            NouveauStatut = nouveauStatut
        });
        candidature.Statut = nouveauStatut;

        await contexte.SaveChangesAsync(jeton);
    }

    public async Task EnregistrerActionAsync(DonneesAction donnees, CancellationToken jeton = default)
    {
        if (!donnees.Type.EstManuelle())
        {
            throw new ExceptionValidationMetier("Ce type d'action ne peut pas être saisi manuellement.");
        }

        string? note = Nettoyer(donnees.Note);
        if (donnees.Type == TypeAction.Autre && note is null)
        {
            throw new ExceptionValidationMetier("Une note est obligatoire pour une action de type « Autre ».");
        }

        await using ContexteBaseDeDonnees contexte = await fabrique.CreateDbContextAsync(jeton);

        bool candidatureExiste = await contexte.Candidatures.AnyAsync(c => c.Identifiant == donnees.IdentifiantCandidature, jeton);
        if (!candidatureExiste)
        {
            throw new ExceptionValidationMetier("Cette candidature n'existe plus.");
        }

        if (donnees.Identifiant == 0)
        {
            contexte.Actions.Add(new ActionCandidature
            {
                IdentifiantCandidature = donnees.IdentifiantCandidature,
                Type = donnees.Type,
                Date = donnees.Date,
                Note = note
            });
        }
        else
        {
            ActionCandidature action = await contexte.Actions.FirstOrDefaultAsync(
                    a => a.Identifiant == donnees.Identifiant && a.IdentifiantCandidature == donnees.IdentifiantCandidature, jeton)
                ?? throw new ExceptionValidationMetier("Cette action n'existe plus.");

            if (!action.Type.EstManuelle())
            {
                throw new ExceptionValidationMetier("Cette action est automatique et ne peut pas être modifiée.");
            }

            action.Type = donnees.Type;
            action.Date = donnees.Date;
            action.Note = note;
        }

        await contexte.SaveChangesAsync(jeton);
    }

    public async Task SupprimerActionAsync(int identifiantAction, CancellationToken jeton = default)
    {
        await using ContexteBaseDeDonnees contexte = await fabrique.CreateDbContextAsync(jeton);

        ActionCandidature action = await contexte.Actions.FirstOrDefaultAsync(a => a.Identifiant == identifiantAction, jeton)
            ?? throw new ExceptionValidationMetier("Cette action n'existe plus.");

        if (!action.Type.EstManuelle())
        {
            throw new ExceptionValidationMetier("Cette action est automatique et ne peut pas être supprimée.");
        }

        contexte.Actions.Remove(action);
        await contexte.SaveChangesAsync(jeton);
    }

    public async Task EnregistrerParametresRelanceAsync(
        int identifiant, int? delaiRelanceJours, bool relancesDesactivees, CancellationToken jeton = default)
    {
        if (delaiRelanceJours is { } delai &&
            (delai < Parametres.DelaiRelanceMinimumJours || delai > Parametres.DelaiRelanceMaximumJours))
        {
            throw new ExceptionValidationMetier(
                $"Le délai de relance doit être compris entre {Parametres.DelaiRelanceMinimumJours} et {Parametres.DelaiRelanceMaximumJours} jours.");
        }

        await using ContexteBaseDeDonnees contexte = await fabrique.CreateDbContextAsync(jeton);

        Candidature candidature = await contexte.Candidatures.FirstOrDefaultAsync(c => c.Identifiant == identifiant, jeton)
            ?? throw new ExceptionValidationMetier("Cette candidature n'existe plus.");

        candidature.DelaiRelanceJours = delaiRelanceJours;
        candidature.RelancesDesactivees = relancesDesactivees;

        await contexte.SaveChangesAsync(jeton);
    }

    public async Task SupprimerAsync(int identifiant, CancellationToken jeton = default)
    {
        await using ContexteBaseDeDonnees contexte = await fabrique.CreateDbContextAsync(jeton);

        // Les actions, les offres et les liaisons avec les contacts sont supprimées en cascade par la base. Les contacts sont conservés.
        await contexte.Candidatures.Where(c => c.Identifiant == identifiant).ExecuteDeleteAsync(jeton);
    }

    private static string? Nettoyer(string? valeur) => string.IsNullOrWhiteSpace(valeur) ? null : valeur.Trim();
}
