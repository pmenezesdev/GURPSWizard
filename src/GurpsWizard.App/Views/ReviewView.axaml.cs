using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using GurpsWizard.App.ViewModels.Steps;
using ReactiveUI;

namespace GurpsWizard.App.Views;

public partial class ReviewView : ReactiveUserControl<ReviewViewModel>
{
    public ReviewView()
    {
        InitializeComponent();
        this.WhenActivated(d =>
        {
            if (ViewModel is null) return;

            d(ViewModel.SaveFileInteraction.RegisterHandler(async ctx =>
            {
                var provider = Avalonia.Controls.TopLevel.GetTopLevel(this)?.StorageProvider;
                if (provider is null) { ctx.SetOutput(null); return; }

                var file = await provider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title             = "Exportar personagem",
                    SuggestedFileName = ctx.Input + ".json",
                    FileTypeChoices   = [new FilePickerFileType("JSON") { Patterns = ["*.json"] }],
                });
                ctx.SetOutput(file?.TryGetLocalPath());
            }));

            d(ViewModel.SaveGcsInteraction.RegisterHandler(async ctx =>
            {
                var provider = Avalonia.Controls.TopLevel.GetTopLevel(this)?.StorageProvider;
                if (provider is null) { ctx.SetOutput(null); return; }

                var file = await provider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title             = "Exportar para GURPS Character Sheet",
                    SuggestedFileName = ctx.Input + ".gcs",
                    FileTypeChoices   = [new FilePickerFileType("GURPS Character Sheet") { Patterns = ["*.gcs"] }],
                });
                ctx.SetOutput(file?.TryGetLocalPath());
            }));
        });
    }
}
