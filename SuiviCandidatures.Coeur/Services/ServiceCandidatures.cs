using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SuiviCandidatures.Coeur.Donnees;
using SuiviCandidatures.Coeur.Modeles;
using SuiviCandidatures.Coeur.Modeles.Enumerations;
using SuiviCandidatures.Coeur.Transferts;

namespace SuiviCandidatures.Coeur.Services;

public sealed class ServiceCandidatures(IDbContextFactory<ContexteBaseDeDonnees> fabrique) : IServiceCandidatures
{
    private const string CaractereEchappement = "\\";

    /// <summary>
    /// Les statuts sont stockés en texte : trier dessus donnerait un ordre alphabétique.
    /// Cette expression les range dans l'ordre de déclaration de l'énumération, traduite en CASE SQL.
    /// </summary>
    private static readonly Expression<Func<Candidature, int>> RangStatut = ConstruireRangStatut();

    public async Task<ResultatPagine<ElementListeCandidature>> RechercherAsync(
        CritereRechercheCandidatures critere, CancellationToken jeton = default)
    {
        await using ContexteBaseDeDonnees contexte = await fabrique.CreateDbContextAsync(jeton);

        IQueryable<Candidature> requete = contexte.Candidatures.AsNoTracking();

        string texte = critere.TexteRecherche?.Trim() ?? string.Empty;
        if (texte.Length > 0)
        {
            string motif = $"%{EchapperMotif(texte)}%";
            // Titre, entreprise, et offres liées : la référence ou le lien d'une offre permet de retrouver
            // une candidature déjà envoyée pour cette offre.
            requete = requete.Where(c =>
                EF.Functions.Like(c.TitreOffre, motif, CaractereEchappement) ||
                EF.Functions.Like(c.Entreprise.Nom, motif, CaractereEchappement) ||
                c.Offres.Any(o =>
                    (o.Reference != null && EF.Functions.Like(o.Reference, motif, CaractereEchappement)) ||
                    (o.Lien != null && EF.Functions.Like(o.Lien, motif, CaractereEchappement))));
        }

        if (critere.Statut is { } statut)
        {
            requete = requete.Where(c => c.Statut == statut);
        }

        int nombreTotal = await requete.CountAsync(jeton);
        int taillePage = Math.Max(1, critere.TaillePage);
        int nombrePages = Math.Max(1, (int)Math.Ceiling(nombreTotal / (double)taillePage));
        int numeroPage = Math.Clamp(critere.NumeroPage, 1, nombrePages);

        List<ElementListeCandidature> elements = await Trier(requete, critere.Tri, critere.Decroissant)
            .Skip((numeroPage - 1) * taillePage)
            .Take(taillePage)
            .Select(c => new ElementListeCandidature(
                c.Identifiant, c.TitreOffre, c.Entreprise.Nom, c.DateCandidature, c.TypeContrat, c.Statut))
            .ToListAsync(jeton);

        return new ResultatPagine<ElementListeCandidature>(elements, nombreTotal, numeroPage, taillePage);
    }

    public async Task<IReadOnlyDictionary<StatutCandidature, int>> CompterParStatutAsync(CancellationToken jeton = default)
    {
        await using ContexteBaseDeDonnees contexte = await fabrique.CreateDbContextAsync(jeton);

        var groupes = await contexte.Candidatures
            .AsNoTracking()
            .GroupBy(c => c.Statut)
            .Select(g => new { Statut = g.Key, Nombre = g.Count() })
            .ToListAsync(jeton);

        return groupes.ToDictionary(g => g.Statut, g => g.Nombre);
    }

    public async Task<DonneesCandidature?> ObtenirPourModificationAsync(int identifiant, CancellationToken jeton = default)
    {
        await using ContexteBaseDeDonnees contexte = await fabrique.CreateDbContextAsync(jeton);

        Candidature? candidature = await contexte.Candidatures
            .AsNoTracking()
            .Include(c => c.Entreprise)
            .FirstOrDefaultAsync(c => c.Identifiant == identifiant, jeton);

        return candidature is null
            ? null
            : new DonneesCandidature
            {
                Identifiant = candidature.Identifiant,
                TitreOffre = candidature.TitreOffre,
                DateCandidature = candidature.DateCandidature,
                Statut = candidature.Statut,
                TypeCandidature = candidature.TypeCandidature,
                TypeContrat = candidature.TypeContrat,
                Commentaires = candidature.Commentaires,
                NomEntreprise = candidature.Entreprise.Nom,
                IdentifiantAdresse = candidature.IdentifiantAdresse
            };
    }

    public async Task<int> EnregistrerAsync(DonneesCandidature donnees, CancellationToken jeton = default)
    {
        string titre = donnees.TitreOffre.Trim();
        string nomEntreprise = donnees.NomEntreprise.Trim();

        if (titre.Length == 0)
        {
            throw new ExceptionValidationMetier("Le titre de l'offre est obligatoire.");
        }

        if (nomEntreprise.Length == 0)
        {
            throw new ExceptionValidationMetier("Le nom de l'entreprise est obligatoire.");
        }

        await using ContexteBaseDeDonnees contexte = await fabrique.CreateDbContextAsync(jeton);

        // Entreprise : existante (comparaison sans casse ni accents) ou à créer.
        List<ResumeEntreprise> resumes = await contexte.Entreprises
            .AsNoTracking()
            .Select(e => new ResumeEntreprise(e.Identifiant, e.Nom))
            .ToListAsync(jeton);
        ResumeEntreprise? existante = CorrespondanceNomsEntreprises.TrouverExacte(resumes, nomEntreprise);

        Entreprise entreprise;
        if (existante is null)
        {
            entreprise = new Entreprise
            {
                Nom = nomEntreprise,
                SiteWeb = Nettoyer(donnees.NouvelleEntreprise?.SiteWeb),
                Secteur = Nettoyer(donnees.NouvelleEntreprise?.Secteur),
                Notes = Nettoyer(donnees.NouvelleEntreprise?.Notes)
            };
            contexte.Entreprises.Add(entreprise);
        }
        else
        {
            entreprise = await contexte.Entreprises
                .Include(e => e.Adresses)
                .FirstAsync(e => e.Identifiant == existante.Identifiant, jeton);
        }

        // Adresse : nouvelle, ou existante ET appartenant bien à cette entreprise.
        AdresseEntreprise? adresse = null;
        if (donnees.NouvelleAdresse is { EstVide: false } nouvelle)
        {
            ValidationAdresse.Valider(nouvelle);
            adresse = new AdresseEntreprise
            {
                Adresse = Nettoyer(nouvelle.Adresse),
                CodePostal = Nettoyer(nouvelle.CodePostal),
                Ville = Nettoyer(nouvelle.Ville),
                Mail = Nettoyer(nouvelle.Mail),
                Telephone = Nettoyer(nouvelle.Telephone)
            };
            entreprise.Adresses.Add(adresse);
        }
        else if (donnees.IdentifiantAdresse is { } identifiantAdresse)
        {
            adresse = entreprise.Adresses.FirstOrDefault(a => a.Identifiant == identifiantAdresse)
                ?? throw new ExceptionValidationMetier("L'adresse choisie n'appartient pas à cette entreprise.");
        }

        // Candidature : création ou modification.
        bool estCreation = donnees.Identifiant == 0;
        Candidature candidature;
        StatutCandidature? ancienStatut = null;
        if (estCreation)
        {
            candidature = new Candidature();
            contexte.Candidatures.Add(candidature);
        }
        else
        {
            candidature = await contexte.Candidatures.FirstOrDefaultAsync(c => c.Identifiant == donnees.Identifiant, jeton)
                ?? throw new ExceptionValidationMetier("Cette candidature n'existe plus.");
            ancienStatut = candidature.Statut;
        }

        candidature.TitreOffre = titre;
        candidature.DateCandidature = donnees.DateCandidature.Date;
        candidature.Statut = donnees.Statut;
        candidature.TypeCandidature = donnees.TypeCandidature;
        candidature.TypeContrat = donnees.TypeContrat;
        candidature.Commentaires = Nettoyer(donnees.Commentaires);
        candidature.Entreprise = entreprise;
        candidature.Adresse = adresse;

        // Historique : action initiale à la création, changement de statut à la modification.
        if (estCreation)
        {
            contexte.Actions.Add(new ActionCandidature
            {
                Candidature = candidature,
                Type = TypeAction.CandidatureEnvoyee,
                Date = candidature.DateCandidature
            });
        }
        else if (ancienStatut != candidature.Statut)
        {
            contexte.Actions.Add(new ActionCandidature
            {
                Candidature = candidature,
                Type = TypeAction.ChangementStatut,
                Date = DateTime.Now,
                AncienStatut = ancienStatut,
                NouveauStatut = candidature.Statut
            });
        }

        await contexte.SaveChangesAsync(jeton);
        return candidature.Identifiant;
    }

    private static IOrderedQueryable<Candidature> Trier(IQueryable<Candidature> requete, CritereTriCandidatures tri, bool decroissant)
    {
        IOrderedQueryable<Candidature> trie = tri switch
        {
            CritereTriCandidatures.Statut => decroissant
                ? requete.OrderByDescending(RangStatut) : requete.OrderBy(RangStatut),
            CritereTriCandidatures.TypeContrat => decroissant
                ? requete.OrderByDescending(c => c.TypeContrat) : requete.OrderBy(c => c.TypeContrat),
            CritereTriCandidatures.Entreprise => decroissant
                ? requete.OrderByDescending(c => c.Entreprise.Nom) : requete.OrderBy(c => c.Entreprise.Nom),
            _ => decroissant
                ? requete.OrderByDescending(c => c.DateCandidature) : requete.OrderBy(c => c.DateCandidature)
        };

        // Critère de départage : garantit un ordre stable d'une page à l'autre.
        return decroissant ? trie.ThenByDescending(c => c.Identifiant) : trie.ThenBy(c => c.Identifiant);
    }

    private static Expression<Func<Candidature, int>> ConstruireRangStatut()
    {
        ParameterExpression parametre = Expression.Parameter(typeof(Candidature), "c");
        MemberExpression statut = Expression.Property(parametre, nameof(Candidature.Statut));

        Expression corps = Expression.Constant(0);
        foreach (StatutCandidature valeur in Enum.GetValues<StatutCandidature>().OrderByDescending(v => (int)v))
        {
            corps = Expression.Condition(
                Expression.Equal(statut, Expression.Constant(valeur)),
                Expression.Constant((int)valeur),
                corps);
        }

        return Expression.Lambda<Func<Candidature, int>>(corps, parametre);
    }

    private static string EchapperMotif(string texte) => texte
        .Replace(CaractereEchappement, CaractereEchappement + CaractereEchappement)
        .Replace("%", CaractereEchappement + "%")
        .Replace("_", CaractereEchappement + "_");

    private static string? Nettoyer(string? valeur) => string.IsNullOrWhiteSpace(valeur) ? null : valeur.Trim();
}
