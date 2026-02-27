using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using GurpsWizard.App.Services;
using Avalonia.Controls.ApplicationLifetimes;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GurpsWizard.App.ViewModels;

public class SettingsViewModel : ReactiveObject
{
    private readonly MainViewModel _main;

    [Reactive] public string BooksPath { get; set; }
    [Reactive] public bool EnableLogging { get; set; }
    [Reactive] public int BasicSetOffset { get; set; }
    [Reactive] public int MagiaOffset { get; set; }
    [Reactive] public int DeluxOffset { get; set; }

    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> SaveCommand { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> CancelCommand { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> BrowseFolderCommand { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> OpenLogsFolderCommand { get; }

    public SettingsViewModel(MainViewModel main)
    {
        _main = main;

        var s = SettingsService.Instance;
        BooksPath = s.BooksPath;
        EnableLogging = s.EnableLogging;
        BasicSetOffset = s.Offsets.GetValueOrDefault("Basic Set.pdf", 0);
        MagiaOffset = s.Offsets.GetValueOrDefault("GURPS Magia.pdf", 0);
        DeluxOffset = s.Offsets.GetValueOrDefault("Delux.pdf", 0);

        BrowseFolderCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var folders = await desktop.MainWindow!.StorageProvider.OpenFolderPickerAsync(new Avalonia.Platform.Storage.FolderPickerOpenOptions
                {
                    Title = "Selecionar pasta de livros PDF",
                    AllowMultiple = false
                });

                if (folders.Count > 0)
                {
                    BooksPath = folders[0].Path.LocalPath;
                }
            }
        });

        OpenLogsFolderCommand = ReactiveCommand.Create(() =>
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GurpsWizard");
            if (Directory.Exists(path))
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) Process.Start("explorer.exe", path);
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) Process.Start("xdg-open", path);
            }
        });

        SaveCommand = ReactiveCommand.Create(() =>
        {
            s.BooksPath = BooksPath;
            s.EnableLogging = EnableLogging;
            s.Offsets["Basic Set.pdf"] = BasicSetOffset;
            s.Offsets["GURPS Magia.pdf"] = MagiaOffset;
            s.Offsets["Delux.pdf"] = DeluxOffset;
            
            if (EnableLogging) SettingsService.Log("Logs habilitados pelo usuário.");
            
            SettingsService.Save();
            _main.ShowHome();
        });

        CancelCommand = ReactiveCommand.Create(_main.ShowHome);
    }
}
