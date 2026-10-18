namespace SuiviCandidatures.Presentation.Services;

public static class FormatageRelance
{
    /// <summary>Décrit une échéance de relance en texte : « en retard de 3 jour(s) », « aujourd'hui », « demain »...</summary>
    public static string DecrireEcheance(int joursRestants) => joursRestants switch
    {
        < 0 => $"en retard de {-joursRestants} jour(s)",
        0 => "aujourd'hui",
        1 => "demain",
        _ => $"dans {joursRestants} jours"
    };
}
