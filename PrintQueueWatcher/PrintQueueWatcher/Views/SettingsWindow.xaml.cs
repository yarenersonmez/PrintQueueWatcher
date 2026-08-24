using System.Windows;
using System.Windows.Controls;
using PrintQueueWatcher.Models;
using PrintQueueWatcher.Services;

namespace PrintQueueWatcher.Views;

/// <summary>
/// Ayarlar penceresi. Kullanıcı "Kaydet"e basana kadar değişiklikler yerel
/// bir kopya (_workingSettings) üzerinde tutulur; "İptal" ile pencere
/// kapatılırsa App.CurrentSettings hiç dokunulmamış olur. Bu, kullanıcının
/// yanlışlıkla yaptığı değişiklikleri geri almasını kolaylaştırır.
/// </summary>
public partial class SettingsWindow : Window
{
    private readonly AppSettings _originalSettings;
    private readonly AppSettings _workingSettings;
    private readonly SoundService _soundService = new();
    private AlertWindow? _previewWindow;

    public SettingsWindow(AppSettings currentSettings)
    {
        InitializeComponent();

        _originalSettings = currentSettings;

        // Sığ kopya yeterli: AppSettings yalnızca değer tipleri ve string içerir.
        _workingSettings = new AppSettings
        {
            SelectedPrinterName = currentSettings.SelectedPrinterName,
            WaitSeconds = currentSettings.WaitSeconds,
            CheckIntervalSeconds = currentSettings.CheckIntervalSeconds,
            AlertSize = currentSettings.AlertSize,
            AlertBackgroundColor = currentSettings.AlertBackgroundColor,
            AlertButtonColor = currentSettings.AlertButtonColor,
            Sound = currentSettings.Sound,
            Theme = currentSettings.Theme,
            Language = currentSettings.Language,
            StartWithWindows = currentSettings.StartWithWindows,
            WasMonitoringOnLastExit = currentSettings.WasMonitoringOnLastExit
        };

        Loaded += SettingsWindow_Loaded;
    }

    private void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
    {
        PopulatePrinterList();
        PopulateSoundList();
        PopulateAlertSizeList();
        PopulateThemeList();
        PopulateLanguageList();

        WaitSecondsSlider.Value = _workingSettings.WaitSeconds;
        UpdateWaitSecondsLabel();

        CheckIntervalSlider.Value = _workingSettings.CheckIntervalSeconds;
        UpdateCheckIntervalLabel();

        UpdateColorSwatch(BackgroundColorSwatch, _workingSettings.AlertBackgroundColor);
        UpdateColorSwatch(ButtonColorSwatch, _workingSettings.AlertButtonColor);
    }

    // =====================================================
    //  Liste doldurma
    // =====================================================
    private void PopulatePrinterList()
    {
        var printers = App.PrinterService.GetInstalledPrinterNames();
        PrinterComboBox.ItemsSource = printers;

        if (!string.IsNullOrWhiteSpace(_workingSettings.SelectedPrinterName) &&
            printers.Contains(_workingSettings.SelectedPrinterName))
        {
            PrinterComboBox.SelectedItem = _workingSettings.SelectedPrinterName;
        }
        else if (printers.Count > 0)
        {
            // Hiç seçim yoksa, sistemin varsayılan yazıcısını öner (varsa).
            string? defaultPrinter = App.PrinterService.GetDefaultPrinterName();
            PrinterComboBox.SelectedItem = defaultPrinter != null && printers.Contains(defaultPrinter)
                ? defaultPrinter
                : printers[0];
        }
    }

    private void RefreshPrintersButton_Click(object sender, RoutedEventArgs e)
    {
        PopulatePrinterList();
    }

    private void PopulateSoundList()
    {
        var items = new List<ComboBoxItem>
        {
            new() { Content = LocalizationService.Get("Str_SoundNone"), Tag = NotificationSound.None },
            new() { Content = LocalizationService.Get("Str_SoundSingleBeep"), Tag = NotificationSound.SingleBeep },
            new() { Content = LocalizationService.Get("Str_SoundDoubleBeep"), Tag = NotificationSound.DoubleBeep },
            new() { Content = LocalizationService.Get("Str_SoundTripleBeep"), Tag = NotificationSound.TripleBeep },
            new() { Content = LocalizationService.Get("Str_SoundLowTone"), Tag = NotificationSound.LowTone },
            new() { Content = LocalizationService.Get("Str_SoundHighTone"), Tag = NotificationSound.HighTone },
            new() { Content = LocalizationService.Get("Str_SoundRisingTone"), Tag = NotificationSound.RisingTone },
            new() { Content = LocalizationService.Get("Str_SoundSystemExclamation"), Tag = NotificationSound.SystemExclamation },
            new() { Content = LocalizationService.Get("Str_SoundSystemAsterisk"), Tag = NotificationSound.SystemAsterisk },
        };
        SoundComboBox.ItemsSource = items;
        SoundComboBox.SelectedItem = items.FirstOrDefault(i => (NotificationSound)i.Tag! == _workingSettings.Sound)
            ?? items[7];
    }

    private void PopulateAlertSizeList()
    {
        // Not: "Tam Ekran" artık bir seçenek olarak sunulmuyor. Ayarlar dosyasında
        // önceden bu değer kaydedilmiş olabileceğinden (bkz. AlertSize.FullScreen
        // açıklaması), böyle bir durumda sessizce "Orta" seçilir.
        var items = new List<ComboBoxItem>
        {
            new() { Content = LocalizationService.Get("Str_AlertSizeSmall"), Tag = AlertSize.Small },
            new() { Content = LocalizationService.Get("Str_AlertSizeMedium"), Tag = AlertSize.Medium },
        };
        AlertSizeComboBox.ItemsSource = items;

        AlertSize effectiveSize = _workingSettings.AlertSize == AlertSize.FullScreen
            ? AlertSize.Medium
            : _workingSettings.AlertSize;

        AlertSizeComboBox.SelectedItem = items.FirstOrDefault(i => (AlertSize)i.Tag! == effectiveSize)
            ?? items[1];
    }

    private void PopulateThemeList()
    {
        var items = new List<ComboBoxItem>
        {
            new() { Content = LocalizationService.Get("Str_ThemeLight"), Tag = AppTheme.Light },
            new() { Content = LocalizationService.Get("Str_ThemeDark"), Tag = AppTheme.Dark },
            new() { Content = LocalizationService.Get("Str_ThemeSystem"), Tag = AppTheme.System },
        };
        ThemeComboBox.ItemsSource = items;
        ThemeComboBox.SelectedItem = items.FirstOrDefault(i => (AppTheme)i.Tag! == _workingSettings.Theme)
            ?? items[2];
    }

    private void PopulateLanguageList()
    {
        var items = new List<ComboBoxItem>
        {
            new() { Content = "Türkçe", Tag = AppLanguage.Turkish },
            new() { Content = "English", Tag = AppLanguage.English },
        };
        LanguageComboBox.ItemsSource = items;
        LanguageComboBox.SelectedItem = items.FirstOrDefault(i => (AppLanguage)i.Tag! == _workingSettings.Language)
            ?? items[0];
    }

    // =====================================================
    //  Zamanlama
    // =====================================================
    private void WaitSecondsSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        UpdateWaitSecondsLabel();
    }

    private void UpdateWaitSecondsLabel()
    {
        int seconds = (int)WaitSecondsSlider.Value;
        WaitSecondsLabel.Text = LocalizationService.Get("Str_WaitSecondsLabel", seconds);
    }

    private void CheckIntervalSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        UpdateCheckIntervalLabel();
    }

    private void UpdateCheckIntervalLabel()
    {
        int seconds = (int)CheckIntervalSlider.Value;
        CheckIntervalLabel.Text = LocalizationService.Get("Str_CheckIntervalLabel", seconds);
    }

    // =====================================================
    //  Ses testi
    // =====================================================
    private void TestSoundButton_Click(object sender, RoutedEventArgs e)
    {
        if (SoundComboBox.SelectedItem is ComboBoxItem item && item.Tag is NotificationSound sound)
        {
            _soundService.Play(sound);
        }
    }

    // =====================================================
    //  Renk seçiciler
    // =====================================================
    private void BackgroundColorSwatch_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        OpenColorPicker(
            _workingSettings.AlertBackgroundColor,
            selectedHex =>
            {
                _workingSettings.AlertBackgroundColor = selectedHex;
                UpdateColorSwatch(BackgroundColorSwatch, selectedHex);
            });
    }

    private void ButtonColorSwatch_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        OpenColorPicker(
            _workingSettings.AlertButtonColor,
            selectedHex =>
            {
                _workingSettings.AlertButtonColor = selectedHex;
                UpdateColorSwatch(ButtonColorSwatch, selectedHex);
            });
    }

    private void OpenColorPicker(string currentHex, Action<string> onSelected)
    {
        var picker = new ColorPickerWindow(currentHex) { Owner = this };
        if (picker.ShowDialog() == true)
        {
            onSelected(picker.SelectedColorHex);
        }
    }

    private void UpdateColorSwatch(Border swatch, string hex)
    {
        var color = ColorHelper.FromHex(hex);
        swatch.Background = ColorHelper.ToBrush(color);
    }

    // =====================================================
    //  Önizleme
    // =====================================================
    private void PreviewAlertButton_Click(object sender, RoutedEventArgs e)
    {
        // Önceki önizleme penceresi hâlâ açıksa kapat; aksi halde her tıklamada
        // üst üste yeni pencereler birikir ve kullanıcı hangisine baktığını
        // şaşırabilir (renk değişikliğinin "yansımadığı" izlenimini bu verir).
        if (_previewWindow != null)
        {
            _previewWindow.Close();
            _previewWindow = null;
        }

        var previewSettings = new AppSettings
        {
            AlertSize = ((ComboBoxItem)AlertSizeComboBox.SelectedItem).Tag is AlertSize size ? size : AlertSize.Medium,
            AlertBackgroundColor = _workingSettings.AlertBackgroundColor,
            AlertButtonColor = _workingSettings.AlertButtonColor,
            Sound = NotificationSound.None // Önizlemede ses rahatsız etmesin.
        };

        _previewWindow = new AlertWindow(previewSettings, isPreview: true) { Owner = this };
        _previewWindow.Closed += (s, args) => _previewWindow = null;
        _previewWindow.Show();
        _previewWindow.Activate();
    }

    // =====================================================
    //  Kaydet / İptal
    // =====================================================
    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (PrinterComboBox.SelectedItem is string selectedPrinter)
        {
            _workingSettings.SelectedPrinterName = selectedPrinter;
        }

        _workingSettings.WaitSeconds = (int)WaitSecondsSlider.Value;
        _workingSettings.CheckIntervalSeconds = (int)CheckIntervalSlider.Value;

        if (SoundComboBox.SelectedItem is ComboBoxItem soundItem && soundItem.Tag is NotificationSound sound)
        {
            _workingSettings.Sound = sound;
        }

        if (AlertSizeComboBox.SelectedItem is ComboBoxItem sizeItem && sizeItem.Tag is AlertSize alertSize)
        {
            _workingSettings.AlertSize = alertSize;
        }

        bool themeChanged = false;
        if (ThemeComboBox.SelectedItem is ComboBoxItem themeItem && themeItem.Tag is AppTheme theme)
        {
            themeChanged = theme != _workingSettings.Theme;
            _workingSettings.Theme = theme;
        }

        bool languageChanged = false;
        if (LanguageComboBox.SelectedItem is ComboBoxItem langItem && langItem.Tag is AppLanguage language)
        {
            languageChanged = language != _workingSettings.Language;
            _workingSettings.Language = language;
        }

        // Çalışan kopyadaki tüm değerleri gerçek (App.CurrentSettings) nesneye yaz.
        CopyInto(_originalSettings, _workingSettings);

        App.SettingsService.Save(_originalSettings);

        if (themeChanged)
        {
            App.ThemeService.ApplyTheme(_originalSettings.Theme);
        }
        if (languageChanged)
        {
            App.LocalizationService.ApplyLanguage(_originalSettings.Language);
        }

        DialogResult = true;
        Close();
    }

    private static void CopyInto(AppSettings target, AppSettings source)
    {
        target.SelectedPrinterName = source.SelectedPrinterName;
        target.WaitSeconds = source.WaitSeconds;
        target.CheckIntervalSeconds = source.CheckIntervalSeconds;
        target.AlertSize = source.AlertSize;
        target.AlertBackgroundColor = source.AlertBackgroundColor;
        target.AlertButtonColor = source.AlertButtonColor;
        target.Sound = source.Sound;
        target.Theme = source.Theme;
        target.Language = source.Language;
        target.StartWithWindows = source.StartWithWindows;
        target.WasMonitoringOnLastExit = source.WasMonitoringOnLastExit;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
