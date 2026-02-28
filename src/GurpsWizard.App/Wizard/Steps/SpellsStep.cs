using GurpsWizard.Core.Models;

namespace GurpsWizard.App.Wizard.Steps;

public class SpellsStep : IWizardStep
{
    public string Title => "Mágicas";
    public string Description => "Selecione as mágicas do personagem. Exige a vantagem Aptidão Mágica. Opcional — você pode prosseguir sem adicionar nenhuma.";
    public bool CanProceed(CharacterDraft draft) => true;
    public Task OnEnterAsync(CharacterDraft draft) => Task.CompletedTask;
    public Task OnLeaveAsync(CharacterDraft draft) => Task.CompletedTask;
}
