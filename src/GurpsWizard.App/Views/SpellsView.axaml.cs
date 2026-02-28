using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using GurpsWizard.App.ViewModels.Steps;

namespace GurpsWizard.App.Views;

public partial class SpellsView : ReactiveUserControl<SpellsViewModel>
{
    public SpellsView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
