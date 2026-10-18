using SuiviCandidatures.Presentation.Vues;

namespace SuiviCandidatures.Presentation;

public partial class CoquilleApplication : Shell
{
    public CoquilleApplication()
    {
        InitializeComponent();

        // Pages de détail : atteintes par navigation depuis les listes, avec le paramètre « identifiant ».
        Routing.RegisterRoute(NomsRoutes.DetailCandidature, typeof(PageDetailCandidature));
        Routing.RegisterRoute(NomsRoutes.DetailEntreprise, typeof(PageDetailEntreprise));
        Routing.RegisterRoute(NomsRoutes.FormulaireCandidature, typeof(PageFormulaireCandidature));
        Routing.RegisterRoute(NomsRoutes.FormulaireEntreprise, typeof(PageFormulaireEntreprise));
    }
}
