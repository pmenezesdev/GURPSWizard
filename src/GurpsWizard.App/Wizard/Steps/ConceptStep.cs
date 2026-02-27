using GurpsWizard.Core.Models;

namespace GurpsWizard.App.Wizard.Steps;

public class ConceptStep : IWizardStep
{
    public string Title => "Conceito";
    public string Description => "Defina o nome e a descrição do personagem e o total de pontos da campanha.";
    public bool CanProceed(CharacterDraft draft) => !string.IsNullOrWhiteSpace(draft.Name);
    public Task OnEnterAsync(CharacterDraft draft) => Task.CompletedTask;
    public Task OnLeaveAsync(CharacterDraft draft) => Task.CompletedTask;
}
