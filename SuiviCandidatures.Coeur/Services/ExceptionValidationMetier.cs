namespace SuiviCandidatures.Coeur.Services;

/// <summary>Erreur de règle métier dont le message peut être affiché tel quel à l'utilisateur.</summary>
public sealed class ExceptionValidationMetier(string message) : Exception(message);
