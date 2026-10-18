using CommunityToolkit.Mvvm.ComponentModel;

namespace SuiviCandidatures.Presentation.ViewModels;

public abstract class ViewModelDeBase : ObservableObject
{
    public abstract string Titre { get; }
}
