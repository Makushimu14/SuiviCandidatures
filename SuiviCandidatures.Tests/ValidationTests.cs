using SuiviCandidatures.Coeur.Services;
using SuiviCandidatures.Coeur.Transferts;

namespace SuiviCandidatures.Tests;

public class ValidationTests
{
    private static DonneesAdresse Adresse(string? codePostal = null, string? mail = null, string? telephone = null) =>
        new("1 rue de la Paix", codePostal, "Paris", mail, telephone);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("69000")]
    public void Un_code_postal_vide_ou_a_cinq_chiffres_est_accepte(string? codePostal)
    {
        ValidationAdresse.Valider(Adresse(codePostal: codePostal));
    }

    [Theory]
    [InlineData("6900")]
    [InlineData("690000")]
    [InlineData("69A00")]
    public void Un_code_postal_invalide_est_refuse(string codePostal)
    {
        Assert.Throws<ExceptionValidationMetier>(() => ValidationAdresse.Valider(Adresse(codePostal: codePostal)));
    }

    [Theory]
    [InlineData("04 78 12 34 56")]
    [InlineData("+33 4 78 12 34 56")]
    [InlineData("(04) 78.12.34.56")]
    [InlineData("0478123456")]
    public void Un_telephone_valide_est_accepte(string telephone)
    {
        ValidationAdresse.ValiderTelephone(telephone);
    }

    [Theory]
    [InlineData("abcdefghij")]
    [InlineData("123")]
    [InlineData("1234567890123456")]
    [InlineData("04 78 12 34 5x")]
    public void Un_telephone_invalide_est_refuse(string telephone)
    {
        Assert.Throws<ExceptionValidationMetier>(() => ValidationAdresse.ValiderTelephone(telephone));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("contact@exemple.fr")]
    public void Un_mail_vide_ou_valide_est_accepte(string? mail)
    {
        ValidationAdresse.ValiderMail(mail);
    }

    [Theory]
    [InlineData("pas un mail")]
    [InlineData("sans-arobase.fr")]
    [InlineData("Nom <contact@exemple.fr>")]
    public void Un_mail_invalide_est_refuse(string mail)
    {
        Assert.Throws<ExceptionValidationMetier>(() => ValidationAdresse.ValiderMail(mail));
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("   ", null)]
    [InlineData("exemple.fr", "https://exemple.fr")]
    [InlineData("  http://exemple.fr/offre  ", "http://exemple.fr/offre")]
    public void La_normalisation_d_un_lien_ajoute_https_quand_le_schema_manque(string? saisie, string? attendu)
    {
        Assert.Equal(attendu, LiensWeb.Normaliser(saisie));
    }

    [Theory]
    [InlineData("https://exemple.fr/offre?id=1", true)]
    [InlineData("http://exemple.fr", true)]
    [InlineData("ftp://exemple.fr", false)]
    [InlineData("file:///c:/secret.txt", false)]
    [InlineData("javascript:alert(1)", false)]
    [InlineData("pas un lien", false)]
    [InlineData(null, false)]
    public void Seuls_les_liens_http_et_https_sont_valides(string? lien, bool attendu)
    {
        Assert.Equal(attendu, LiensWeb.EstValide(lien));
    }

    [Fact]
    public void Un_lien_dangereux_reste_invalide_apres_normalisation()
    {
        Assert.False(LiensWeb.EstValide(LiensWeb.Normaliser("javascript:alert(1)")));
    }
}
