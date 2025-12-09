@echo off
echo ========================================
echo Veritabani Kurulum Scripti
echo ========================================
echo.

echo [1/3] Proje klasorune gidiliyor...
cd /d "%~dp0"

echo [2/3] EF Core tool kontrol ediliyor...
dotnet tool list -g | findstr dotnet-ef >nul
if %errorlevel% neq 0 (
    echo EF Core tool bulunamadi, yukleniyor...
    dotnet tool install --global dotnet-ef --version 6.0.29
)

echo [3/3] Veritabani olusturuluyor...
echo.
echo NOT: appsettings.json dosyasindaki SQL Server bilgilerinizi kontrol edin!
echo.
dotnet ef database update

if %errorlevel% equ 0 (
    echo.
    echo ========================================
    echo VERITABANI BASARIYLA OLUSTURULDU!
    echo ========================================
    echo.
    echo Simdi RUN_PROJECT.bat dosyasini calistirabilirsiniz.
) else (
    echo.
    echo ========================================
    echo HATA: Veritabani olusturulamadi!
    echo ========================================
    echo.
    echo Kontrol edin:
    echo 1. SQL Server calisiyor mu?
    echo 2. appsettings.json'daki connection string dogru mu?
    echo 3. SQL Server Management Studio ile baglanti test edin
)

echo.
pause

