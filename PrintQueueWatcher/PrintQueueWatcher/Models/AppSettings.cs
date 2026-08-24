namespace PrintQueueWatcher.Models;

/// <summary>
/// Pop-up uyarı penceresinin boyut modu.
/// </summary>
public enum AlertSize
{
    Small,
    Medium,

    /// <summary>
    /// Artık Ayarlar arayüzünde seçenek olarak sunulmuyor (kullanıcı geri
    /// bildirimiyle kaldırıldı), ancak önceden bu değeri kaydetmiş
    /// settings.json dosyalarının bozulmadan okunabilmesi için enum'da
    /// tutuluyor. Böyle bir dosya yüklenirse SettingsWindow bunu sessizce
    /// "Medium" olarak ele alır (bkz. PopulateAlertSizeList).
    /// </summary>
    FullScreen
}

/// <summary>
/// Uygulama genel görsel teması.
/// </summary>
public enum AppTheme
{
    Light,
    Dark,
    System
}

/// <summary>
/// Desteklenen diller.
/// </summary>
public enum AppLanguage
{
    Turkish,
    English
}

/// <summary>
/// Bildirim sesi seçenekleri. Sistem seslerine karşılık gelir.
/// </summary>
public enum NotificationSound
{
    None,
    SingleBeep,
    DoubleBeep,
    TripleBeep,
    LowTone,
    HighTone,
    RisingTone,
    SystemExclamation,
    SystemAsterisk
}

/// <summary>
/// %APPDATA%\PrintQueueWatcher\settings.json içinde saklanan tüm kullanıcı ayarları.
/// Yeni bir alan eklerken mevcut kullanıcıların dosyasında bu alan bulunmayacağı için
/// mutlaka makul bir varsayılan değer atanmalıdır (JSON deserializer eksik alanları
/// varsayılan değerle doldurur).
/// </summary>
public class AppSettings
{
    /// <summary>Seçili yazıcının tam adı (Windows'taki görünen ad). Boşsa henüz seçim yapılmamıştır.</summary>
    public string SelectedPrinterName { get; set; } = string.Empty;

    /// <summary>Kuyruk boşaldıktan sonra uyarı verilmeden önce beklenecek süre (saniye).</summary>
    public int WaitSeconds { get; set; } = 30;

    /// <summary>Kuyruk kaç saniyede bir kontrol edilsin.</summary>
    public int CheckIntervalSeconds { get; set; } = 2;

    /// <summary>Pop-up uyarı penceresinin boyutu.</summary>
    public AlertSize AlertSize { get; set; } = AlertSize.Medium;

    /// <summary>Pop-up arkaplan rengi (ARGB hex string, örn. "#FF1E1E1E").</summary>
    public string AlertBackgroundColor { get; set; } = "#FF1E1E1E";

    /// <summary>Pop-up buton rengi (ARGB hex string, örn. "#FF0078D7").</summary>
    public string AlertButtonColor { get; set; } = "#FF0078D7";

    /// <summary>Bildirim sesi seçimi.</summary>
    public NotificationSound Sound { get; set; } = NotificationSound.SystemExclamation;

    /// <summary>Uygulama genel teması.</summary>
    public AppTheme Theme { get; set; } = AppTheme.System;

    /// <summary>Seçili dil.</summary>
    public AppLanguage Language { get; set; } = AppLanguage.Turkish;

    /// <summary>Windows başlangıcında uygulamanın otomatik başlayıp başlamayacağı.</summary>
    public bool StartWithWindows { get; set; } = false;

    /// <summary>
    /// Artık uygulama mantığı tarafından kullanılmıyor (izleme artık her açılışta
    /// yazıcı seçiliyse otomatik başlar, bu bayrağa ihtiyaç kalmadı). Yalnızca
    /// önceki sürümlerde bu alanı içeren settings.json dosyalarının hatasız
    /// okunabilmesi için modelde tutuluyor.
    /// </summary>
    public bool WasMonitoringOnLastExit { get; set; } = false;

    /// <summary>
    /// Kullanıcı ilk açılış dil seçim penceresinde bir seçim yapmış mı.
    /// False ise uygulama açılışta dil seçim penceresini gösterir; bir kez
    /// seçim yapıldıktan sonra kalıcı olarak true'ya döner ve bir daha
    /// gösterilmez (dil daha sonra Ayarlar'dan değiştirilebilir).
    /// </summary>
    public bool HasCompletedInitialLanguageSelection { get; set; } = false;
}
