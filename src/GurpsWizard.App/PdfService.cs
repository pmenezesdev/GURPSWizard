using GurpsWizard.App.ViewModels;
using System.Text.RegularExpressions;

namespace GurpsWizard.App;

public static class PdfService
{
    private static string GetPdfDirectory()
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null)
        {
            var candidate = Path.Combine(dir, "data", "livros");
            if (Directory.Exists(candidate)) return candidate;
            dir = Path.GetDirectoryName(dir);
        }
        
        var appDataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GurpsWizard", "livros");
        Directory.CreateDirectory(appDataDir);
        return appDataDir;
    }

    public static void OpenReference(string? reference, string? itemName = null)
    {
        if (string.IsNullOrWhiteSpace(reference)) return;

        // Regex para pegar o código do livro e o primeiro número que aparecer logo após
        // Ex: "MB 35, P40" -> Group 1: MB, Group 2: 35
        var match = Regex.Match(reference, @"([A-Za-z]+)\s*(\d+)");
        
        if (!match.Success) return;

        string bookCode = match.Groups[1].Value.ToUpper();
        if (!int.TryParse(match.Groups[2].Value, out int page)) return;

        string pdfFileName = bookCode switch
        {
            "MB" or "B" or "MBP" or "MBC" or "CH" or "CA" or "MBAM" => "Basic Set.pdf",
            "M" => "GURPS Magia.pdf",
            _ => bookCode + ".pdf"
        };

        var pdfDir = GetPdfDirectory();
        var pdfPath = Path.Combine(pdfDir, pdfFileName);

        if ((pdfFileName == "Basic Set.pdf") && !File.Exists(pdfPath))
        {
            var deluxCandidate = Path.Combine(pdfDir, "Delux.pdf");
            if (File.Exists(deluxCandidate)) pdfPath = deluxCandidate;
        }

        if (File.Exists(pdfPath))
        {
            // Passamos a página impressa. O ViewModel aplicará o offset configurado.
            MainViewModel.Instance?.PdfViewer.Open(pdfPath, page, itemName, itemName);
        }
        else
        {
            Console.WriteLine($"[PDF] Livro não encontrado: {pdfFileName}");
        }
    }
}
