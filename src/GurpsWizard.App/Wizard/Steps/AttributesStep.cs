using GurpsWizard.Core.Models;

namespace GurpsWizard.App.Wizard.Steps;

public class AttributesStep : IWizardStep
{
    public string Title => "Atributos";
    public string Description => "Defina os atributos primários do personagem (ST, DX, IQ, HT).";
    public bool CanProceed(CharacterDraft draft) => true;
    public Task OnEnterAsync(CharacterDraft draft) => Task.CompletedTask;
    public Task OnLeaveAsync(CharacterDraft draft) => Task.CompletedTask;
}
