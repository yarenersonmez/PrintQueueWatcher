using System.Windows.Threading;

namespace PrintQueueWatcher.Services;

/// <summary>
/// Anlık kuyruk durumunu UI'a bildirmek için kullanılan olay verisi.
/// </summary>
public class QueueStatusChangedEventArgs : EventArgs
{
    /// <summary>Kuyruktaki iş sayısı. -1 ise kuyruk okunamadı (hata).</summary>
    public int JobCount { get; init; }

    /// <summary>Kuyruk boş kaldıktan bu yana geçen süre (saniye). Kuyrukta iş varsa 0.</summary>
    public int EmptySeconds { get; init; }

    /// <summary>Uyarı eşiğine ulaşmak için gereken toplam süre (saniye).</summary>
    public int WaitThresholdSeconds { get; init; }
}

/// <summary>
/// Belirlenen yazıcının kuyruğunu periyodik olarak izler. Kuyruk tamamen
/// boşaldıktan sonra ayarlanan süre kadar boş kalırsa bir kerelik
/// "PrintingCompleted" olayı fırlatır. Bu olay yalnızca boş->dolu->boş
/// geçişinde tekrar tetiklenebilir (aynı boşluk döneminde tekrar tekrar
/// ateşlenmez).
///
/// DispatcherTimer kullanılır çünkü bu, WPF UI thread'i üzerinde çalışır ve
/// olay handler'larının doğrudan UI'ı güncellemesine izin verir (Invoke
/// gerektirmez).
/// </summary>
public class QueueMonitor
{
    private readonly PrinterService _printerService;
    private readonly DispatcherTimer _timer;

    private string _printerName = string.Empty;
    private int _waitSeconds = 30;
    private int _checkIntervalSeconds = 2;

    private int _emptySeconds;
    private bool _alertFiredForThisEmptyPeriod;

    public bool IsRunning { get; private set; }

    /// <summary>Her kontrol tetiklendiğinde (her Tick'te) fırlatılır. UI'ın anlık durumu göstermesi için.</summary>
    public event EventHandler<QueueStatusChangedEventArgs>? StatusChanged;

    /// <summary>Kuyruk, ayarlanan süre kadar kesintisiz boş kaldığında (yazdırma tamamlandığında) bir kez fırlatılır.</summary>
    public event EventHandler? PrintingCompleted;

    public QueueMonitor(PrinterService printerService)
    {
        _printerService = printerService;
        _timer = new DispatcherTimer();
        _timer.Tick += OnTick;
    }

    /// <summary>
    /// İzlenecek yazıcıyı ve zamanlama parametrelerini ayarlar.
    /// İzleme çalışırken de çağrılabilir (ayarlar penceresinden değişiklik yapıldığında);
    /// bir sonraki Tick'te yeni değerler kullanılır.
    /// </summary>
    public void Configure(string printerName, int waitSeconds, int checkIntervalSeconds)
    {
        _printerName = printerName;
        _waitSeconds = Math.Max(5, waitSeconds);
        _checkIntervalSeconds = Math.Max(1, checkIntervalSeconds);
        _timer.Interval = TimeSpan.FromSeconds(_checkIntervalSeconds);
    }

    public void Start()
    {
        if (string.IsNullOrWhiteSpace(_printerName))
        {
            throw new InvalidOperationException("İzleme başlatılmadan önce bir yazıcı seçilmelidir.");
        }

        _emptySeconds = 0;
        _alertFiredForThisEmptyPeriod = true; // Başlangıçta uyarı vermesin; ilk boşlukta doğal akış başlasın.
        IsRunning = true;
        _timer.Start();
    }

    public void Stop()
    {
        _timer.Stop();
        IsRunning = false;
    }

    private void OnTick(object? sender, EventArgs e)
    {
        int count = _printerService.GetQueuedJobCount(_printerName);

        if (count > 0)
        {
            _emptySeconds = 0;
            _alertFiredForThisEmptyPeriod = false;
        }
        else if (count == 0)
        {
            _emptySeconds += _checkIntervalSeconds;

            if (_emptySeconds >= _waitSeconds && !_alertFiredForThisEmptyPeriod)
            {
                _alertFiredForThisEmptyPeriod = true;
                PrintingCompleted?.Invoke(this, EventArgs.Empty);
            }
        }
        // count == -1 (hata) durumunda emptySeconds sayacını değiştirmiyoruz;
        // geçici bir okuma hatası yanlışlıkla erken uyarıya yol açmasın.

        StatusChanged?.Invoke(this, new QueueStatusChangedEventArgs
        {
            JobCount = count,
            EmptySeconds = _emptySeconds,
            WaitThresholdSeconds = _waitSeconds
        });
    }
}
