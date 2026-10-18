namespace SuiviCandidatures.Presentation.Comportements;

/// <summary>Empêche la saisie de caractères non autorisés dans une zone de texte (frappe comme collage).</summary>
public sealed class ComportementSaisieFiltree : Behavior<Entry>
{
    public static readonly BindableProperty CaracteresAutorisesProperty = BindableProperty.Create(
        nameof(CaracteresAutorises),
        typeof(string),
        typeof(ComportementSaisieFiltree),
        string.Empty);

    /// <summary>Liste exhaustive des caractères acceptés, par exemple « 0123456789 ».</summary>
    public string CaracteresAutorises
    {
        get => (string)GetValue(CaracteresAutorisesProperty);
        set => SetValue(CaracteresAutorisesProperty, value);
    }

    protected override void OnAttachedTo(Entry zoneDeTexte)
    {
        base.OnAttachedTo(zoneDeTexte);
        zoneDeTexte.TextChanged += QuandTexteModifie;
    }

    protected override void OnDetachingFrom(Entry zoneDeTexte)
    {
        zoneDeTexte.TextChanged -= QuandTexteModifie;
        base.OnDetachingFrom(zoneDeTexte);
    }

    private void QuandTexteModifie(object? expediteur, TextChangedEventArgs arguments)
    {
        if (expediteur is not Entry zoneDeTexte || string.IsNullOrEmpty(arguments.NewTextValue))
        {
            return;
        }

        string filtre = new(arguments.NewTextValue.Where(caractere => CaracteresAutorises.Contains(caractere)).ToArray());
        if (filtre != arguments.NewTextValue)
        {
            zoneDeTexte.Text = filtre;
            zoneDeTexte.CursorPosition = filtre.Length;
        }
    }
}
