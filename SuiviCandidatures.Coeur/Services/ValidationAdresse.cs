using System.Net.Mail;
using SuiviCandidatures.Coeur.Transferts;

namespace SuiviCandidatures.Coeur.Services;

/// <summary>Règles de format des coordonnées (adresse, téléphone). Appliquées par les services, indépendamment de l'interface.</summary>
public static class ValidationAdresse
{
    public const int LongueurCodePostal = 5;
    public const int NombreMinimumChiffresTelephone = 9;
    public const int NombreMaximumChiffresTelephone = 15;
    public const string CaracteresTelephoneAutorises = " +-.()";

    public static void Valider(DonneesAdresse adresse)
    {
        string codePostal = adresse.CodePostal?.Trim() ?? string.Empty;
        if (codePostal.Length > 0 && !(codePostal.Length == LongueurCodePostal && codePostal.All(char.IsAsciiDigit)))
        {
            throw new ExceptionValidationMetier($"Le code postal doit comporter {LongueurCodePostal} chiffres.");
        }

        ValiderMail(adresse.Mail);
        ValiderTelephone(adresse.Telephone);
    }

    /// <summary>Une valeur vide est acceptée.</summary>
    public static void ValiderMail(string? valeur)
    {
        string mail = valeur?.Trim() ?? string.Empty;
        if (mail.Length > 0 && !(MailAddress.TryCreate(mail, out MailAddress? adresseMail) && adresseMail.Address == mail))
        {
            throw new ExceptionValidationMetier("L'adresse mail n'est pas valide.");
        }
    }

    /// <summary>Une valeur vide est acceptée.</summary>
    public static void ValiderTelephone(string? valeur)
    {
        string telephone = valeur?.Trim() ?? string.Empty;
        if (telephone.Length == 0)
        {
            return;
        }

        if (!telephone.All(c => char.IsAsciiDigit(c) || CaracteresTelephoneAutorises.Contains(c)))
        {
            throw new ExceptionValidationMetier(
                "Le numéro de téléphone ne peut contenir que des chiffres, des espaces et les signes + - . ( ).");
        }

        int nombreChiffres = telephone.Count(char.IsAsciiDigit);
        if (nombreChiffres < NombreMinimumChiffresTelephone || nombreChiffres > NombreMaximumChiffresTelephone)
        {
            throw new ExceptionValidationMetier(
                $"Le numéro de téléphone doit comporter entre {NombreMinimumChiffresTelephone} et {NombreMaximumChiffresTelephone} chiffres.");
        }
    }
}
