namespace SuiviCandidatures.Coeur.Transferts;

public enum RaisonRelanceInactive
{
    Aucune,
    StatutTerminal,
    DesactiveeParUtilisateur
}

/// <summary>JoursRestants est négatif lorsque la relance est en retard.</summary>
public sealed record EtatRelance(bool EstActive, DateTime? DateRelance, int? JoursRestants, RaisonRelanceInactive Raison);
