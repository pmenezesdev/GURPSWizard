using System.Collections.Generic;
using System.IO;
using System.Reactive;
using Avalonia.Media.Imaging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using GurpsWizard.App.Services;

namespace GurpsWizard.App.ViewModels;

public class PdfViewerViewModel : ReactiveObject
{
    private readonly PdfInternalService _pdfService = new();

    [Reactive] public bool IsOpen { get; set; }
    [Reactive] public string? CurrentTitle { get; set; }
    [Reactive] public string? CurrentBookPath { get; set; }
    [Reactive] public int CurrentPageIndex { get; set; }
    [Reactive] public int TotalPages { get; set; }
    [Reactive] public WriteableBitmap? CurrentPageBitmap { get; set; }
    [Reactive] public string? SearchTerm { get; set; }
    [Reactive] public double ZoomLevel { get; set; } = 1.2;

    public ReactiveCommand<Unit, Unit> CloseCommand { get; }
    public ReactiveCommand<Unit, Unit> NextPageCommand { get; }
    public ReactiveCommand<Unit, Unit> PrevPageCommand { get; }
    public ReactiveCommand<Unit, Unit> ZoomInCommand { get; }
    public ReactiveCommand<Unit, Unit> ZoomOutCommand { get; }

    public PdfViewerViewModel()
    {
        CloseCommand = ReactiveCommand.Create(() => { IsOpen = false; });
        NextPageCommand = ReactiveCommand.Create(() => { ChangePage(1); }, 
            this.WhenAnyValue(x => x.CurrentPageIndex, x => x.TotalPages, (curr, total) => curr < total - 1));
        PrevPageCommand = ReactiveCommand.Create(() => { ChangePage(-1); }, 
            this.WhenAnyValue(x => x.CurrentPageIndex, (curr) => curr > 0));
        
        ZoomInCommand = ReactiveCommand.Create(() => { ZoomLevel += 0.2; });
        ZoomOutCommand = ReactiveCommand.Create(() => { if (ZoomLevel > 0.4) ZoomLevel -= 0.2; });

        this.WhenAnyValue(x => x.CurrentPageIndex, x => x.ZoomLevel)
            .Subscribe(_ => RenderCurrentPage());
    }

    public void Open(string pdfPath, int printedPage, string? title = null, string? searchTerm = null)
    {
        string fileName = Path.GetFileName(pdfPath);
        string effectivePath = pdfPath;
        
        // 1. Tentar caminho customizado das configurações
        if (!string.IsNullOrWhiteSpace(SettingsService.Instance.BooksPath))
        {
            var customPath = Path.Combine(SettingsService.Instance.BooksPath, fileName);
            if (File.Exists(customPath)) 
            {
                effectivePath = customPath;
            }
            else
            {
                SettingsService.Log($"[PDF] Não encontrado no caminho customizado: {customPath}");
            }
        }

        // 2. Se não existir no customizado e nem no padrão enviado (data/livros), desistir
        if (!File.Exists(effectivePath))
        {
            SettingsService.Log($"[PDF] Arquivo não encontrado em nenhum lugar: {fileName}");
            return;
        }

        SettingsService.Log($"[PDF] Abrindo: {effectivePath} para o item '{title}'");
        CurrentBookPath = effectivePath;
        CurrentTitle = title;
        SearchTerm = searchTerm?.Split('(')[0].Trim(); 
        
        try
        {
            _pdfService.Load(effectivePath);
            TotalPages = _pdfService.GetPageCount();

            // Aplicar offset das configurações
            int offset = SettingsService.Instance.Offsets.GetValueOrDefault(fileName, 0);
            CurrentPageIndex = printedPage + offset;

            if (CurrentPageIndex < 0) CurrentPageIndex = 0;
            if (CurrentPageIndex >= TotalPages) CurrentPageIndex = TotalPages - 1;

            IsOpen = true;
            RenderCurrentPage();
        }
        catch (Exception ex)
        {
            SettingsService.Log($"[PDF] Erro ao carregar documento: {ex.Message}");
        }
    }

    private void ChangePage(int delta)
    {
        int next = CurrentPageIndex + delta;
        if (next >= 0 && next < TotalPages) CurrentPageIndex = next;
    }

    private void RenderCurrentPage()
    {
        if (!_pdfService.IsDocumentOpen) return;
        CurrentPageBitmap = _pdfService.RenderPage(CurrentPageIndex, ZoomLevel, SearchTerm);
    }
}
