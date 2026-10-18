using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SuiviCandidatures.Coeur.Donnees;
using SuiviCandidatures.Coeur.Modeles;
using SuiviCandidatures.Coeur.Sauvegarde;

namespace SuiviCandidatures.Coeur.Services;

public sealed class ServiceSauvegarde(IDbContextFactory<ContexteBaseDeDonnees> fabrique) : IServiceSauvegarde
{
    private static readonly JsonSerializerOptions OptionsJson = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task ExporterAsync(Stream destination, CancellationToken jeton = default)
    {
        ContenuSauvegarde contenu = await ConstruireContenuAsync(jeton);
        await JsonSerializer.SerializeAsync(destination, contenu, OptionsJson, jeton);
    }

    public async Task<ResumeSauvegarde> AnalyserAsync(Stream source, CancellationToken jeton = default) =>
        Resumer(await LireAsync(source, jeton));

    public async Task<ResumeSauvegarde> ImporterAsync(Stream source, CancellationToken jeton = default)
    {
        // Tout est lu et validé avant la première modification de la base.
        ContenuSauvegarde contenu = await LireAsync(source, jeton);

        await using ContexteBaseDeDonnees contexte = await fabrique.CreateDbContextAsync(jeton);
        await using IDbContextTransaction transaction = await contexte.Database.BeginTransactionAsync(jeton);

        // Sans validation de la transaction (exception ci-dessous), tout est annulé à la libération.
        await contexte.Candidatures.ExecuteDeleteAsync(jeton);
        await contexte.Entreprises.ExecuteDeleteAsync(jeton);

        Dictionary<int, Entreprise> entreprises = [];
        Dictionary<int, AdresseEntreprise> adresses = [];
        Dictionary<int, Contact> contacts = [];

        foreach (EntrepriseSauvegarde entrepriseSource in contenu.Entreprises)
        {
            Entreprise entreprise = new()
            {
                Nom = entrepriseSource.Nom.Trim(),
                SiteWeb = entrepriseSource.SiteWeb,
                Secteur = entrepriseSource.Secteur,
                Notes = entrepriseSource.Notes
            };

            foreach (AdresseSauvegarde adresseSource in entrepriseSource.Adresses)
            {
                AdresseEntreprise adresse = new()
                {
                    Adresse = adresseSource.Adresse,
                    CodePostal = adresseSource.CodePostal,
                    Ville = adresseSource.Ville,
                    Mail = adresseSource.Mail,
                    Telephone = adresseSource.Telephone
                };
                entreprise.Adresses.Add(adresse);
                adresses[adresseSource.Identifiant] = adresse;
            }

            foreach (ContactSauvegarde contactSource in entrepriseSource.Contacts)
            {
                Contact contact = new()
                {
                    Nom = contactSource.Nom.Trim(),
                    Prenom = contactSource.Prenom,
                    Role = contactSource.Role,
                    Mail = contactSource.Mail,
                    Telephone = contactSource.Telephone,
                    Notes = contactSource.Notes
                };
                entreprise.Contacts.Add(contact);
                contacts[contactSource.Identifiant] = contact;
            }

            entreprises[entrepriseSource.Identifiant] = entreprise;
        }

        List<Candidature> candidatures = [];
        foreach (CandidatureSauvegarde candidatureSource in contenu.Candidatures)
        {
            Candidature candidature = new()
            {
                TitreOffre = candidatureSource.TitreOffre.Trim(),
                DateCandidature = candidatureSource.DateCandidature,
                Statut = candidatureSource.Statut,
                TypeCandidature = candidatureSource.TypeCandidature,
                TypeContrat = candidatureSource.TypeContrat,
                Commentaires = candidatureSource.Commentaires,
                DelaiRelanceJours = candidatureSource.DelaiRelanceJours,
                RelancesDesactivees = candidatureSource.RelancesDesactivees,
                Entreprise = entreprises[candidatureSource.IdentifiantEntreprise],
                Adresse = candidatureSource.IdentifiantAdresse is { } identifiantAdresse ? adresses[identifiantAdresse] : null
            };

            foreach (OffreSauvegarde offre in candidatureSource.Offres)
            {
                candidature.Offres.Add(new OffreCandidature { NomSite = offre.NomSite, Reference = offre.Reference, Lien = offre.Lien });
            }

            foreach (ActionSauvegarde action in candidatureSource.Actions)
            {
                candidature.Actions.Add(new ActionCandidature
                {
                    Date = action.Date,
                    Type = action.Type,
                    Note = action.Note,
                    AncienStatut = action.AncienStatut,
                    NouveauStatut = action.NouveauStatut
                });
            }

            foreach (int identifiantContact in candidatureSource.IdentifiantsContacts)
            {
                candidature.Contacts.Add(contacts[identifiantContact]);
            }

            candidatures.Add(candidature);
        }

        contexte.Entreprises.AddRange(entreprises.Values);
        contexte.Candidatures.AddRange(candidatures);
        await contexte.SaveChangesAsync(jeton);
        await transaction.CommitAsync(jeton);

        return Resumer(contenu);
    }

    private async Task<ContenuSauvegarde> ConstruireContenuAsync(CancellationToken jeton)
    {
        await using ContexteBaseDeDonnees contexte = await fabrique.CreateDbContextAsync(jeton);

        List<Entreprise> entreprises = await contexte.Entreprises
            .AsNoTracking()
            .Include(e => e.Adresses)
            .Include(e => e.Contacts)
            .AsSplitQuery()
            .OrderBy(e => e.Identifiant)
            .ToListAsync(jeton);

        List<Candidature> candidatures = await contexte.Candidatures
            .AsNoTracking()
            .Include(c => c.Offres)
            .Include(c => c.Actions)
            .Include(c => c.Contacts)
            .AsSplitQuery()
            .OrderBy(c => c.Identifiant)
            .ToListAsync(jeton);

        return new ContenuSauvegarde
        {
            DateExport = DateTime.Now,
            Entreprises = entreprises.Select(e => new EntrepriseSauvegarde
            {
                Identifiant = e.Identifiant,
                Nom = e.Nom,
                SiteWeb = e.SiteWeb,
                Secteur = e.Secteur,
                Notes = e.Notes,
                Adresses = e.Adresses.OrderBy(a => a.Identifiant).Select(a => new AdresseSauvegarde
                {
                    Identifiant = a.Identifiant,
                    Adresse = a.Adresse,
                    CodePostal = a.CodePostal,
                    Ville = a.Ville,
                    Mail = a.Mail,
                    Telephone = a.Telephone
                }).ToList(),
                Contacts = e.Contacts.OrderBy(c => c.Identifiant).Select(c => new ContactSauvegarde
                {
                    Identifiant = c.Identifiant,
                    Nom = c.Nom,
                    Prenom = c.Prenom,
                    Role = c.Role,
                    Mail = c.Mail,
                    Telephone = c.Telephone,
                    Notes = c.Notes
                }).ToList()
            }).ToList(),
            Candidatures = candidatures.Select(c => new CandidatureSauvegarde
            {
                Identifiant = c.Identifiant,
                IdentifiantEntreprise = c.IdentifiantEntreprise,
                IdentifiantAdresse = c.IdentifiantAdresse,
                TitreOffre = c.TitreOffre,
                DateCandidature = c.DateCandidature,
                Statut = c.Statut,
                TypeCandidature = c.TypeCandidature,
                TypeContrat = c.TypeContrat,
                Commentaires = c.Commentaires,
                DelaiRelanceJours = c.DelaiRelanceJours,
                RelancesDesactivees = c.RelancesDesactivees,
                Offres = c.Offres.OrderBy(o => o.Identifiant).Select(o => new OffreSauvegarde
                {
                    NomSite = o.NomSite,
                    Reference = o.Reference,
                    Lien = o.Lien
                }).ToList(),
                Actions = c.Actions.OrderBy(a => a.Date).ThenBy(a => a.Identifiant).Select(a => new ActionSauvegarde
                {
                    Date = a.Date,
                    Type = a.Type,
                    Note = a.Note,
                    AncienStatut = a.AncienStatut,
                    NouveauStatut = a.NouveauStatut
                }).ToList(),
                IdentifiantsContacts = c.Contacts.Select(contact => contact.Identifiant).OrderBy(identifiant => identifiant).ToList()
            }).ToList()
        };
    }

    private static async Task<ContenuSauvegarde> LireAsync(Stream source, CancellationToken jeton)
    {
        ContenuSauvegarde? contenu;
        try
        {
            contenu = await JsonSerializer.DeserializeAsync<ContenuSauvegarde>(source, OptionsJson, jeton);
        }
        catch (Exception exception) when (exception is JsonException or NotSupportedException)
        {
            throw new ExceptionValidationMetier("Ce fichier n'est pas une sauvegarde valide de l'application.");
        }

        if (contenu is null)
        {
            throw new ExceptionValidationMetier("Ce fichier est vide.");
        }

        Valider(contenu);
        return contenu;
    }

    /// <summary>Vérifie la cohérence complète du fichier : version, unicité des identifiants, références et valeurs.</summary>
    private static void Valider(ContenuSauvegarde contenu)
    {
        if (contenu.Version != ContenuSauvegarde.VersionActuelle)
        {
            throw new ExceptionValidationMetier(
                $"Ce fichier utilise un format de sauvegarde non pris en charge (version {contenu.Version}).");
        }

        VerifierUnicite(contenu.Entreprises.Select(e => e.Identifiant), "entreprises");
        VerifierUnicite(contenu.Entreprises.SelectMany(e => e.Adresses).Select(a => a.Identifiant), "adresses");
        VerifierUnicite(contenu.Entreprises.SelectMany(e => e.Contacts).Select(c => c.Identifiant), "contacts");
        VerifierUnicite(contenu.Candidatures.Select(c => c.Identifiant), "candidatures");

        Dictionary<int, EntrepriseSauvegarde> entreprises = contenu.Entreprises.ToDictionary(e => e.Identifiant);

        foreach (EntrepriseSauvegarde entreprise in contenu.Entreprises)
        {
            if (string.IsNullOrWhiteSpace(entreprise.Nom))
            {
                throw new ExceptionValidationMetier("Le fichier contient une entreprise sans nom.");
            }

            if (entreprise.Contacts.Any(c => string.IsNullOrWhiteSpace(c.Nom)))
            {
                throw new ExceptionValidationMetier($"Le fichier contient un contact sans nom (entreprise « {entreprise.Nom} »).");
            }
        }

        foreach (CandidatureSauvegarde candidature in contenu.Candidatures)
        {
            if (string.IsNullOrWhiteSpace(candidature.TitreOffre))
            {
                throw new ExceptionValidationMetier("Le fichier contient une candidature sans titre d'offre.");
            }

            if (!entreprises.TryGetValue(candidature.IdentifiantEntreprise, out EntrepriseSauvegarde? entreprise))
            {
                throw new ExceptionValidationMetier(
                    $"La candidature « {candidature.TitreOffre} » référence une entreprise absente du fichier.");
            }

            if (candidature.IdentifiantAdresse is { } identifiantAdresse
                && entreprise.Adresses.All(a => a.Identifiant != identifiantAdresse))
            {
                throw new ExceptionValidationMetier(
                    $"La candidature « {candidature.TitreOffre} » référence une adresse qui n'appartient pas à son entreprise.");
            }

            if (candidature.IdentifiantsContacts.Any(id => entreprise.Contacts.All(c => c.Identifiant != id)))
            {
                throw new ExceptionValidationMetier(
                    $"La candidature « {candidature.TitreOffre} » référence un contact qui n'appartient pas à son entreprise.");
            }

            bool valeursValides =
                Enum.IsDefined(candidature.Statut)
                && Enum.IsDefined(candidature.TypeCandidature)
                && Enum.IsDefined(candidature.TypeContrat)
                && candidature.Actions.All(a =>
                    Enum.IsDefined(a.Type)
                    && (a.AncienStatut is null || Enum.IsDefined(a.AncienStatut.Value))
                    && (a.NouveauStatut is null || Enum.IsDefined(a.NouveauStatut.Value)));
            if (!valeursValides)
            {
                throw new ExceptionValidationMetier(
                    $"La candidature « {candidature.TitreOffre} » contient une valeur inconnue (statut ou type).");
            }
        }
    }

    private static void VerifierUnicite(IEnumerable<int> identifiants, string elements)
    {
        HashSet<int> vus = [];
        if (identifiants.Any(identifiant => !vus.Add(identifiant)))
        {
            throw new ExceptionValidationMetier($"Le fichier contient des {elements} en double.");
        }
    }

    private static ResumeSauvegarde Resumer(ContenuSauvegarde contenu) => new(
        contenu.Entreprises.Count,
        contenu.Candidatures.Count,
        contenu.Entreprises.Sum(e => e.Contacts.Count));
}
