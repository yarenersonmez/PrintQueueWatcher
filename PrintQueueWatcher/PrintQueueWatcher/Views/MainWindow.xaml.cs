using System.Windows;
using System.Windows.Media;
using Hardcodet.Wpf.TaskbarNotification;
using PrintQueueWatcher.Models;
using PrintQueueWatcher.Services;

namespace PrintQueueWatcher.Views;

public partial class MainWindow : Window
{
    private readonly TaskbarIcon _trayIcon;
    private readonly SoundService _soundService = new();
    private bool _isClosingToTrayAllowed = true;
    private AlertWindow? _activeAlertWindow;

    public MainWindow()
    {
        InitializeComponent();

        _trayIcon = (TaskbarIcon)Application.Current.Resources["TrayIcon"];
        SetupTrayMenu();

        App.QueueMonitor.StatusChanged += QueueMonitor_StatusChanged;
        App.QueueMonitor.PrintingCompleted += QueueMonitor_PrintingCompleted;

        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        RefreshPrinterDisplay();
        UpdateStartWithWindowsCheckbox();
        UpdateIdleStatusDisplay();

        // Bir yazıcı seçilmişse, izleme uygulamanın her açılışında otomatik
        // başlar (elle Başlat/Durdur ayrı bir tercih değil; bu uygulamanın
        // tek amacı zaten izleme yapmaktır). Bu, hem uygulama Windows ile
        // otomatik açıldığında hem de kullanıcı elle çift tıkladığında
        // aynı şekilde çalışır.
        if (!string.IsNullOrWhiteSpace(App.CurrentSettings.SelectedPrinterName))
        {
            StartMonitoring();
        }
    }

    // =====================================================
    //  Tepsi simgesi (tray) kurulumu
    // =====================================================
    private void SetupTrayMenu()
    {
        var contextMenu = new System.Windows.Controls.ContextMenu();

        var showItem = new System.Windows.Controls.MenuItem
        {
            Header = LocalizationService.Get("Str_TrayShowWindow")
        };
        showItem.Click += (s, e) => ShowFromTray();

        var statusItem = new System.Windows.Controls.MenuItem
        {
            Header = LocalizationService.Get("Str_TrayStatus")
        };
        statusItem.Click += (s, e) => ShowStatusBalloon();

        var exitItem = new System.Windows.Controls.MenuItem
        {
            Header = LocalizationService.Get("Str_TrayExit")
        };
        exitItem.Click += (s, e) => ExitApplication();

        contextMenu.Items.Add(showItem);
        contextMenu.Items.Add(statusItem);
        contextMenu.Items.Add(new System.Windows.Controls.Separator());
        contextMenu.Items.Add(exitItem);

        _trayIcon.ContextMenu = contextMenu;
        _trayIcon.TrayMouseDoubleClick += (s, e) => ShowFromTray();
        _trayIcon.ToolTipText = LocalizationService.Get("Str_TrayTooltipIdle");
    }

    private void ShowFromTray()
    {
        Show();
        WindowState = WindowState.Normal;
        ShowInTaskbar = true;
        Activate();
    }

    private void ShowStatusBalloon()
    {
        string title = LocalizationService.Get("Str_TrayBalloonTitle");
        string text = App.QueueMonitor.IsRunning
            ? LocalizationService.Get("Str_StatusMonitoring")
            : LocalizationService.Get("Str_StatusIdle");
        _trayIcon.ShowBalloonTip(title, text, BalloonIcon.Info);
    }

    private void ExitApplication()
    {
        _isClosingToTrayAllowed = false;

        App.SettingsService.Save(App.CurrentSettings);

        _trayIcon.Dispose();
        Application.Current.Shutdown();
    }

    /// <summary>
    /// Kullanıcı pencereyi (X) ile kapatmaya çalıştığında, uygulamayı
    /// sonlandırmak yerine tepsiye küçültür. Gerçek çıkış yalnızca tepsi
    /// menüsündeki "Çıkış" ile yapılabilir.
    /// </summary>
    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_isClosingToTrayAllowed)
        {
            e.Cancel = true;
            Hide();
            ShowInTaskbar = false;
        }
    }

    // =====================================================
    //  Yazıcı / durum gösterimi
    // =====================================================
    public void RefreshPrinterDisplay()
    {
        string name = App.CurrentSettings.SelectedPrinterName;
        PrinterNameText.Text = string.IsNullOrWhiteSpace(name)
            ? LocalizationService.Get("Str_NoPrinterSelected")
            : name;
    }

    private void UpdateStartWithWindowsCheckbox()
    {
        // Checked/Unchecked olaylarının tekrar tetiklenip ayarları
        // gereksiz yere kaydetmemesi için olay geçici olarak koparılır.
        StartWithWindowsCheckBox.Checked -= StartWithWindowsCheckBox_Changed;
        StartWithWindowsCheckBox.Unchecked -= StartWithWindowsCheckBox_Changed;

        StartWithWindowsCheckBox.IsChecked = App.CurrentSettings.StartWithWindows;

        StartWithWindowsCheckBox.Checked += StartWithWindowsCheckBox_Changed;
        StartWithWindowsCheckBox.Unchecked += StartWithWindowsCheckBox_Changed;
    }

    private void UpdateIdleStatusDisplay()
    {
        StatusText.Text = LocalizationService.Get("Str_StatusIdle");
        StatusDot.Fill = Brushes.Gray;
        StartStopButton.Content = LocalizationService.Get("Str_StartButton");
    }

    /// <summary>
    /// Başlat/Durdur butonunun metnini, izlemenin o anki çalışma durumuna göre
    /// aktif dile uyarlar. Buton metni {DynamicResource} yerine kod içinde elle
    /// set edildiğinden (izleme durumuna göre "Başlat"/"Durdur" arasında geçiş
    /// yapabilmek için), dil değiştiğinde bu metodun açıkça çağrılması gerekir;
    /// aksi halde buton eski dildeki metni göstermeye devam eder.
    /// </summary>
    private void RefreshStartStopButtonText()
    {
        StartStopButton.Content = App.QueueMonitor.IsRunning
            ? LocalizationService.Get("Str_StopButton")
            : LocalizationService.Get("Str_StartButton");

        // İzleme çalışmıyorsa durum metni de "Beklemede" olmalı; çalışıyorsa
        // bir sonraki QueueMonitor.Tick'te StatusChanged olayı zaten günceller,
        // ancak Tick gerçekleşene kadar geçen kısa sürede eski dildeki metin
        // görünmesin diye burada da elden geçiriyoruz.
        if (!App.QueueMonitor.IsRunning)
        {
            StatusText.Text = LocalizationService.Get("Str_StatusIdle");
        }
    }

    // =====================================================
    //  Başlat / Durdur
    // =====================================================
    private void StartStopButton_Click(object sender, RoutedEventArgs e)
    {
        if (App.QueueMonitor.IsRunning)
        {
            StopMonitoring();
        }
        else
        {
            StartMonitoring();
        }
    }

    private void StartMonitoring()
    {
        if (string.IsNullOrWhiteSpace(App.CurrentSettings.SelectedPrinterName))
        {
            MessageBox.Show(
                LocalizationService.Get("Str_SelectPrinterFirst"),
                LocalizationService.Get("Str_SettingsTitle"),
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            OpenSettingsWindow();
            return;
        }

        App.QueueMonitor.Configure(
            App.CurrentSettings.SelectedPrinterName,
            App.CurrentSettings.WaitSeconds,
            App.CurrentSettings.CheckIntervalSeconds);

        App.QueueMonitor.Start();
        StartStopButton.Content = LocalizationService.Get("Str_StopButton");
        StatusDot.Fill = (Brush)Application.Current.Resources["Brush_AccentColor"];
    }

    private void StopMonitoring()
    {
        App.QueueMonitor.Stop();
        UpdateIdleStatusDisplay();
    }

    // =====================================================
    //  QueueMonitor olayları (UI thread üzerinde tetiklenir; DispatcherTimer
    //  kullanıldığı için ekstra Invoke gerekmez)
    // =====================================================
    private void QueueMonitor_StatusChanged(object? sender, QueueStatusChangedEventArgs e)
    {
        if (e.JobCount > 0)
        {
            StatusText.Text = LocalizationService.Get("Str_StatusPrinting", e.JobCount);
            StatusDot.Fill = (Brush)Application.Current.Resources["Brush_AccentColor"];
            _trayIcon.ToolTipText = LocalizationService.Get("Str_TrayTooltipMonitoring", e.JobCount);
        }
        else if (e.JobCount == 0)
        {
            StatusText.Text = LocalizationService.Get("Str_StatusQueueEmpty", e.EmptySeconds, e.WaitThresholdSeconds);
            StatusDot.Fill = (Brush)Application.Current.Resources["Brush_SuccessColor"];
            _trayIcon.ToolTipText = LocalizationService.Get("Str_TrayTooltipIdle");
        }
        else
        {
            StatusText.Text = LocalizationService.Get("Str_StatusError");
            StatusDot.Fill = (Brush)Application.Current.Resources["Brush_WarningColor"];
        }
    }

    private void QueueMonitor_PrintingCompleted(object? sender, EventArgs e)
    {
        _soundService.Play(App.CurrentSettings.Sound);

        _trayIcon.ShowBalloonTip(
            LocalizationService.Get("Str_TrayBalloonTitle"),
            LocalizationService.Get("Str_TrayBalloonText"),
            BalloonIcon.Info);

        // Aynı anda birden fazla uyarı penceresi açılmasını engelle
        // (örn. çok kısa bekleme süresi ayarlanmışsa oluşabilecek nadir bir durum).
        if (_activeAlertWindow != null)
        {
            return;
        }

        _activeAlertWindow = new AlertWindow(App.CurrentSettings);
        _activeAlertWindow.Closed += (s, args) => _activeAlertWindow = null;
        _activeAlertWindow.Show();
        _activeAlertWindow.Activate();
    }

    // =====================================================
    //  Windows ile başlat
    // =====================================================
    private void StartWithWindowsCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        bool enabled = StartWithWindowsCheckBox.IsChecked == true;
        App.CurrentSettings.StartWithWindows = enabled;
        App.StartupService.SetEnabled(enabled);
        App.SettingsService.Save(App.CurrentSettings);
    }

    // =====================================================
    //  Ayarlar penceresi
    // =====================================================
    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        OpenSettingsWindow();
    }

    private void OpenSettingsWindow()
    {
        var settingsWindow = new SettingsWindow(App.CurrentSettings)
        {
            Owner = this
        };

        bool? result = settingsWindow.ShowDialog();

        if (result == true)
        {
            // Ayarlar kaydedildi: ekranı ve (çalışıyorsa) izleyiciyi güncelle.
            RefreshPrinterDisplay();
            UpdateStartWithWindowsCheckbox();
            RefreshStartStopButtonText();

            if (App.QueueMonitor.IsRunning)
            {
                App.QueueMonitor.Configure(
                    App.CurrentSettings.SelectedPrinterName,
                    App.CurrentSettings.WaitSeconds,
                    App.CurrentSettings.CheckIntervalSeconds);
            }

            // Dil değişmiş olabilir; tepsi menüsü metinlerini tazele.
            SetupTrayMenu();
        }
    }
}
