using System.Windows;
using Microsoft.Win32;
using PrintQueueWatcher.Models;

namespace PrintQueueWatcher.Services;

/// <summary>
/// Uygulama genelinde açık/koyu tema uygular. "Sistem" seçeneği, Windows'un
/// kendi "Uygulama Modu" (AppsUseLightTheme) registry değerini okuyarak
/// kullanıcının işletim sistemi tercihini takip eder.
/// </summary>
public class ThemeService
{
    private const string LightDictionaryPath = "Resources/Theme.Light.xaml";
    private const string DarkDictionaryPath = "Resources/Theme.Dark.xaml";

    private const string PersonalizeRegistryPath =
        @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

    /// <summary>
    /// Verilen tema tercihine göre (System ise önce Windows ayarını okuyarak)
    /// uygun renk sözlüğünü uygular.
    /// </summary>
    public void ApplyTheme(AppTheme theme)
    {
        bool useDark = theme switch
        {
            AppTheme.Dark => true,
            AppTheme.Light => false,
            AppTheme.System => !IsSystemUsingLightTheme(),
            _ => false
        };

        string dictionaryPath = useDark ? DarkDictionaryPath : LightDictionaryPath;

        var newDictionary = new ResourceDictionary
        {
            Source = new Uri(dictionaryPath, UriKind.Relative)
        };

        var dictionaries = Application.Current.Resources.MergedDictionaries;

        var existing = dictionaries.FirstOrDefault(d =>
            d.Source != null &&
            (d.Source.OriginalString == LightDictionaryPath ||
             d.Source.OriginalString == DarkDictionaryPath));

        if (existing != null)
        {
            dictionaries.Remove(existing);
        }

        dictionaries.Add(newDictionary);
    }

    /// <summary>
    /// Windows'un sistem genelinde açık tema kullanıp kullanmadığını kontrol eder.
    /// Okunamazsa (eski Windows sürümü, registry erişim sorunu vb.) varsayılan
    /// olarak açık tema kullanıldığı varsayılır.
    /// </summary>
    private bool IsSystemUsingLightTheme()
    {
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(PersonalizeRegistryPath);
            object? value = key?.GetValue("AppsUseLightTheme");
            if (value is int intValue)
            {
                return intValue != 0;
            }
        }
        catch
        {
            // Sessizce varsayılana düş.
        }
        return true;
    }
}
