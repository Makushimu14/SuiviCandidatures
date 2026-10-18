using Microsoft.EntityFrameworkCore;
using SuiviCandidatures.Coeur.Donnees;
using SuiviCandidatures.Coeur.Modeles;
using SuiviCandidatures.Coeur.Transferts;

namespace SuiviCandidatures.Coeur.Services;

public sealed class ServiceEntreprises(IDbContextFactory<ContexteBaseDeDonnees> fabrique) : IServiceEntreprises
{
    private const string CollationInsensibleCasse = "NOCASE";

    public async Task<IReadOnlyList<ResumeEntreprise>> ListerAsync(CancellationToken jeton = default)
    {
        await using ContexteBaseDeDonnees contexte = await fabrique.CreateDbContextAsync(jeton);

        return await contexte.Entreprises
            .AsNoTracking()
            .OrderBy(e => e.Nom)
            .Select(e => new ResumeEntreprise(e.Identifiant, e.Nom))
            .ToListAsync(jeton);
    }

    public async Task<IReadOnlyList<ResumeAdresse>> ObtenirAdressesAsync(int identifiantEntreprise, CancellationToken jeton = default)
    {
        await using ContexteBaseDeDonnees contexte = await fabrique.CreateDbContextAsync(jeton);

        var adresses = await contexte.Adresses
            .AsNoTracking()
            .Where(a => a.IdentifiantEntreprise == identifiantEntreprise)
            .OrderBy(a => a.Identifiant)
            .Select(a => new { a.Identifiant, a.Adresse, a.CodePostal, a.Ville })
            .ToListAsync(jeton);

        return adresses
            .Select(a => new ResumeAdresse(a.Identifiant, FormatageAdresse.ConstruireLibelle(a.Adresse, a.CodePostal, a.Ville)))
            .ToList();
    }

    public async Task<ResultatPagine<ElementListeEntreprise>> RechercherAsync(
        CritereRechercheEntreprises critere, CancellationToken jeton = default)
    {
        await using ContexteBaseDeDonnees contexte = await fabrique.CreateDbContextAsync(jeton);

        IQueryable<Entreprise> requete = contexte.Entreprises.AsNoTracking();

        string texte = critere.TexteRecherche?.Trim() ?? string.Empty;
        if (texte.Length > 0)
        {
            string motif = MotifsRecherche.Contient(texte);
            const string echappement = MotifsRecherche.CaractereEchappement;
            requete = requete.Where(e =>
                EF.Functions.Like(e.Nom, motif, echappement) ||
                (e.Secteur != null && EF.Functions.Like(e.Secteur, motif, echappement)) ||
                e.Adresses.Any(a => a.Ville != null && EF.Functions.Like(a.Ville, motif, echappement)));
        }

        int nombreTotal = await requete.CountAsync(jeton);
        int taillePage = Math.Max(1, critere.TaillePage);
        int nombrePages = Math.Max(1, (int)Math.Ceiling(nombreTotal / (double)taillePage));
        int numeroPage = Math.Clamp(critere.NumeroPage, 1, nombrePages);

        var lignes = await Trier(requete, critere.Tri, critere.Decroissant)
            .Skip((numeroPage - 1) * taillePage)
            .Take(taillePage)
            .Select(e => new
            {
                e.Identifiant,
                e.Nom,
                e.Secteur,
                Villes = e.Adresses.Select(a => a.Ville).ToList(),
                NombreCandidatures = e.Candidatures.Count
            })
            .ToListAsync(jeton);

        List<ElementListeEntreprise> elements = lignes
            .Select(l => new ElementListeEntreprise(
                l.Identifiant,
                l.Nom,
                l.Secteur,
                string.Join(", ", l.Villes.Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v!.Trim()).Distinct()),
                l.NombreCandidatures))
            .ToList();

        return new ResultatPagine<ElementListeEntreprise>(elements, nombreTotal, numeroPage, taillePage);
    }

    public async Task<DetailEntreprise?> ObtenirDetailAsync(int identifiant, CancellationToken jeton = default)
    {
        await using ContexteBaseDeDonnees contexte = await fabrique.CreateDbContextAsync(jeton);

        Entreprise? entreprise = await contexte.Entreprises
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Identifiant == identifiant, jeton);
        if (entreprise is null)
        {
            return null;
        }

        List<ElementAdresse> adresses = await contexte.Adresses
            .AsNoTracking()
            .Where(a => a.IdentifiantEntreprise == identifiant)
            .OrderBy(a => a.Identifiant)
            .Select(a => new ElementAdresse(a.Identifiant, a.Adresse, a.CodePostal, a.Ville, a.Mail, a.Telephone))
            .ToListAsync(jeton);

        List<ElementContactEntreprise> contacts = await contexte.Contacts
            .AsNoTracking()
            .Where(c => c.IdentifiantEntreprise == identifiant)
            .OrderBy(c => c.Nom).ThenBy(c => c.Prenom)
            .Select(c => new ElementContactEntreprise(
                c.Identifiant, c.Nom, c.Prenom, c.Role, c.Mail, c.Telephone, c.Notes, c.Candidatures.Count))
            .ToListAsync(jeton);

        List<ElementListeCandidature> candidatures = await contexte.Candidatures
            .AsNoTracking()
            .Where(c => c.IdentifiantEntreprise == identifiant)
            .OrderByDescending(c => c.DateCandidature).ThenByDescending(c => c.Identifiant)
            .Select(c => new ElementListeCandidature(
                c.Identifiant, c.TitreOffre, c.Entreprise.Nom, c.DateCandidature, c.TypeContrat, c.Statut))
            .ToListAsync(jeton);

        return new DetailEntreprise(
            entreprise.Identifiant,
            entreprise.Nom,
            entreprise.SiteWeb,
            entreprise.Secteur,
            entreprise.Notes,
            adresses,
            contacts,
            candidatures);
    }

    public async Task<DonneesEntreprise?> ObtenirPourModificationAsync(int identifiant, CancellationToken jeton = default)
    {
        await using ContexteBaseDeDonnees contexte = await fabrique.CreateDbContextAsync(jeton);

        Entreprise? entreprise = await contexte.Entreprises
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Identifiant == identifiant, jeton);

        return entreprise is null
            ? null
            : new DonneesEntreprise
            {
                Identifiant = entreprise.Identifiant,
                Nom = entreprise.Nom,
                SiteWeb = entreprise.SiteWeb,
                Secteur = entreprise.Secteur,
                Notes = entreprise.Notes
            };
    }

    public async Task<int> EnregistrerAsync(DonneesEntreprise donnees, CancellationToken jeton = default)
    {
        string nom = donnees.Nom.Trim();
        if (nom.Length == 0)
        {
            throw new ExceptionValidationMetier("Le nom de l'entreprise est obligatoire.");
        }

        string? siteWeb = LiensWeb.Normaliser(donnees.SiteWeb);
        if (siteWeb is not null && !LiensWeb.EstValide(siteWeb))
        {
            throw new ExceptionValidationMetier("Le site web doit être une adresse web valide (http ou https).");
        }

        await using ContexteBaseDeDonnees contexte = await fabrique.CreateDbContextAsync(jeton);

        List<ResumeEntreprise> resumes = await contexte.Entreprises
            .AsNoTracking()
            .Select(e => new ResumeEntreprise(e.Identifiant, e.Nom))
            .ToListAsync(jeton);
        ResumeEntreprise? homonyme = CorrespondanceNomsEntreprises.TrouverExacte(resumes, nom);
        if (homonyme is not null && homonyme.Identifiant != donnees.Identifiant)
        {
            throw new ExceptionValidationMetier("Une entreprise portant ce nom existe déjà.");
        }

        Entreprise entreprise;
        if (donnees.Identifiant == 0)
        {
            entreprise = new Entreprise();
            contexte.Entreprises.Add(entreprise);
        }
        else
        {
            entreprise = await contexte.Entreprises.FirstOrDefaultAsync(e => e.Identifiant == donnees.Identifiant, jeton)
                ?? throw new ExceptionValidationMetier("Cette entreprise n'existe plus.");
        }

        entreprise.Nom = nom;
        entreprise.SiteWeb = siteWeb;
        entreprise.Secteur = Nettoyer(donnees.Secteur);
        entreprise.Notes = Nettoyer(donnees.Notes);

        await contexte.SaveChangesAsync(jeton);
        return entreprise.Identifiant;
    }

    public async Task SupprimerAsync(int identifiant, CancellationToken jeton = default)
    {
        await using ContexteBaseDeDonnees contexte = await fabrique.CreateDbContextAsync(jeton);

        // Adresses, contacts, candidatures, et leurs actions, offres et liaisons : supprimés en cascade par la base.
        await contexte.Entreprises.Where(e => e.Identifiant == identifiant).ExecuteDeleteAsync(jeton);
    }

    private static IOrderedQueryable<Entreprise> Trier(IQueryable<Entreprise> requete, CritereTriEntreprises tri, bool decroissant)
    {
        IOrderedQueryable<Entreprise> trie = tri switch
        {
            CritereTriEntreprises.NombreCandidatures => decroissant
                ? requete.OrderByDescending(e => e.Candidatures.Count)
                : requete.OrderBy(e => e.Candidatures.Count),
            _ => decroissant
                ? requete.OrderByDescending(e => EF.Functions.Collate(e.Nom, CollationInsensibleCasse))
                : requete.OrderBy(e => EF.Functions.Collate(e.Nom, CollationInsensibleCasse))
        };

        // Critère de départage : garantit un ordre stable d'une page à l'autre.
        return decroissant ? trie.ThenByDescending(e => e.Identifiant) : trie.ThenBy(e => e.Identifiant);
    }

    private static string? Nettoyer(string? valeur) => string.IsNullOrWhiteSpace(valeur) ? null : valeur.Trim();
}
