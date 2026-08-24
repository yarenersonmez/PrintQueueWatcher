using System.Windows.Media;

namespace PrintQueueWatcher.Services;

/// <summary>
/// Renk dönüşümleri ve otomatik kontrast hesaplama için yardımcı fonksiyonlar.
/// Kullanıcı uyarı penceresi için serbestçe arkaplan/buton rengi seçebildiğinden,
/// üzerine yazılacak metnin her koşulda okunabilir kalması için arkaplanın
/// parlaklığına göre siyah veya beyaz metin rengi otomatik seçilir.
/// </summary>
public static class ColorHelper
{
    /// <summary>"#AARRGGBB" veya "#RRGGBB" formatındaki hex string'i Color'a çevirir.</summary>
    public static Color FromHex(string hex)
    {
        try
        {
            var converted = ColorConverter.ConvertFromString(hex);
            if (converted is Color color)
            {
                return color;
            }
        }
        catch
        {
            // Geçersiz hex string durumunda aşağıdaki varsayılana düşülür.
        }
        return Colors.Gray;
    }

    /// <summary>Color'ı "#AARRGGBB" formatında hex string'e çevirir.</summary>
    public static string ToHex(Color color)
    {
        return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    /// <summary>
    /// Verilen arkaplan rengine göre okunabilir bir metin rengi (siyah veya beyaz) döner.
    /// YIQ parlaklık formülü kullanılır (standart, insan gözü algısına yakın bir ağırlıklandırma).
    /// </summary>
    public static Color GetReadableTextColor(Color background)
    {
        double yiq = (background.R * 299 + background.G * 587 + background.B * 114) / 1000.0;
        return yiq >= 150 ? Colors.Black : Colors.White;
    }

    /// <summary>Bir Color'dan SolidColorBrush oluşturur (dondurulmuş, performans için).</summary>
    public static SolidColorBrush ToBrush(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }
}
