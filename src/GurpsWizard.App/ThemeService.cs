using Avalonia;
using Avalonia.Styling;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace GurpsWizard.App;

public class ThemeService : ReactiveObject
{
    public static ThemeService Instance { get; } = new();

    private ThemeService() { }

    [Reactive] public bool IsDark { get; private set; } = true;

    public void Toggle()
    {
        IsDark = !IsDark;
        Application.Current!.RequestedThemeVariant = IsDark ? ThemeVariant.Dark : ThemeVariant.Light;
    }
}
