using GurpsWizard.Core.Models;

namespace GurpsWizard.App.Wizard.Steps;

public class EquipmentStep : IWizardStep
{
    public string Title => "Equipamento";
    public string Description => "Selecione o equipamento do personagem (seleção opcional).";
    public string? BackgroundImage => "/Assets/Images/Steps/bg_equipment.png";
    public bool CanProceed(CharacterDraft draft) => true;
    public Task OnEnterAsync(CharacterDraft draft) => Task.CompletedTask;
    public Task OnLeaveAsync(CharacterDraft draft) => Task.CompletedTask;
}
