using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace GurpsWizard.App.ViewModels;

public class LoadingViewModel : ReactiveObject
{
    [Reactive] public string StatusMessage { get; set; } = "Iniciando…";
    [Reactive] public bool HasError { get; set; }

    /// <summary>Expõe um IProgress que atualiza StatusMessage na UI thread.</summary>
    public IProgress<string> Progress { get; }

    public LoadingViewModel()
    {
        Progress = new Progress<string>(msg =>
        {
            StatusMessage = msg;
            HasError      = msg.StartsWith("Erro", StringComparison.OrdinalIgnoreCase);
        });
    }
}
