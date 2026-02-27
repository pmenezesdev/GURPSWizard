using GurpsWizard.Core.Models;

namespace GurpsWizard.App.Wizard;

public interface IWizardStep
{
    string Title { get; }
    string Description { get; }
    bool CanProceed(CharacterDraft draft);
    Task OnEnterAsync(CharacterDraft draft);
    Task OnLeaveAsync(CharacterDraft draft);
}
