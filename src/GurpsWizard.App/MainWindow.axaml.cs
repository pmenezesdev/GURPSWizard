using Avalonia.ReactiveUI;
using GurpsWizard.App.ViewModels;

namespace GurpsWizard.App;

public partial class MainWindow : ReactiveWindow<MainViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
    }
}
