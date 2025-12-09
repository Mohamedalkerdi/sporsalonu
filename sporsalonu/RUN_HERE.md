# 🚀 Bu Terminal'de Projeyi Çalıştırma

## ⚡ Hızlı Komutlar

### 1. Proje Klasörüne Git
```powershell
cd C:\Users\muhammed\Desktop\sporsalonu\sporsalonu\sporsalonu\sporsalonu
```

### 2. ÖNCE: SQL Server Bağlantısını Ayarla

`appsettings.json` dosyasını açın ve connection string'i düzenleyin:

**Seçenek A: SQL Server Express kullanıyorsanız:**
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=DESKTOP-GD1SHFB\\SQLEXPRESS;Database=FitnessCenterDb;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```
*(DESKTOP-GD1SHFB\\SQLEXPRESS kısmını kendi bilgisayarınızın adıyla değiştirin)*

**Seçenek B: LocalDB kullanıyorsanız (daha kolay):**
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=FitnessCenterDb;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

### 3. Veritabanını Oluştur
```powershell
dotnet ef database update
```

### 4. Projeyi Çalıştır
```powershell
dotnet run
```

Tarayıcı otomatik açılacak: `https://localhost:7192`

---

## 📝 Tek Seferde Tüm Komutlar

```powershell
# Proje klasörüne git
cd C:\Users\muhammed\Desktop\sporsalonu\sporsalonu\sporsalonu\sporsalonu

# Paketleri yükle
dotnet restore

# Veritabanını oluştur (appsettings.json'ı düzenledikten sonra)
dotnet ef database update

# Projeyi çalıştır
dotnet run
```

---

## ⚠️ SQL Server Bağlantı Hatası Alırsanız

1. **SQL Server'ın çalıştığından emin olun:**
   - Windows Services'te "SQL Server (MSSQLSERVER)" veya "SQL Server (SQLEXPRESS)" servisinin çalıştığını kontrol edin

2. **LocalDB kullanmayı deneyin:**
   - `appsettings.json`'da connection string'i şu şekilde değiştirin:
   ```json
   "Server=(localdb)\\mssqllocaldb;Database=FitnessCenterDb;Trusted_Connection=True;MultipleActiveResultSets=true"
   ```

3. **SQL Server Management Studio ile test edin:**
   - SSMS'i açın ve aynı connection string ile bağlanmayı deneyin

---

## 🎯 Proje Çalıştıktan Sonra

- **Admin Girişi:** `ogrencinumarasi@sakarya.edu.tr` / `sau`
- **Üye Kaydı:** `/Account/Register` sayfasından

---

## 🛑 Projeyi Durdurma

Terminal'de `Ctrl+C` tuşlarına basın.

