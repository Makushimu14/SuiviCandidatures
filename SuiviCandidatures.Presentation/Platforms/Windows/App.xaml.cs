using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace SuiviCandidatures.Presentation.WinUI;

public partial class ApplicationWindows : MauiWinUIApplication
{
    public ApplicationWindows()
    {
        InitializeComponent();
    }

    protected override MauiApp CreateMauiApp() => ProgrammeApplication.CreerApplicationMaui();
}
