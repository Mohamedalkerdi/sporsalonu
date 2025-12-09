# 🚀 Hızlı Başlangıç - Projeyi Çalıştırma

## ⚡ Hemen Çalıştır (Terminal/PowerShell)

### Adım 1: Proje Klasörüne Git
```powershell
cd C:\Users\muhammed\Desktop\sporsalonu\sporsalonu\sporsalonu\sporsalonu
```

### Adım 2: NuGet Paketlerini Yükle (İlk kez çalıştırıyorsanız)
```powershell
dotnet restore
```

### Adım 3: Veritabanını Oluştur (İlk kez çalıştırıyorsanız)
```powershell
# EF Core tool'u yükle (eğer yoksa)
dotnet tool install --global dotnet-ef --version 6.0.29

# Veritabanını oluştur
dotnet ef database update
```

**ÖNEMLİ:** `appsettings.json` dosyasındaki SQL Server bilgilerinizi kontrol edin!

### Adım 4: Projeyi Çalıştır
```powershell
dotnet run
```

Tarayıcı otomatik açılacak. Eğer açılmazsa:
- `https://localhost:7192` 
- veya `http://localhost:5100`

## 📝 Önce Yapılması Gerekenler

### 1. SQL Server Bağlantısını Ayarla
`appsettings.json` dosyasını açın ve connection string'i düzenleyin:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER\\SQLEXPRESS;Database=FitnessCenterDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

**LocalDB kullanıyorsanız:**
```json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=FitnessCenterDb;Trusted_Connection=True;MultipleActiveResultSets=true"
```

### 2. Admin Kullanıcısını Ayarla
`Program.cs` dosyasını açın (satır 73) ve öğrenci numaranızı girin:

```csharp
var adminEmail = "2020123456@sakarya.edu.tr"; // Kendi numaranızı yazın
```

## 🎯 Giriş Bilgileri

**Admin:**
- Email: `ogrencinumarasi@sakarya.edu.tr` (Program.cs'de belirlediğiniz)
- Şifre: `sau`

**Üye:**
- `/Account/Register` sayfasından kayıt olabilirsiniz

## 🛠️ Sorun Giderme

### "dotnet ef" komutu bulunamıyor
```powershell
dotnet tool install --global dotnet-ef --version 6.0.29
```

### SQL Server bağlantı hatası
- SQL Server'ın çalıştığından emin olun
- SQL Server Management Studio ile bağlantıyı test edin
- Connection string'i kontrol edin

### Port zaten kullanılıyor
`Properties/launchSettings.json` dosyasındaki port numaralarını değiştirin

## 📌 Tek Komutla Çalıştırma

Tüm adımları tek seferde yapmak için:

```powershell
cd C:\Users\muhammed\Desktop\sporsalonu\sporsalonu\sporsalonu\sporsalonu
dotnet restore
dotnet ef database update
dotnet run
```

