using System.Windows;
using PrintQueueWatcher.Models;
using PrintQueueWatcher.Services;
using PrintQueueWatcher.Views;

namespace PrintQueueWatcher;

/// <summary>
/// Uygulamanın giriş noktası. Sorumlulukları:
/// - Tek instance garantisi (Mutex ile; kullanıcı yanlışlıkla iki kere açarsa
///   ikinci kopya sessizce kapanır, böylece iki ayrı izleyici çakışmaz).
/// - Ortak servislerin (Settings, Printer, Theme, Localization) tek nokta
///   üzerinden erişilebilir olmasını sağlamak (basit bir servis konteyneri).
/// - Başlangıçta kayıtlı dil ve temayı uygulamak.
/// - "--autostart" komut satırı argümanı geldiğinde (Windows başlangıcından
///   tetiklendiğinde) pencereyi gizli başlatmak. İzlemenin kendisini otomatik
///   başlatma mantığı burada değil, MainWindow.xaml.cs -> MainWindow_Loaded
///   içindedir: bir yazıcı seçiliyse izleme her açılışta (autostart olsun
///   olmasın) otomatik başlar.
/// </summary>
public partial class App : Application
{
    private const string MutexName = "PrintQueueWatcher_SingleInstance_Mutex";
    private System.Threading.Mutex? _singleInstanceMutex;

    // Uygulama genelinde paylaşılan servisler. Basit bir proje olduğu için
    // tam bir DI konteyneri yerine statik erişim tercih edildi; büyürse
    // Microsoft.Extensions.DependencyInjection'a geçilebilir.
    public static SettingsService SettingsService { get; private set; } = null!;
    public static PrinterService PrinterService { get; private set; } = null!;
    public static ThemeService ThemeService { get; private set; } = null!;
    public static LocalizationService LocalizationService { get; private set; } = null!;
    public static QueueMonitor QueueMonitor { get; private set; } = null!;
    public static StartupService StartupService { get; private set; } = null!;

    public static AppSettings CurrentSettings { get; set; } = null!;

    /// <summary>Komut satırından "--autostart" geldiyse true olur.</summary>
    public static bool LaunchedViaAutostart { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        // --- Tek instance kontrolü ---
        _singleInstanceMutex = new System.Threading.Mutex(true, MutexName, out bool isNewInstance);
        if (!isNewInstance)
        {
            // Zaten çalışan bir kopya var; bu ikinci kopya hemen kapanır.
            Shutdown();
            return;
        }

        LaunchedViaAutostart = e.Args.Contains("--autostart");

        // --- Servisleri başlat ---
        SettingsService = new SettingsService();
        PrinterService = new PrinterService();
        ThemeService = new ThemeService();
        LocalizationService = new LocalizationService();
        QueueMonitor = new QueueMonitor(PrinterService);
        StartupService = new StartupService();

        CurrentSettings = SettingsService.Load();

        // --- İlk kurulum dil seçimi ---
        // Otomatik başlangıçta (Windows açılışı) kullanıcı ekranda değil
        // olabileceğinden bu pencere gösterilmez; dil zaten daha önce ya
        // ayarlanmış ya da varsayılan (Türkçe) olarak kalır.
        if (!CurrentSettings.HasCompletedInitialLanguageSelection && !LaunchedViaAutostart)
        {
            var languageWindow = new LanguageSelectionWindow();
            bool? result = languageWindow.ShowDialog();

            if (result == true)
            {
                CurrentSettings.Language = languageWindow.SelectedLanguage;
            }

            CurrentSettings.HasCompletedInitialLanguageSelection = true;
            SettingsService.Save(CurrentSettings);
        }

        // --- Dil ve temayı ayarlara göre uygula ---
        LocalizationService.ApplyLanguage(CurrentSettings.Language);
        ThemeService.ApplyTheme(CurrentSettings.Theme);

        base.OnStartup(e);

        var mainWindow = new MainWindow();

        // Otomatik başlangıçta (Windows açılışı) pencereyi göstermeden
        // doğrudan tepsiye küçültülmüş şekilde başlat; kullanıcıyı rahatsız etmesin.
        if (LaunchedViaAutostart)
        {
            mainWindow.WindowState = WindowState.Minimized;
            mainWindow.ShowInTaskbar = false;
            mainWindow.Show();
            mainWindow.Hide();
        }
        else
        {
            mainWindow.Show();
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _singleInstanceMutex?.ReleaseMutex();
        PrinterService?.Dispose();
        base.OnExit(e);
    }
}
