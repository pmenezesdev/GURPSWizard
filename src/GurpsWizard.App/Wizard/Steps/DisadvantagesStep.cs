using GurpsWizard.Core.Models;

namespace GurpsWizard.App.Wizard.Steps;

/// <summary>
/// Válido enquanto o total absoluto de pontos de desvantagens não ultrapassar 75 pts
/// (limite padrão GURPS 4e; equivale à maior lista habitual de campanha).
/// </summary>
public class DisadvantagesStep : IWizardStep
{
    private const int DefaultDisadvLimit = 75;

    public string Title => "Desvantagens";
    public string Description => $"Escolha as desvantagens do personagem. Limite: {DefaultDisadvLimit} pontos.";

    public bool CanProceed(CharacterDraft draft)
    {
        var total = draft.Disadvantages.Sum(t => Math.Abs(t.Cost));
        return total <= DefaultDisadvLimit;
    }

    public Task OnEnterAsync(CharacterDraft draft) => Task.CompletedTask;
    public Task OnLeaveAsync(CharacterDraft draft) => Task.CompletedTask;
}
