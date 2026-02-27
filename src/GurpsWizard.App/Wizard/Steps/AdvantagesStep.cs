using GurpsWizard.Core.Models;

namespace GurpsWizard.App.Wizard.Steps;

public class AdvantagesStep : IWizardStep
{
    public string Title => "Vantagens";
    public string Description => "Escolha as vantagens do personagem (seleção opcional).";
    public bool CanProceed(CharacterDraft draft) => true;
    public Task OnEnterAsync(CharacterDraft draft) => Task.CompletedTask;
    public Task OnLeaveAsync(CharacterDraft draft) => Task.CompletedTask;
}
