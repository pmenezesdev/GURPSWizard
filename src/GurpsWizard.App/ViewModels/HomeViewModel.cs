using ReactiveUI;

namespace GurpsWizard.App.ViewModels;

public class HomeViewModel : ReactiveObject
{
    private readonly MainViewModel _main;

    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> NewCharacterCommand { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> ListCharactersCommand { get; }

    public HomeViewModel(MainViewModel main)
    {
        _main = main;

        NewCharacterCommand  = ReactiveCommand.Create(main.StartNewCharacter);
        ListCharactersCommand = ReactiveCommand.Create(main.ShowCharacterList);
    }
}
