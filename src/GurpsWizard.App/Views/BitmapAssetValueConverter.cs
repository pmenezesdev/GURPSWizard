using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace GurpsWizard.App.Views;

public class BitmapAssetValueConverter : IValueConverter
{
    public static BitmapAssetValueConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string rawUri && !string.IsNullOrWhiteSpace(rawUri))
        {
            try
            {
                var uri = new Uri(rawUri, UriKind.RelativeOrAbsolute);
                if (!uri.IsAbsoluteUri)
                {
                    // Tenta prefixo com e sem a barra inicial duplicada
                    var path = rawUri.StartsWith("/") ? rawUri : $"/{rawUri}";
                    uri = new Uri($"avares://GurpsWizard.App{path}");
                }

                Console.WriteLine($"[Converter] Tentando carregar: {uri}");
                using var asset = AssetLoader.Open(uri);
                return new Bitmap(asset);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Converter] ERRO ao carregar {rawUri}: {ex.Message}");
                return null;
            }
        }

        return null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
