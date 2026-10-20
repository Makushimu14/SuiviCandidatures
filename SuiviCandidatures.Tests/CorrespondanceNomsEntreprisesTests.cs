using SuiviCandidatures.Coeur.Services;
using SuiviCandidatures.Coeur.Transferts;

namespace SuiviCandidatures.Tests;

public class CorrespondanceNomsEntreprisesTests
{
    private static readonly ResumeEntreprise[] Entreprises =
    [
        new(1, "Société Générale"),
        new(2, "Acme"),
        new(3, "Acme France"),
        new(4, "Boulangerie Acme")
    ];

    [Theory]
    [InlineData("Société Générale")]
    [InlineData("societe generale")]
    [InlineData("  SOCIÉTÉ GÉNÉRALE  ")]
    public void Un_nom_est_reconnu_sans_tenir_compte_de_la_casse_des_accents_ni_des_espaces(string saisie)
    {
        ResumeEntreprise? trouvee = CorrespondanceNomsEntreprises.TrouverExacte(Entreprises, saisie);

        Assert.NotNull(trouvee);
        Assert.Equal(1, trouvee.Identifiant);
    }

    [Fact]
    public void Un_nom_partiel_n_est_pas_une_correspondance_exacte()
    {
        ResumeEntreprise? trouvee = CorrespondanceNomsEntreprises.TrouverExacte(Entreprises, "Acme");

        Assert.NotNull(trouvee);
        Assert.Equal(2, trouvee.Identifiant);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Inconnue")]
    public void Une_saisie_vide_ou_inconnue_ne_correspond_a_aucune_entreprise(string saisie)
    {
        Assert.Null(CorrespondanceNomsEntreprises.TrouverExacte(Entreprises, saisie));
    }

    [Fact]
    public void Les_suggestions_placent_en_premier_les_noms_qui_commencent_par_le_texte()
    {
        IReadOnlyList<ResumeEntreprise> suggestions = CorrespondanceNomsEntreprises.Suggerer(Entreprises, "acme", 10);

        Assert.Equal([2, 3, 4], suggestions.Select(s => s.Identifiant));
    }

    [Fact]
    public void Les_suggestions_ignorent_les_accents_et_respectent_le_maximum()
    {
        Assert.Equal([1], CorrespondanceNomsEntreprises.Suggerer(Entreprises, "societe", 10).Select(s => s.Identifiant));
        Assert.Equal(2, CorrespondanceNomsEntreprises.Suggerer(Entreprises, "acme", 2).Count);
    }

    [Fact]
    public void Un_texte_vide_ne_donne_aucune_suggestion()
    {
        Assert.Empty(CorrespondanceNomsEntreprises.Suggerer(Entreprises, "  ", 10));
    }
}
