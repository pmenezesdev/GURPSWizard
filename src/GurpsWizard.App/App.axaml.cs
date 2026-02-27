using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GurpsWizard.App.ViewModels;
using GurpsWizard.App.Wizard;
using GurpsWizard.App.Wizard.Steps;
using GurpsWizard.Data;
using GurpsWizard.Data.Gcs;
using GurpsWizard.Data.Repositories;
using GurpsWizard.App.Services;
using Microsoft.EntityFrameworkCore;

namespace GurpsWizard.App;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        SettingsService.Load();
        if (SettingsService.Instance.EnableLogging) SettingsService.ClearLog();
        SettingsService.Log("Aplicação inicializada.");
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // ── 1. Mostrar tela de loading imediatamente ──────────────────────
            var loadingVm     = new LoadingViewModel();
            var loadingWindow = new LoadingWindow { DataContext = loadingVm };
            desktop.MainWindow = loadingWindow;
            loadingWindow.Show();

            // ── 2. Inicializar serviços em background ─────────────────────────
            _ = Task.Run(async () =>
            {
                ILibraryRepository libraryRepo;
                ICharacterRepository characterRepo;

                try
                {
                    SettingsService.Log("Iniciando inicialização de serviços...");
                    (libraryRepo, characterRepo) =
                        await InitializeServicesAsync(loadingVm.Progress);
                    SettingsService.Log("Serviços inicializados com sucesso.");
                    
                    // Pequeno delay para o usuário ler a última mensagem de status
                    await Task.Delay(500);
                }
                catch (Exception ex)
                {
                    SettingsService.Log("=== ERRO FATAL NA INICIALIZAÇÃO ===");
                    SettingsService.Log(ex.ToString());

                    var msg = UnwrapMessages(ex);
                    await Dispatcher.UIThread.InvokeAsync(() => {
                        loadingVm.StatusMessage = $"ERRO FATAL: {msg}";
                        loadingVm.HasError = true;
                    });
                    return;
                }

                // ── 3. Abrir MainWindow na UI thread e fechar loading ─────────
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    try
                    {
                        SettingsService.Log("Criando MainViewModel e MainWindow...");
                        var viewModel  = new MainViewModel(libraryRepo, characterRepo);
                        var mainWindow = new MainWindow { DataContext = viewModel };

                        SettingsService.Log("Trocando MainWindow e fechando loading...");
                        desktop.MainWindow = mainWindow;
                        mainWindow.Show();
                        loadingWindow.Close();
                        SettingsService.Log("Transição concluída. App pronto.");
                    }
                    catch (Exception ex)
                    {
                        SettingsService.Log("=== ERRO AO ABRIR MAINWINDOW ===");
                        SettingsService.Log(ex.ToString());
                    }
                });
            });
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static async Task<(ILibraryRepository library, ICharacterRepository character)>
        InitializeServicesAsync(IProgress<string> progress)
    {
        progress.Report("Criando diretório de dados…");

        var appDataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GurpsWizard");
        Directory.CreateDirectory(appDataDir);
        var dbPath = Path.Combine(appDataDir, "gurpswizard.db");

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;

        var db          = new AppDbContext(options);
        var loader      = new GcsLoader();
        var initializer = new DatabaseInitializer(db, loader);

        var gcsPath = FindGcsPath();
        if (gcsPath is not null)
        {
            await initializer.InitializeAsync(gcsPath, progress);
        }
        else
        {
            progress.Report("Dados GCS não encontrados — aplicando migrações…");
            await db.Database.MigrateAsync();
            progress.Report("Pronto (sem dados de biblioteca).");
        }

        return (new LibraryRepository(db), new CharacterRepository(db));
    }

    private static string? FindGcsPath()
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null)
        {
            var candidate = Path.Combine(dir, "data", "gcs-ptbr");
            if (Directory.Exists(candidate))
                return candidate;
            dir = Path.GetDirectoryName(dir);
        }
        return null;
    }

    /// <summary>Concatena mensagens de toda a cadeia de exceções.</summary>
    private static string UnwrapMessages(Exception ex)
    {
        var parts = new List<string>();
        var current = ex;
        while (current is not null)
        {
            if (!string.IsNullOrWhiteSpace(current.Message))
                parts.Add(current.Message);
            current = current.InnerException;
        }
        return string.Join(" → ", parts);
    }
}
