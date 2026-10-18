namespace SuiviCandidatures.Coeur.Transferts;

/// <summary>Relance prévue pour une candidature. JoursRestants est négatif lorsque la relance est en retard.</summary>
public sealed record ElementRelance(
    int IdentifiantCandidature,
    string TitreOffre,
    string NomEntreprise,
    DateTime DateRelance,
    int JoursRestants)
{
    public bool EstDue => JoursRestants <= 0;
}
