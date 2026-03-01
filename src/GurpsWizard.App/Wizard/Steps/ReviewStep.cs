using GurpsWizard.Core.Models;

namespace GurpsWizard.App.Wizard.Steps;

public class ReviewStep : IWizardStep
{
    public string Title => "Revisão";
    public string Description => "Revise a ficha completa e salve o personagem.";
    public string? BackgroundImage => "/Assets/Images/Steps/bg_review.png";
    public bool CanProceed(CharacterDraft draft) => true;
    public Task OnEnterAsync(CharacterDraft draft) => Task.CompletedTask;
    public Task OnLeaveAsync(CharacterDraft draft) => Task.CompletedTask;
}
