using System.Windows;
using PrintQueueWatcher.Models;

namespace PrintQueueWatcher.Views;

/// <summary>
/// Uygulama ilk kez açıldığında (daha önce hiç dil seçimi yapılmamışsa)
/// gösterilen basit dil seçim penceresi. Seçim App.xaml.cs tarafından okunur
/// ve kalıcı olarak ayarlara yazılır; bir daha gösterilmez.
/// </summary>
public partial class LanguageSelectionWindow : Window
{
    public AppLanguage SelectedLanguage { get; private set; } = AppLanguage.Turkish;

    public LanguageSelectionWindow()
    {
        InitializeComponent();
    }

    private void TurkishButton_Click(object sender, RoutedEventArgs e)
    {
        SelectedLanguage = AppLanguage.Turkish;
        DialogResult = true;
        Close();
    }

    private void EnglishButton_Click(object sender, RoutedEventArgs e)
    {
        SelectedLanguage = AppLanguage.English;
        DialogResult = true;
        Close();
    }
}
