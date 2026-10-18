using Microsoft.EntityFrameworkCore;
using SuiviCandidatures.Coeur.Modeles;

namespace SuiviCandidatures.Coeur.Donnees;

public class ContexteBaseDeDonnees(DbContextOptions<ContexteBaseDeDonnees> options) : DbContext(options)
{
    public DbSet<Entreprise> Entreprises => Set<Entreprise>();
    public DbSet<AdresseEntreprise> Adresses => Set<AdresseEntreprise>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Candidature> Candidatures => Set<Candidature>();
    public DbSet<OffreCandidature> Offres => Set<OffreCandidature>();
    public DbSet<ActionCandidature> Actions => Set<ActionCandidature>();
    public DbSet<Parametres> Parametres => Set<Parametres>();

    protected override void OnModelCreating(ModelBuilder constructeur)
    {
        // EF Core ne reconnaît par convention que « Id » ou « <Type>Id » : les clés primaires
        // nommées « Identifiant » doivent donc être déclarées explicitement.
        constructeur.Entity<Entreprise>(entite =>
        {
            entite.HasKey(e => e.Identifiant);
            entite.Property(e => e.Nom).IsRequired().HasMaxLength(200);
            entite.HasIndex(e => e.Nom);

            entite.HasMany(e => e.Adresses).WithOne(a => a.Entreprise)
                .HasForeignKey(a => a.IdentifiantEntreprise).OnDelete(DeleteBehavior.Cascade);
            entite.HasMany(e => e.Contacts).WithOne(c => c.Entreprise)
                .HasForeignKey(c => c.IdentifiantEntreprise).OnDelete(DeleteBehavior.Cascade);
            entite.HasMany(e => e.Candidatures).WithOne(c => c.Entreprise)
                .HasForeignKey(c => c.IdentifiantEntreprise).OnDelete(DeleteBehavior.Cascade);
        });

        constructeur.Entity<AdresseEntreprise>(entite =>
        {
            entite.HasKey(a => a.Identifiant);
            // Supprimer une adresse conserve les candidatures, qui perdent simplement leur adresse.
            entite.HasMany(a => a.Candidatures).WithOne(c => c.Adresse)
                .HasForeignKey(c => c.IdentifiantAdresse).OnDelete(DeleteBehavior.SetNull);
        });

        constructeur.Entity<Contact>(entite =>
        {
            entite.HasKey(c => c.Identifiant);
            entite.Property(c => c.Nom).IsRequired().HasMaxLength(200);
        });

        constructeur.Entity<Candidature>(entite =>
        {
            entite.HasKey(c => c.Identifiant);
            entite.Property(c => c.TitreOffre).IsRequired().HasMaxLength(300);
            entite.Property(c => c.Statut).HasConversion<string>().HasMaxLength(50);
            entite.Property(c => c.TypeCandidature).HasConversion<string>().HasMaxLength(50);
            entite.Property(c => c.TypeContrat).HasConversion<string>().HasMaxLength(50);
            entite.HasIndex(c => c.Statut);
            entite.HasIndex(c => c.DateCandidature);

            entite.HasMany(c => c.Offres).WithOne(o => o.Candidature)
                .HasForeignKey(o => o.IdentifiantCandidature).OnDelete(DeleteBehavior.Cascade);
            entite.HasMany(c => c.Actions).WithOne(a => a.Candidature)
                .HasForeignKey(a => a.IdentifiantCandidature).OnDelete(DeleteBehavior.Cascade);
            // Supprimer une candidature ou un contact supprime seulement les liaisons, jamais l'autre côté.
            entite.HasMany(c => c.Contacts).WithMany(c => c.Candidatures)
                .UsingEntity(jointure => jointure.ToTable("ContactsCandidatures"));
        });

        constructeur.Entity<ActionCandidature>(entite =>
        {
            entite.HasKey(a => a.Identifiant);
            entite.Property(a => a.Type).HasConversion<string>().HasMaxLength(50);
            entite.Property(a => a.AncienStatut).HasConversion<string>().HasMaxLength(50);
            entite.Property(a => a.NouveauStatut).HasConversion<string>().HasMaxLength(50);
            entite.HasIndex(a => a.Date);
        });

        constructeur.Entity<OffreCandidature>(entite =>
        {
            entite.HasKey(o => o.Identifiant);
        });

        constructeur.Entity<Parametres>(entite =>
        {
            entite.HasKey(p => p.Identifiant);
            entite.Property(p => p.Identifiant).ValueGeneratedNever();
            entite.Property(p => p.Theme).HasConversion<string>().HasMaxLength(50);
            entite.Property(p => p.NotificationsActivees).HasDefaultValue(true).ValueGeneratedNever();
        });
    }
}
