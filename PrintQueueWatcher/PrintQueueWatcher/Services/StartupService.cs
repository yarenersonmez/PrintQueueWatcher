using Microsoft.Win32;

namespace PrintQueueWatcher.Services;

/// <summary>
/// Uygulamanın Windows açılışında otomatik başlamasını Registry "Run" anahtarı
/// üzerinden yönetir. HKEY_CURRENT_USER altında çalışır; bu sayede yönetici
/// yetkisi gerektirmez ve yalnızca mevcut kullanıcıyı etkiler.
/// </summary>
public class StartupService
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppRegistryValueName = "PrintQueueWatcher";

    /// <summary>
    /// Uygulamanın şu anda Windows başlangıcına kayıtlı olup olmadığını kontrol eder.
    /// </summary>
    public bool IsRegistered()
    {
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: false);
            return key?.GetValue(AppRegistryValueName) != null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Uygulamayı Windows başlangıcına ekler veya kaldırır.
    /// </summary>
    public void SetEnabled(bool enabled)
    {
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);
            if (key == null)
            {
                return;
            }

            if (enabled)
            {
                string exePath = Environment.ProcessPath
                    ?? System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName
                    ?? string.Empty;

                if (string.IsNullOrEmpty(exePath))
                {
                    return;
                }

                // Uygulama açılışta otomatik olarak izlemeyi de başlatsın diye
                // özel bir komut satırı bayrağı ekleniyor (App.xaml.cs bunu okuyacak).
                key.SetValue(AppRegistryValueName, $"\"{exePath}\" --autostart");
            }
            else
            {
                key.DeleteValue(AppRegistryValueName, throwOnMissingValue: false);
            }
        }
        catch
        {
            // Registry erişim sorunları sessizce yutulur; UI tarafında ayrı bir
            // doğrulama (IsRegistered ile geri okuma) yapılabilir.
        }
    }
}
