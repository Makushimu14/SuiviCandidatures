using Foundation;

namespace SuiviCandidatures.Presentation;

[Register("DelegueApplication")]
public class DelegueApplication : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => ProgrammeApplication.CreerApplicationMaui();
}
