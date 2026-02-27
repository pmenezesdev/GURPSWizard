using GurpsWizard.Core.Models;

namespace GurpsWizard.App.Wizard;

/// <summary>
/// Gerencia a lista ordenada de etapas do wizard, o índice atual
/// e a navegação (avançar / voltar).
/// </summary>
public class WizardEngine
{
    private readonly IReadOnlyList<IWizardStep> _steps;
    private int _currentIndex;

    public WizardEngine(IReadOnlyList<IWizardStep> steps)
    {
        if (steps.Count == 0)
            throw new ArgumentException("A lista de steps não pode ser vazia.", nameof(steps));
        _steps = steps;
    }

    public IWizardStep CurrentStep => _steps[_currentIndex];
    public int CurrentIndex => _currentIndex;
    public int TotalSteps => _steps.Count;
    public bool CanGoBack => _currentIndex > 0;
    public bool IsLastStep => _currentIndex == _steps.Count - 1;
    public IReadOnlyList<IWizardStep> Steps => _steps;

    /// <summary>
    /// Tenta avançar para a próxima etapa. Retorna false se a etapa atual
    /// não permite avançar ou se já está na última etapa.
    /// </summary>
    public async Task<bool> TryAdvanceAsync(CharacterDraft draft)
    {
        if (!CurrentStep.CanProceed(draft)) return false;
        await CurrentStep.OnLeaveAsync(draft);
        if (_currentIndex < _steps.Count - 1)
        {
            _currentIndex++;
            await CurrentStep.OnEnterAsync(draft);
        }
        return true;
    }

    /// <summary>
    /// Volta para a etapa anterior. Não faz nada se já estiver na primeira etapa.
    /// </summary>
    public async Task GoBackAsync(CharacterDraft draft)
    {
        if (_currentIndex > 0)
        {
            await CurrentStep.OnLeaveAsync(draft);
            _currentIndex--;
            await CurrentStep.OnEnterAsync(draft);
        }
    }

    /// <summary>
    /// Pula diretamente para uma etapa específica.
    /// </summary>
    public void JumpTo(int index)
    {
        if (index >= 0 && index < _steps.Count)
        {
            _currentIndex = index;
        }
    }
}
