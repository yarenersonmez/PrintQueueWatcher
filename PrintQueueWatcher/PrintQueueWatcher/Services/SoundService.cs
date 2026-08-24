using System.Media;
using System.Threading.Tasks;
using PrintQueueWatcher.Models;

namespace PrintQueueWatcher.Services;

/// <summary>
/// Kullanıcının seçtiği bildirim sesini çalar.
///
/// Not: Windows'un bazı yerleşik sistem sesleri (özellikle eski "Question" sesi)
/// kullanıcının ses şemasında boş/sessiz olarak tanımlı olabilir ve bu durumda
/// hiçbir şey duyulmaz. Bunun önüne geçmek için çoğu seçenek Console.Beep
/// üzerine kurulu basit tonlardan oluşur; bunlar doğrudan hoparlörü kullanır ve
/// Windows ses şemasından bağımsız olarak her zaman duyulabilir.
/// </summary>
public class SoundService
{
    public void Play(NotificationSound sound)
    {
        // Beep çağrıları senkron ve blokluyor olduğundan (özellikle üçlü desenlerde
        // yarım saniyeye yakın sürebilir), UI thread'ini kilitlememek için
        // arka plan görevinde çalıştırılır.
        Task.Run(() => PlayInternal(sound));
    }

    private static void PlayInternal(NotificationSound sound)
    {
        try
        {
            switch (sound)
            {
                case NotificationSound.None:
                    break;

                case NotificationSound.SingleBeep:
                    Console.Beep(1000, 300);
                    break;

                case NotificationSound.DoubleBeep:
                    Console.Beep(1000, 200);
                    Task.Delay(100).Wait();
                    Console.Beep(1000, 200);
                    break;

                case NotificationSound.TripleBeep:
                    for (int i = 0; i < 3; i++)
                    {
                        Console.Beep(1000, 150);
                        Task.Delay(80).Wait();
                    }
                    break;

                case NotificationSound.LowTone:
                    Console.Beep(440, 400);
                    break;

                case NotificationSound.HighTone:
                    Console.Beep(1800, 300);
                    break;

                case NotificationSound.RisingTone:
                    Console.Beep(600, 150);
                    Console.Beep(900, 150);
                    Console.Beep(1300, 200);
                    break;

                case NotificationSound.SystemExclamation:
                    SystemSounds.Exclamation.Play();
                    break;

                case NotificationSound.SystemAsterisk:
                    SystemSounds.Asterisk.Play();
                    break;
            }
        }
        catch
        {
            // Ses donanımı yoksa veya Console.Beep desteklenmiyorsa (bazı sanal
            // makineler) sessizce yut; ses özelliği kritik değildir.
        }
    }
}
