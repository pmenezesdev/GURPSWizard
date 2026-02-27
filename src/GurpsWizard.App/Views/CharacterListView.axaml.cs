using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using GurpsWizard.App.ViewModels;
using ReactiveUI;

namespace GurpsWizard.App.Views;

public partial class CharacterListView : ReactiveUserControl<CharacterListViewModel>
{
    public CharacterListView()
    {
        InitializeComponent();
        this.WhenActivated(d =>
        {
            if (ViewModel is null) return;
            d(ViewModel.OpenFileInteraction.RegisterHandler(async ctx =>
            {
                var provider = TopLevel.GetTopLevel(this)?.StorageProvider;
                if (provider is null) { ctx.SetOutput(null); return; }

                var files = await provider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title          = "Importar personagem",
                    AllowMultiple  = false,
                    FileTypeFilter = [new FilePickerFileType("JSON") { Patterns = ["*.json"] }],
                });
                ctx.SetOutput(files.Count > 0 ? files[0].TryGetLocalPath() : null);
            }));
        });
    }
}
