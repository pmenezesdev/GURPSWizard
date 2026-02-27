using Avalonia.ReactiveUI;
using GurpsWizard.App.ViewModels.Steps;

namespace GurpsWizard.App.Views;

public partial class ReviewView : ReactiveUserControl<ReviewViewModel>
{
    public ReviewView()
    {
        InitializeComponent();
    }
}
