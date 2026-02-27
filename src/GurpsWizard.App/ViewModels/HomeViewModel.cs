using ReactiveUI;

namespace GurpsWizard.App.ViewModels;

public class HomeViewModel : ReactiveObject
{
    private readonly MainViewModel _main;

    public ThemeService Theme => ThemeService.Instance;

    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> NewCharacterCommand { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> ListCharactersCommand { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> ToggleThemeCommand { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> ShowSettingsCommand { get; }

    public HomeViewModel(MainViewModel main)
    {
        _main = main;

        NewCharacterCommand   = ReactiveCommand.Create(main.StartNewCharacter);
        ListCharactersCommand = ReactiveCommand.Create(main.ShowCharacterList);
        ToggleThemeCommand    = ReactiveCommand.Create(ThemeService.Instance.Toggle);
        ShowSettingsCommand   = ReactiveCommand.Create(main.ShowSettings);
    }
}
