using GurpsWizard.Core.Models;

namespace GurpsWizard.App.Wizard.Steps;

public class TechniquesStep : IWizardStep
{
    public string Title => "Técnicas";
    public string Description => "Técnicas são especializações de perícias. Opcional — você pode prosseguir sem adicionar nenhuma.";
    public bool CanProceed(CharacterDraft draft) => true;
    public Task OnEnterAsync(CharacterDraft draft) => Task.CompletedTask;
    public Task OnLeaveAsync(CharacterDraft draft) => Task.CompletedTask;
}
