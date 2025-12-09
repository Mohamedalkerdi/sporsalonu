using FitnessCenterApp.Data;
using FitnessCenterApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Veritabanı bağlantısını ekleyin
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity yapılandırması
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Proje kurallarına göre admin şifresi "sau" olabilsin diye şifre kurallarını basitleştiriyoruz
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 3;
        options.Password.RequiredUniqueChars = 1;

        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// MVC ve Razor Pages desteği ekliyoruz
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

var app = builder.Build();
// Configure the HTTP request pipeline.
// Geliştirme ortamı için hata sayfasını göster
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Swagger endpoint ekleniyor
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
// Varsayılan yönlendirmeyi ayarlayın
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");


// Veritabanı başlatma - hata durumunda uygulama yine de çalışır (sadece arayüzü görmek için)
try
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        string[] roleNames = { "Admin", "User" };
        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Admin kullanıcısını oluştur (öğrenci numarası ile)
        // NOT: ogrencinumarasi@sakarya.edu.tr formatında kullanılmalı
        var adminEmail = "g211210585@sakarya.edu.tr";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            var user = new ApplicationUser { UserName = adminEmail, Email = adminEmail, FullName = "Admin User" };
            var createUserResult = await userManager.CreateAsync(user, "sau");
            if (createUserResult.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Admin");
            }
        }

        // Seed Data - Örnek veriler ekle (eğer veritabanı boşsa)
        var db = services.GetRequiredService<ApplicationDbContext>();
        
        // Spor Salonu ekle
        if (!db.FitnessCenters.Any())
        {
            var fitnessCenter = new FitnessCenter
            {
                Name = "Premium Fitness Center",
                Address = "Sakarya Üniversitesi Kampüsü, Serdivan/Sakarya",
                Phone = "+90 (264) 295 50 00"
            };
            db.FitnessCenters.Add(fitnessCenter);
            await db.SaveChangesAsync();

            // Antrenörler ekle
            var trainers = new List<Trainer>
            {
                new Trainer
                {
                    Name = "Ahmet Yılmaz",
                    Expertise = "Kas Geliştirme, Kuvvet Antrenmanı",
                    FitnessCenterId = fitnessCenter.FitnessCenterId
                },
                new Trainer
                {
                    Name = "Ayşe Demir",
                    Expertise = "Kilo Verme, Kardiyovasküler",
                    FitnessCenterId = fitnessCenter.FitnessCenterId
                },
                new Trainer
                {
                    Name = "Mehmet Kaya",
                    Expertise = "Yoga, Esneklik",
                    FitnessCenterId = fitnessCenter.FitnessCenterId
                },
                new Trainer
                {
                    Name = "Zeynep Şahin",
                    Expertise = "Pilates, Postür Düzeltme",
                    FitnessCenterId = fitnessCenter.FitnessCenterId
                }
            };
            db.Trainers.AddRange(trainers);
            await db.SaveChangesAsync();

            // Hizmetler ekle
            var serviceList = new List<Service>
            {
                new Service
                {
                    Name = "Kişisel Antrenman",
                    ServiceType = "Kişisel Antrenman",
                    Duration = 60,
                    Price = 300.00m,
                    FitnessCenterId = fitnessCenter.FitnessCenterId
                },
                new Service
                {
                    Name = "Fitness Grup Dersi",
                    ServiceType = "Fitness",
                    Duration = 45,
                    Price = 150.00m,
                    FitnessCenterId = fitnessCenter.FitnessCenterId
                },
                new Service
                {
                    Name = "Yoga Dersi",
                    ServiceType = "Yoga",
                    Duration = 60,
                    Price = 200.00m,
                    FitnessCenterId = fitnessCenter.FitnessCenterId
                },
                new Service
                {
                    Name = "Pilates Dersi",
                    ServiceType = "Pilates",
                    Duration = 50,
                    Price = 180.00m,
                    FitnessCenterId = fitnessCenter.FitnessCenterId
                },
                new Service
                {
                    Name = "Kardiyovasküler Antrenman",
                    ServiceType = "Kardiyovasküler",
                    Duration = 30,
                    Price = 100.00m,
                    FitnessCenterId = fitnessCenter.FitnessCenterId
                },
                new Service
                {
                    Name = "Kuvvet Antrenmanı",
                    ServiceType = "Kuvvet Antrenmanı",
                    Duration = 45,
                    Price = 250.00m,
                    FitnessCenterId = fitnessCenter.FitnessCenterId
                }
            };
            db.Services.AddRange(serviceList);
            await db.SaveChangesAsync();

            // Antrenör müsaitlik saatleri ekle (her antrenör için hafta içi 09:00-18:00)
            var availabilities = new List<TrainerAvailability>();
            foreach (var trainer in trainers)
            {
                for (DayOfWeek day = DayOfWeek.Monday; day <= DayOfWeek.Friday; day++)
                {
                    availabilities.Add(new TrainerAvailability
                    {
                        TrainerId = trainer.TrainerId,
                        DayOfWeek = day,
                        StartTime = new TimeSpan(9, 0, 0),
                        EndTime = new TimeSpan(18, 0, 0)
                    });
                }
            }
            db.TrainerAvailabilities.AddRange(availabilities);
            await db.SaveChangesAsync();

            // Spor salonu çalışma saatleri ekle
            var workingHours = new List<WorkingHours>();
            for (DayOfWeek day = DayOfWeek.Monday; day <= DayOfWeek.Saturday; day++)
            {
                workingHours.Add(new WorkingHours
                {
                    FitnessCenterId = fitnessCenter.FitnessCenterId,
                    DayOfWeek = day,
                    OpeningTime = new TimeSpan(8, 0, 0),
                    ClosingTime = new TimeSpan(20, 0, 0)
                });
            }
            db.WorkingHours.AddRange(workingHours);
            await db.SaveChangesAsync();

            Console.WriteLine("Seed data basariyla eklendi: 1 Spor Salonu, 4 Antrenor, 6 Hizmet");
        }
    }
}
catch (Exception ex)
{
    // Veritabanı bağlantı hatası - sadece arayüzü görmek için devam et
    Console.WriteLine($"Veritabani baglanti hatasi (arayuz goruntuleme modu): {ex.Message}");
    Console.WriteLine("Uygulama veritabani olmadan calisiyor - sadece arayuz gorunur.");
}


app.Run();
