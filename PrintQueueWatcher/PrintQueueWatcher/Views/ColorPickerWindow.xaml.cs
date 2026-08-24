using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PrintQueueWatcher.Services;

namespace PrintQueueWatcher.Views;

/// <summary>
/// Basit bir renk seçici: sabit bir hazır palet (16 renk) ve serbest hex girişi
/// sunar. WPF'te yerleşik bir ColorPicker kontrolü olmadığından, harici bir
/// bağımlılık eklemeden minimal ve güvenilir bir çözüm tercih edildi.
/// </summary>
public partial class ColorPickerWindow : Window
{
    private static readonly string[] PaletteHexColors =
    {
        "#FF1E1E1E", "#FF2D2D2D", "#FF3A3A3A", "#FF505050",
        "#FFFFFFFF", "#FFF3F3F3", "#FFE0E0E0", "#FFCCCCCC",
        "#FF0078D7", "#FF3A96DD", "#FF107C10", "#FF6CCB5F",
        "#FFD83B01", "#FFFF8C00", "#FFE81123", "#FFB4009E"
    };

    /// <summary>Kullanıcının seçtiği son renk (hex string, "#AARRGGBB").</summary>
    public string SelectedColorHex { get; private set; }

    private bool _suppressHexTextChanged;

    public ColorPickerWindow(string initialColorHex)
    {
        InitializeComponent();
        SelectedColorHex = initialColorHex;

        BuildPalette();

        _suppressHexTextChanged = true;
        HexTextBox.Text = initialColorHex;
        _suppressHexTextChanged = false;

        UpdatePreview(ColorHelper.FromHex(initialColorHex));
    }

    private void BuildPalette()
    {
        foreach (string hex in PaletteHexColors)
        {
            var color = ColorHelper.FromHex(hex);
            var swatch = new Border
            {
                Margin = new Thickness(3),
                CornerRadius = new CornerRadius(4),
                Background = ColorHelper.ToBrush(color),
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Cursor = System.Windows.Input.Cursors.Hand,
                Height = 28
            };
            swatch.MouseLeftButtonUp += (s, e) =>
            {
                SelectedColorHex = hex;
                _suppressHexTextChanged = true;
                HexTextBox.Text = hex;
                _suppressHexTextChanged = false;
                UpdatePreview(color);
            };
            PaletteGrid.Children.Add(swatch);
        }
    }

    private void HexTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_suppressHexTextChanged)
        {
            return;
        }

        string text = HexTextBox.Text.Trim();
        if (IsValidHex(text))
        {
            var color = ColorHelper.FromHex(text);
            SelectedColorHex = ColorHelper.ToHex(color);
            UpdatePreview(color);
        }
    }

    private static bool IsValidHex(string text)
    {
        if (!text.StartsWith("#"))
        {
            return false;
        }
        int len = text.Length;
        return len == 7 || len == 9; // #RRGGBB veya #AARRGGBB
    }

    private void UpdatePreview(Color color)
    {
        PreviewBorder.Background = ColorHelper.ToBrush(color);
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (!IsValidHex(HexTextBox.Text.Trim()))
        {
            MessageBox.Show("Geçersiz renk kodu. Örn: #FF0078D7", "Hata",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
