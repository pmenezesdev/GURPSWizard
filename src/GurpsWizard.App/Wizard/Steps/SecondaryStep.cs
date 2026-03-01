using GurpsWizard.Core.Models;

namespace GurpsWizard.App.Wizard.Steps;

public class SecondaryStep : IWizardStep
{
    public string Title => "Secundários";
    public string Description => "Ajuste os atributos secundários (PV, PM, Vontade, Per, Vel, Desl).";
    public string? BackgroundImage => "/Assets/Images/Steps/bg_secondary.png";
    public bool CanProceed(CharacterDraft draft) => true;
    public Task OnEnterAsync(CharacterDraft draft) => Task.CompletedTask;
    public Task OnLeaveAsync(CharacterDraft draft) => Task.CompletedTask;
}
