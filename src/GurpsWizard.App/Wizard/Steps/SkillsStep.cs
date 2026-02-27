using GurpsWizard.Core.Models;

namespace GurpsWizard.App.Wizard.Steps;

public class SkillsStep : IWizardStep
{
    public string Title => "Perícias";
    public string Description => "Selecione ao menos uma perícia para o personagem.";
    public bool CanProceed(CharacterDraft draft) => draft.Skills.Count > 0;
    public Task OnEnterAsync(CharacterDraft draft) => Task.CompletedTask;
    public Task OnLeaveAsync(CharacterDraft draft) => Task.CompletedTask;
}
