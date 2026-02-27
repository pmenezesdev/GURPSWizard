using System.Text.Json;
using System.Text;

namespace GurpsWizard.App.Services;

public class AppSettings
{
    public string BooksPath { get; set; } = "";
    public bool EnableLogging { get; set; } = false;
    public Dictionary<string, int> Offsets { get; set; } = new()
    {
        { "Basic Set.pdf", 0 },
        { "Delux.pdf", 0 },
        { "GURPS Magia.pdf", 0 }
    };
}

public static class SettingsService
{
    private static readonly string SettingsDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "GurpsWizard");
    
    private static readonly string SettingsPath = Path.Combine(SettingsDir, "settings.json");
    private static readonly string LogPath = Path.Combine(SettingsDir, "session_log.txt");

    private static AppSettings? _instance;
    private static StringBuilder? _sessionLog;

    public static AppSettings Instance
    {
        get
        {
            if (_instance == null) Load();
            return _instance!;
        }
    }

    public static void Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                _instance = JsonSerializer.Deserialize<AppSettings>(json);
            }
        }
        catch (Exception ex)
        {
            Log($"Erro ao carregar settings: {ex.Message}");
        }

        _instance ??= new AppSettings();
        
        foreach (var key in new[] { "Basic Set.pdf", "Delux.pdf", "GURPS Magia.pdf" })
        {
            if (!_instance.Offsets.ContainsKey(key))
                _instance.Offsets[key] = 0;
        }
    }

    public static void Save()
    {
        try
        {
            Directory.CreateDirectory(SettingsDir);
            var json = JsonSerializer.Serialize(_instance, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsPath, json);
            Log("Configurações salvas.");
        }
        catch (Exception ex)
        {
            Log($"Erro crítico ao salvar settings: {ex.Message}");
        }
    }

    public static void Log(string message)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var logLine = $"[{timestamp}] {message}";
        
        // Sempre logar no console
        Console.WriteLine(logLine);

        // Se habilitado, logar em memória e arquivo
        if (Instance.EnableLogging)
        {
            try
            {
                _sessionLog ??= new StringBuilder();
                _sessionLog.AppendLine(logLine);
                
                // Gravação imediata para não perder dados em crash
                File.AppendAllText(LogPath, logLine + Environment.NewLine);
            }
            catch { }
        }
    }

    public static void ClearLog()
    {
        try { if (File.Exists(LogPath)) File.Delete(LogPath); } catch { }
        _sessionLog = new StringBuilder();
        Log("Log de sessão iniciado.");
    }
}
