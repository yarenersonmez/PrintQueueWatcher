using System.Windows;
using PrintQueueWatcher.Models;

namespace PrintQueueWatcher.Services;

/// <summary>
/// Uygulama genelinde dili değiştirir. App.xaml içindeki MergedDictionaries
/// listesindeki dil sözlüğünü (Strings.tr.xaml / Strings.en.xaml) değiştirerek
/// çalışır. WPF'te {DynamicResource} kullanan tüm bağlı elemanlar bu değişikliği
/// otomatik olarak yansıtır; uygulamayı yeniden başlatmaya gerek yoktur.
/// </summary>
public class LocalizationService
{
    private const string TurkishDictionaryPath = "Localization/Strings.tr.xaml";
    private const string EnglishDictionaryPath = "Localization/Strings.en.xaml";

    /// <summary>
    /// Uygulamanın aktif dilini değiştirir. App.xaml.cs içinde başlangıçta ve
    /// ayarlar penceresinden dil değiştirildiğinde çağrılır.
    /// </summary>
    public void ApplyLanguage(AppLanguage language)
    {
        string dictionaryPath = language == AppLanguage.English
            ? EnglishDictionaryPath
            : TurkishDictionaryPath;

        var newDictionary = new ResourceDictionary
        {
            Source = new Uri(dictionaryPath, UriKind.Relative)
        };

        ResourceDictionary.MergedDictionaries.Remove(
            ResourceDictionary.MergedDictionaries
                .FirstOrDefault(d => d.Source != null &&
                    (d.Source.OriginalString == TurkishDictionaryPath ||
                     d.Source.OriginalString == EnglishDictionaryPath))
            ?? new ResourceDictionary());

        ResourceDictionary.MergedDictionaries.Add(newDictionary);
    }

    /// <summary>
    /// Uygulamanın merkezi kaynak sözlüğüne (App.Resources) kısayol erişim.
    /// </summary>
    private static ResourceDictionary ResourceDictionary => Application.Current.Resources;

    /// <summary>
    /// Kod tarafında (XAML dışında) lokalize bir metne erişmek için yardımcı metod.
    /// Örneğin bir MessageBox başlığı veya dinamik olarak oluşturulan bir metin için kullanılır.
    /// </summary>
    public static string Get(string key)
    {
        if (Application.Current.TryFindResource(key) is string value)
        {
            return value;
        }
        return key;
    }

    /// <summary>
    /// Format parametreli lokalize metin (örn. "Kuyrukta {0} iş var").
    /// </summary>
    public static string Get(string key, params object[] args)
    {
        string template = Get(key);
        return string.Format(template, args);
    }
}
