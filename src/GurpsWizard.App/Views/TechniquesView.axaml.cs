using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using GurpsWizard.App.ViewModels.Steps;

namespace GurpsWizard.App.Views;

public partial class TechniquesView : ReactiveUserControl<TechniquesViewModel>
{
    public TechniquesView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
