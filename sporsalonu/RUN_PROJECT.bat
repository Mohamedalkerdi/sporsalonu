@echo off
echo ========================================
echo Spor Salonu Projesi - Calistirma Scripti
echo ========================================
echo.

echo [1/4] Proje klasorune gidiliyor...
cd /d "%~dp0"

echo [2/4] NuGet paketleri restore ediliyor...
dotnet restore
if %errorlevel% neq 0 (
    echo HATA: NuGet paketleri restore edilemedi!
    pause
    exit /b 1
)

echo [3/4] Veritabani migration kontrol ediliyor...
echo NOT: Eger ilk kez calistiriyorsaniz, once 'dotnet ef database update' komutunu calistirin!
echo.

echo [4/4] Proje calistiriliyor...
echo.
echo Tarayici otomatik acilacak...
echo Eger acilmazsa: https://localhost:7192 adresini kullanin
echo.
echo DURDURMAK ICIN: Ctrl+C
echo.

dotnet run

pause

