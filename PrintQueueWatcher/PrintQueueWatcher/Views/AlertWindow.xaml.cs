using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using PrintQueueWatcher.Models;
using PrintQueueWatcher.Services;

namespace PrintQueueWatcher.Views;

/// <summary>
/// Kuyruk boşaldığında (veya Ayarlar penceresinden "Önizleme" ile) gösterilen
/// büyük uyarı penceresi. Kullanıcının seçtiği boyut moduna, arkaplan ve buton
/// rengine göre görünümünü ayarlar. Metin rengi her zaman otomatik olarak
/// arkaplanla kontrast oluşturacak şekilde (siyah/beyaz) hesaplanır; bu,
/// kullanıcının serbestçe seçtiği bir arkaplan renginin okunmaz bir kombinasyon
/// oluşturmasını engeller.
/// </summary>
public partial class AlertWindow : Window
{
    private readonly bool _isPreview;

    public AlertWindow(AppSettings settings, bool isPreview = false)
    {
        InitializeComponent();
        _isPreview = isPreview;

        ApplySize(settings.AlertSize);
        ApplyColors(settings.AlertBackgroundColor, settings.AlertButtonColor);

        MessageText.Text = LocalizationService.Get("Str_AlertMessage", Environment.NewLine);

        if (!isPreview)
        {
            Loaded += (s, e) => Activate();
        }
    }

    private void ApplySize(AlertSize size)
    {
        switch (size)
        {
            case AlertSize.Small:
                WindowStyle = WindowStyle.SingleBorderWindow;
                ResizeMode = ResizeMode.NoResize;
                Width = 480;
                Height = 280;
                MessageText.FontSize = 15;
                break;

            case AlertSize.Medium:
                WindowStyle = WindowStyle.SingleBorderWindow;
                ResizeMode = ResizeMode.NoResize;
                Width = 650;
                Height = 400;
                MessageText.FontSize = 22;
                break;

            case AlertSize.FullScreen:
                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;
                WindowState = WindowState.Maximized;
                MessageText.FontSize = 40;
                break;
        }
    }

    private void ApplyColors(string backgroundHex, string buttonHex)
    {
        Color backgroundColor = ColorHelper.FromHex(backgroundHex);
        Color buttonColor = ColorHelper.FromHex(buttonHex);

        Color textColor = ColorHelper.GetReadableTextColor(backgroundColor);
        Color buttonTextColor = ColorHelper.GetReadableTextColor(buttonColor);

        Background = ColorHelper.ToBrush(backgroundColor);
        MessageText.Foreground = ColorHelper.ToBrush(textColor);

        OkButton.Background = ColorHelper.ToBrush(buttonColor);
        OkButton.Foreground = ColorHelper.ToBrush(buttonTextColor);
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape || e.Key == Key.Enter)
        {
            Close();
        }
    }
}
