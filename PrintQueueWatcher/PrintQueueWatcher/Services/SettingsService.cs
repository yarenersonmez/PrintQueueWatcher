using System.IO;
using System.Text.Json;
using PrintQueueWatcher.Models;

namespace PrintQueueWatcher.Services;

/// <summary>
/// Ayarları diskte kalıcı olarak saklar ve okur.
/// Konum: %APPDATA%\PrintQueueWatcher\settings.json
/// </summary>
public class SettingsService
{
    private readonly string _settingsFolder;
    private readonly string _settingsFilePath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public SettingsService()
    {
        _settingsFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "PrintQueueWatcher");
        _settingsFilePath = Path.Combine(_settingsFolder, "settings.json");
    }

    /// <summary>
    /// Ayarları diskten okur. Dosya yoksa veya bozuksa varsayılan ayarlarla
    /// yeni bir AppSettings döner (uygulamanın çökmesine izin vermez).
    /// </summary>
    public AppSettings Load()
    {
        try
        {
            if (!File.Exists(_settingsFilePath))
            {
                return new AppSettings();
            }

            string json = File.ReadAllText(_settingsFilePath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
            return settings ?? new AppSettings();
        }
        catch
        {
            // Bozuk/okunamayan bir ayar dosyası uygulamanın açılmasını engellememeli.
            return new AppSettings();
        }
    }

    /// <summary>
    /// Ayarları diske yazar. Klasör yoksa oluşturur.
    /// </summary>
    public void Save(AppSettings settings)
    {
        try
        {
            if (!Directory.Exists(_settingsFolder))
            {
                Directory.CreateDirectory(_settingsFolder);
            }

            string json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(_settingsFilePath, json);
        }
        catch
        {
            // Ayarların kaydedilememesi (örn. disk dolu, izin sorunu) sessizce
            // yutulur; uygulama çalışmaya devam etmelidir. İleride bir log
            // mekanizması eklenebilir.
        }
    }
}
