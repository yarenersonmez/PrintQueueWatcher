# PrintQueueWatcher

Windows yazdırma kuyruğunu izleyen, kuyruk boşaldığında görsel ve sesli uyarı veren basit bir masaüstü uygulaması.

Özellikle **ardışık olarak çok sayıda yazdırma işi gönderildiğinde** (örneğin bir PDF'in sayfalarını gruplar halinde yazdırırken), bir önceki işin tamamlanıp tamamlanmadığını bilmeden yeni iş göndermek bazı yazıcı/sürücü kombinasyonlarında hatalı veya eksik baskıya yol açabilir. PrintQueueWatcher, kuyruğun gerçekten boşaldığını ve belirlediğiniz bir süre boyunca boş kaldığını tespit ederek "artık yeni işi güvenle gönderebilirsiniz" uyarısı verir.

*Read this in English: [README.en.md](README.en.md)*

## Özellikler

- **Sistem tepsisinde çalışır** — görev çubuğunu kaplamaz, saatin yanındaki simgeler arasında yer alır.
- **Gerçek Windows Print API** (`System.Printing`) kullanır — spool klasörü dosya sayımı gibi kırılgan yöntemlere dayanmaz, seçilen yazıcıya özel iş sayısını doğru okur.
- **Büyük, göze çarpan uyarı penceresi** — küçük / orta boyut seçenekleri.
- **Özelleştirilebilir renkler** — uyarı penceresinin arkaplan ve buton rengini seçebilirsiniz; metin rengi okunabilirlik için otomatik hesaplanır.
- **Bildirim sesi seçimi** — sessiz veya çeşitli tonlar arasından seçim, "Sesi Test Et" ile önceden dinleyebilirsiniz.
- **Ayarlanabilir bekleme süresi** — kuyruk boşaldıktan kaç saniye sonra uyarı verileceği kaydırma çubuğuyla ayarlanır.
- **Ayarlanabilir kontrol sıklığı** — programın kuyruğa ne sıklıkla bakacağı da ayarlanabilir; düşük donanımlı bilgisayarlarda artırılabilir.
- **Açık / Koyu / Sistem teması.**
- **Türkçe ve İngilizce dil desteği** — ilk açılışta sorulur, seçim kalıcıdır, sonradan Ayarlar'dan değiştirilebilir.
- **Windows ile otomatik başlatma** — bir kayıt defteri anahtarı ile, yönetici yetkisi gerektirmez. Bir yazıcı seçiliyse izleme her açılışta otomatik başlar.

## Ekran Görüntüleri

> _Buraya ana pencere ve ayarlar penceresi ekran görüntüleri eklenebilir._

## İndir ve Çalıştır

Kurulum gerektirmez. [Releases](../../releases) sayfasından en son `PrintQueueWatcher.exe` dosyasını indirip doğrudan çalıştırabilirsiniz. Uygulama tek bir dosyadır; .NET Runtime'ı ayrıca kurmanıza gerek yoktur (gerekli her şey exe'nin içine gömülüdür).

> Windows SmartScreen, imzasız bir exe olduğu için ilk çalıştırmada bir uyarı gösterebilir. "Daha fazla bilgi" → "Yine de çalıştır" ile devam edebilirsiniz.

## Gereksinimler

- Windows 10 veya üzeri (64-bit)
- Kaynak koddan derlemek isteyenler için: [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) ve Visual Studio 2022 (17.8 veya üzeri önerilir)

## Kaynak Koddan Derleme (Geliştirici)

1. Bu depoyu klonlayın:
   ```
   git clone https://github.com/<kullanici-adiniz>/PrintQueueWatcher.git
   ```
2. `PrintQueueWatcher.sln` dosyasını Visual Studio ile açın.
3. NuGet paketlerinin geri yüklenmesini bekleyin (`Hardcodet.NotifyIcon.Wpf`).
4. F5 ile derleyip çalıştırın.

### Tek dosyalık .exe üretmek (publish)

Dağıtılabilir tek bir `.exe` dosyası üretmek için proje klasöründe şu komutu çalıştırın:

```
dotnet publish PrintQueueWatcher/PrintQueueWatcher.csproj -c Release
```

Çıktı `PrintQueueWatcher/bin/Release/net8.0-windows/win-x64/publish/PrintQueueWatcher.exe` altında oluşur. Bu dosya self-contained'dir (yaklaşık 150-160 MB) ve başka bir Windows bilgisayara kopyalanıp doğrudan çalıştırılabilir.

## Kullanım

1. Uygulamayı ilk açtığınızda dil seçimi sorulur (bir defalık).
2. **Ayarlar**'a girip izlemek istediğiniz yazıcıyı seçin.
3. İsterseniz bekleme süresini, kontrol sıklığını, bildirim sesini, uyarı penceresi görünümünü ve temayı ayarlayın; **Kaydet**'e basın.
4. Bir yazıcı seçiliyse izleme otomatik başlar. Uygulama artık arka planda (sistem tepsisinde) çalışmaya devam eder. Yazdırma işlerinizi gönderin; kuyruk tamamen boşalıp belirlediğiniz süre geçtiğinde büyük bir uyarı penceresi ve ses ile bilgilendirilirsiniz.
5. Uygulamayı tamamen kapatmak için, sistem tepsisindeki simgeye **sağ tıklayıp "Çıkış"** seçeneğini kullanın (pencereyi X ile kapatmak yalnızca tepsiye küçültür, uygulama arka planda çalışmaya devam eder).

## Proje Yapısı

```
PrintQueueWatcher/
├── Models/            AppSettings ve ilgili enum'lar
├── Services/           İş mantığı: yazıcı okuma, kuyruk izleme, ayarlar, tema, dil, ses, başlangıç kaydı
├── Views/              WPF pencereleri: MainWindow, SettingsWindow, AlertWindow, ColorPickerWindow
├── Localization/        Strings.tr.xaml, Strings.en.xaml
├── Resources/           Tema renk sözlükleri, ortak stiller, uygulama ikonu
└── App.xaml(.cs)        Giriş noktası, tek instance kontrolü, servis kayıtları
```

## Katkıda Bulunma

Sorun bildirimleri (issue) ve pull request'ler memnuniyetle karşılanır. Büyük değişiklikler için önce bir issue açıp tartışmanızı öneririz.

## Lisans

Bu proje [MIT Lisansı](LICENSE) ile lisanslanmıştır.
