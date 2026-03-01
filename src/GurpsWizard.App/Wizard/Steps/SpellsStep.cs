using GurpsWizard.Core.Models;

namespace GurpsWizard.App.Wizard.Steps;

public class SpellsStep : IWizardStep
{
    public string Title => "Mágicas";
    public string Description => "Selecione as mágicas do personagem. Exige a vantagem Aptidão Mágica. Opcional — você pode prosseguir sem adicionar nenhuma.";
    public string? BackgroundImage => "/Assets/Images/Steps/bg_spells.jpg.png";
    public bool CanProceed(CharacterDraft draft) => true;
    public Task OnEnterAsync(CharacterDraft draft) => Task.CompletedTask;
    public Task OnLeaveAsync(CharacterDraft draft) => Task.CompletedTask;
}
