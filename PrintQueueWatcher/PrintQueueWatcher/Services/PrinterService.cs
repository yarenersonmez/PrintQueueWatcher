using System.Printing;

namespace PrintQueueWatcher.Services;

/// <summary>
/// Windows'un System.Printing API'si üzerinden yazıcı listesi ve
/// seçilen yazıcıya özel kuyruk bilgisi sağlar.
///
/// Not: System.Printing, klasik spool-klasörü-sayma yöntemine göre çok daha
/// güvenilirdir çünkü doğrudan Windows Print Spooler servisiyle konuşur ve
/// yalnızca ilgilenilen yazıcının işlerini sayar; sistemdeki diğer yazıcıların
/// işlerini karıştırmaz.
/// </summary>
public class PrinterService : IDisposable
{
    private readonly PrintServer _printServer;

    public PrinterService()
    {
        _printServer = new PrintServer();
    }

    /// <summary>
    /// Sistemde kurulu tüm yazıcıların adlarını döner.
    /// Hata durumunda boş liste döner (uygulama çökmemeli).
    /// </summary>
    public List<string> GetInstalledPrinterNames()
    {
        try
        {
            var queues = _printServer.GetPrintQueues();
            return queues.Select(q => q.FullName).OrderBy(n => n).ToList();
        }
        catch
        {
            return new List<string>();
        }
    }

    /// <summary>
    /// Belirtilen isimdeki yazıcının kuyruğundaki iş sayısını döner.
    /// Yazıcı bulunamazsa veya bir hata oluşursa -1 döner; bu, çağıran
    /// tarafında "bilinmiyor/hata" durumu olarak yorumlanmalıdır.
    /// </summary>
    public int GetQueuedJobCount(string printerName)
    {
        if (string.IsNullOrWhiteSpace(printerName))
        {
            return -1;
        }

        try
        {
            using PrintQueue queue = _printServer.GetPrintQueue(printerName);
            queue.Refresh();
            return queue.NumberOfJobs;
        }
        catch
        {
            return -1;
        }
    }

    /// <summary>
    /// Sistemin varsayılan yazıcısının tam adını döner. Bulunamazsa null.
    /// İlk kurulumda kullanıcıya makul bir başlangıç seçimi sunmak için kullanılır.
    /// </summary>
    public string? GetDefaultPrinterName()
    {
        try
        {
            using PrintQueue? defaultQueue = LocalPrintServer.GetDefaultPrintQueue();
            return defaultQueue?.FullName;
        }
        catch
        {
            return null;
        }
    }

    public void Dispose()
    {
        _printServer.Dispose();
        GC.SuppressFinalize(this);
    }
}
