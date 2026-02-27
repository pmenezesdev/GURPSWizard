using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using PDFiumCore;
using SkiaSharp;
using static PDFiumCore.fpdfview;
using static PDFiumCore.fpdf_text;

namespace GurpsWizard.App;

public sealed class PdfInternalService : IDisposable
{
    private FpdfDocumentT? _document;
    private string? _currentPath;
    private readonly object _lock = new();

    public bool IsDocumentOpen => _document != null;

    public void Load(string filePath)
    {
        lock (_lock)
        {
            if (_currentPath == filePath && _document != null) return;
            Close();
            _document = FPDF_LoadDocument(filePath, null);
            _currentPath = filePath;
            if (_document == null) throw new Exception($"Falha ao carregar PDF: {filePath}");
        }
    }

    public int GetPageCount()
    {
        lock (_lock)
        {
            return _document != null ? FPDF_GetPageCount(_document) : 0;
        }
    }

    public WriteableBitmap? RenderPage(int pageIndex, double zoom, string? searchTerm = null)
    {
        lock (_lock)
        {
            if (_document == null) return null;
            var page = FPDF_LoadPage(_document, pageIndex);
            if (page == null) return null;

            try
            {
                double width = FPDF_GetPageWidth(page);
                double height = fpdfview.FPDF_GetPageHeight(page);
                
                int renderWidth = (int)(width * zoom);
                int renderHeight = (int)(height * zoom);

                var bitmap = new WriteableBitmap(
                    new PixelSize(renderWidth, renderHeight),
                    new Vector(96, 96),
                    PixelFormat.Bgra8888,
                    AlphaFormat.Premul);

                using (var lockedBitmap = bitmap.Lock())
                {
                    var info = new SKImageInfo(renderWidth, renderHeight, SKColorType.Bgra8888, SKAlphaType.Premul);
                    using (var surface = SKSurface.Create(info, lockedBitmap.Address, lockedBitmap.RowBytes))
                    {
                        var canvas = surface.Canvas;
                        canvas.Clear(SKColors.White);

                        // 1. Renderizar o conteúdo original do PDF
                        var fpdfBitmap = FPDFBitmapCreateEx(renderWidth, renderHeight, (int)FPDFBitmapFormat.BGRA, lockedBitmap.Address, lockedBitmap.RowBytes);
                        try {
                            FPDF_RenderPageBitmap(fpdfBitmap, page, 0, 0, renderWidth, renderHeight, 0, (int)RenderFlags.RenderAnnotations);
                        } finally {
                            FPDFBitmapDestroy(fpdfBitmap);
                        }

                        // 2. Se houver termo de busca, encontrar e grifar
                        if (!string.IsNullOrWhiteSpace(searchTerm))
                        {
                            HighlightText(page, searchTerm, canvas, zoom, renderHeight);
                        }
                    }
                }
                return bitmap;
            }
            finally { FPDF_ClosePage(page); }
        }
    }

    private void HighlightText(FpdfPageT page, string term, SKCanvas canvas, double zoom, int renderHeight)
    {
        var textPage = FPDFTextLoadPage(page);
        if (textPage == null) return;

        try
        {
            // PDFium espera UTF-16 terminado em zero para busca
            byte[] bytes = System.Text.Encoding.Unicode.GetBytes(term + "\0");
            ushort[] uterm = new ushort[bytes.Length / 2];
            Buffer.BlockCopy(bytes, 0, uterm, 0, bytes.Length);

            var search = FPDFTextFindStart(textPage, ref uterm[0], 0, 0);
            if (search == null) return;

            using var paint = new SKPaint { Color = new SKColor(255, 255, 0, 100), Style = SKPaintStyle.Fill };

            while (FPDFTextFindNext(search) != 0)
            {
                int startIndex = FPDFTextGetSchResultIndex(search);
                int charCount = FPDFTextGetSchCount(search);
                
                int rectCount = FPDFTextCountRects(textPage, startIndex, charCount);
                for (int i = 0; i < rectCount; i++)
                {
                    double left = 0, top = 0, right = 0, bottom = 0;
                    if (FPDFTextGetRect(textPage, i, ref left, ref top, ref right, ref bottom) != 0)
                    {
                        float x = (float)(left * zoom);
                        float y = (float)(renderHeight - (top * zoom));
                        float w = (float)((right - left) * zoom);
                        float h = (float)((top - bottom) * zoom);

                        canvas.DrawRect(x, y, w, h, paint);
                    }
                }
            }
            FPDFTextFindClose(search);
        }
        finally { FPDFTextClosePage(textPage); }
    }

    public void Close()
    {
        lock (_lock)
        {
            if (_document != null) { FPDF_CloseDocument(_document); _document = null; _currentPath = null; }
        }
    }

    public void Dispose() => Close();

    static PdfInternalService() { FPDF_InitLibrary(); }
}
