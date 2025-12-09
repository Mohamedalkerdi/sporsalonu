# Spor Salonu Yönetim Sistemi

ASP.NET Core MVC ile geliştirilmiş spor salonu randevu ve yönetim sistemi.

## 📋 Proje Gereksinimleri

- **.NET 6.0 SDK** veya üzeri
- **Visual Studio 2022** veya **Visual Studio Code**
- **SQL Server** (LocalDB, Express veya Full Edition)
- **Internet bağlantısı** (AI API için)

## 🚀 Projeyi Çalıştırma

### Yöntem 1: Visual Studio 2022 ile

1. **Visual Studio 2022'yi açın**

2. **Projeyi açın:**
   - `File` → `Open` → `Project/Solution`
   - Şu dosyayı seçin: `FitnessCenterApp.sln`
   - Veya doğrudan `.csproj` dosyasını açabilirsiniz: `FitnessCenterApp.csproj`

3. **Veritabanı bağlantı string'ini güncelleyin:**
   - `appsettings.json` dosyasını açın
   - `ConnectionStrings` bölümündeki `DefaultConnection` değerini kendi SQL Server bilgilerinize göre düzenleyin:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=DESKTOP-GD1SHFB\\SQLEXPRESS;Database=FitnessCenterDb;Trusted_Connection=True;MultipleActiveResultSets=true"
   }
   ```
   - `DESKTOP-GD1SHFB\\SQLEXPRESS` kısmını kendi SQL Server instance adınızla değiştirin
   - Eğer LocalDB kullanıyorsanız: `Server=(localdb)\\mssqllocaldb;Database=FitnessCenterDb;Trusted_Connection=True;MultipleActiveResultSets=true`

4. **NuGet paketlerini restore edin:**
   - Solution Explorer'da projeye sağ tıklayın
   - `Restore NuGet Packages` seçeneğini tıklayın
   - Veya: `Tools` → `NuGet Package Manager` → `Package Manager Console`
   - Console'da: `dotnet restore`

5. **Veritabanını oluşturun:**
   - `Tools` → `NuGet Package Manager` → `Package Manager Console`
   - Şu komutu çalıştırın:
   ```powershell
   dotnet ef database update
   ```
   - Eğer hata alırsanız, önce şunu çalıştırın:
   ```powershell
   dotnet tool install --global dotnet-ef --version 6.0.29
   ```

6. **Admin kullanıcısını güncelleyin:**
   - `Program.cs` dosyasını açın
   - Satır 73'teki `ogrencinumarasi@sakarya.edu.tr` kısmını kendi öğrenci numaranızla değiştirin
   - Örnek: `2020123456@sakarya.edu.tr`

7. **Projeyi çalıştırın:**
   - `F5` tuşuna basın veya `Debug` → `Start Debugging`
   - Veya `Ctrl+F5` ile debug olmadan çalıştırın

8. **Tarayıcı otomatik açılacak:**
   - Eğer açılmazsa, şu adresi tarayıcıda açın: `https://localhost:7192` veya `http://localhost:5100`

### Yöntem 2: Komut Satırı (Terminal/PowerShell) ile

1. **Terminal'i açın** (PowerShell veya Command Prompt)

2. **Proje klasörüne gidin:**
   ```powershell
   cd C:\Users\muhammed\Desktop\sporsalonu\sporsalonu\sporsalonu\sporsalonu
   ```

3. **Veritabanı bağlantı string'ini güncelleyin:**
   - `appsettings.json` dosyasını bir metin editörüyle açın ve SQL Server bilgilerinizi girin

4. **NuGet paketlerini restore edin:**
   ```powershell
   dotnet restore
   ```

5. **Veritabanını oluşturun:**
   ```powershell
   dotnet ef database update
   ```
   - Eğer `dotnet ef` komutu bulunamazsa:
   ```powershell
   dotnet tool install --global dotnet-ef --version 6.0.29
   ```

6. **Admin kullanıcısını güncelleyin:**
   - `Program.cs` dosyasını açın ve öğrenci numaranızı girin

7. **Projeyi çalıştırın:**
   ```powershell
   dotnet run
   ```

8. **Tarayıcıda açın:**
   - Terminal'de gösterilen URL'yi kopyalayıp tarayıcıda açın
   - Genellikle: `https://localhost:7192` veya `http://localhost:5100`

## 🔐 Giriş Bilgileri

### Admin Girişi:
- **Email:** `ogrencinumarasi@sakarya.edu.tr` (Program.cs'de belirlediğiniz)
- **Şifre:** `sau`

### Üye Girişi:
- Kayıt sayfasından yeni kullanıcı oluşturabilirsiniz
- `/Account/Register` sayfasından kayıt olun

## 📁 Proje Yapısı

```
FitnessCenterApp/
├── Controllers/          # MVC Controller'ları
│   ├── AccountController.cs      # Giriş/Kayıt
│   ├── AppointmentController.cs  # Randevu yönetimi
│   ├── FitnessCenterController.cs # Spor salonu CRUD
│   ├── TrainerController.cs      # Antrenör CRUD
│   ├── ServiceController.cs      # Hizmet CRUD
│   ├── AIController.cs           # AI önerileri
│   └── ApiController.cs          # REST API endpoints
├── Models/               # Veri modelleri
├── Views/                # Razor view'ları
├── Data/                 # DbContext
├── Migrations/           # Veritabanı migration'ları
├── wwwroot/              # Statik dosyalar (CSS, JS, images)
├── Program.cs            # Uygulama giriş noktası
└── appsettings.json      # Yapılandırma dosyası
```

## 🛠️ Sorun Giderme

### "Could not find a part of the path" hatası:
- Visual Studio'da `FitnessCenterApp.sln` dosyasını açtığınızdan emin olun
- Veya doğrudan `FitnessCenterApp.csproj` dosyasını açın

### "SQL Server connection" hatası:
- SQL Server'ın çalıştığından emin olun
- `appsettings.json`'daki connection string'i kontrol edin
- SQL Server Management Studio ile bağlantıyı test edin

### "dotnet ef" komutu bulunamıyor:
```powershell
dotnet tool install --global dotnet-ef --version 6.0.29
```

### Migration hatası:
- Eğer veritabanı zaten varsa, önce silin:
```powershell
dotnet ef database drop
dotnet ef database update
```

### Port zaten kullanılıyor:
- `Properties/launchSettings.json` dosyasındaki port numaralarını değiştirin
- Veya çalışan uygulamayı kapatın

## 📝 Önemli Notlar

1. **İlk çalıştırmada** veritabanı otomatik oluşturulur ve admin kullanıcısı eklenir
2. **AI API Key** için `appsettings.json`'da `AI:ApiKey` değerini kendi Gemini API anahtarınızla değiştirin
3. **Swagger UI** geliştirme modunda erişilebilir: `https://localhost:7192/swagger`

## 🎯 Özellikler

✅ Spor salonu yönetimi (CRUD)
✅ Antrenör yönetimi ve müsaitlik takibi
✅ Hizmet yönetimi (fitness, yoga, pilates vb.)
✅ Randevu sistemi (çakışma kontrolü, onay mekanizması)
✅ Rol bazlı yetkilendirme (Admin/User)
✅ REST API endpoints (LINQ sorguları ile filtreleme)
✅ AI entegrasyonu (egzersiz ve diyet önerileri)
✅ Bootstrap 5 ile modern UI

## 📞 Yardım

Sorun yaşarsanız:
1. `dotnet --version` ile .NET SDK versiyonunu kontrol edin (6.0 veya üzeri olmalı)
2. Visual Studio'da `Help` → `About Microsoft Visual Studio` ile versiyonu kontrol edin
3. Projeyi temizleyip yeniden build edin: `Build` → `Clean Solution` → `Build Solution`

