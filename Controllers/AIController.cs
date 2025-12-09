using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace FitnessCenterApp.Controllers
{
    [Authorize]
    public class AIController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AIController(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string description, IFormFile? photo,
            double? height, double? weight, string? bodyType, string? goal)
        {
            string apiKey = _configuration["AI:ApiKey"] ?? "";
            string provider = _configuration["AI:Provider"] ?? "Mock";
            bool hasApiKey = !string.IsNullOrWhiteSpace(apiKey);

            ViewBag.Provider = provider;
            ViewBag.HasApiKey = hasApiKey;

            var parts = new List<object>();

            // --- PROMPT HAZIRLAMA ---
            string prompt = "Sen profesyonel bir fitness koçusun. ";
            if (height.HasValue && weight.HasValue) prompt += $"Kullanıcı Boy: {height} cm, Kilo: {weight} kg. ";
            if (!string.IsNullOrEmpty(bodyType)) prompt += $"Vücut Tipi: {bodyType}. ";
            if (!string.IsNullOrEmpty(goal)) prompt += $"Hedef: {goal}. ";
            if (!string.IsNullOrEmpty(description)) prompt += $"Ek Açıklama: {description}. ";

            prompt += "Bana haftalık detaylı bir egzersiz programı ve beslenme tavsiyeleri hazırla. Cevabı sade, maddeler halinde ve motive edici bir dille ver.";

            // Fotoğraf varsa ekle
            if (photo != null && photo.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await photo.CopyToAsync(memoryStream);
                    var base64Image = Convert.ToBase64String(memoryStream.ToArray());
                    parts.Add(new { inline_data = new { mime_type = photo.ContentType, data = base64Image } });
                    prompt += " (Eklenen fotoğraftaki vücut yapısını da analiz et).";
                }
            }
            parts.Add(new { text = prompt });

            // --- GOOGLE GEMINI (Çoklu Model Denemesi) ---
            if (provider.Equals("Google", StringComparison.OrdinalIgnoreCase) && hasApiKey)
            {
                // Denenecek modeller listesi (Biri mutlaka çalışır)
                string[] modelsToTry = { "gemini-1.5-flash", "gemini-1.5-flash-latest", "gemini-1.0-pro", "gemini-pro" };

                string finalResult = null;
                string lastError = "";

                foreach (var modelName in modelsToTry)
                {
                    var result = await CallGoogleGemini(apiKey, modelName, parts);
                    if (result.Success)
                    {
                        finalResult = result.Text;
                        break; // Başarılı olduysa döngüden çık
                    }
                    else
                    {
                        lastError = result.Error; // Hatayı kaydet, bir sonraki modeli dene
                    }
                }

                if (finalResult != null)
                {
                    ProcessResponse(finalResult, height, weight, photo != null);
                }
                else
                {
                    // Hiçbiri çalışmadıysa son hatayı göster
                    ViewBag.Error = $"Tüm modeller denendi fakat başarısız oldu. Son Hata: {lastError}";
                    ProcessResponse(GenerateMockSuggestions(prompt, goal), height, weight, photo != null);
                }
            }
            else
            {
                // Mock Modu
                ProcessResponse(GenerateMockSuggestions(prompt, goal), height, weight, photo != null);
            }

            return View();
        }

        // Google API Çağıran Yardımcı Metot
        private async Task<(bool Success, string? Text, string? Error)> CallGoogleGemini(string apiKey, string model, List<object> parts)
        {
            try
            {
                // v1beta API'sini kullanıyoruz
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

                var requestBody = new { contents = new[] { new { parts = parts } } };
                var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var resultJson = await response.Content.ReadAsStringAsync();
                    var jsonNode = JsonNode.Parse(resultJson);
                    var text = jsonNode?["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();
                    return (true, text, null);
                }
                else
                {
                    var errorDetail = await response.Content.ReadAsStringAsync();
                    // Hata detayını temizleyip dönelim
                    return (false, null, $"{model} Hatası: {response.StatusCode} - {errorDetail}");
                }
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }

        private void ProcessResponse(string? text, double? height, double? weight, bool hasPhoto)
        {
            if (string.IsNullOrEmpty(text)) text = "Sonuç üretilemedi.";

            ViewBag.SuggestionsList = text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            ViewBag.HasPhoto = hasPhoto;

            if (height.HasValue && weight.HasValue)
            {
                double h = height.Value / 100.0;
                ViewBag.BMI = weight.Value / (h * h);
                ViewBag.Height = height.Value;
                ViewBag.Weight = weight.Value;
            }
        }

        private string GenerateMockSuggestions(string prompt, string? goal)
        {
            var sb = new StringBuilder();
            sb.AppendLine("--- SİMÜLASYON RAPORU (Bağlantı Hatası) ---");
            sb.AppendLine("Üzgünüz, Google AI servislerine şu an ulaşılamıyor. İşte örnek bir plan:");
            sb.AppendLine();
            sb.AppendLine("Haftalık Program:");
            sb.AppendLine("• Pazartesi: Tüm Vücut (Squat, Push-up, Row) - 3 Set x 12 Tekrar");
            sb.AppendLine("• Salı: Dinlenme ve Esneme");
            sb.AppendLine("• Çarşamba: Kardiyo ve Karın - 30dk Koşu + Plank");
            sb.AppendLine("• Perşembe: Üst Vücut (Omuz Press, Biceps Curl)");
            sb.AppendLine("• Cuma: Alt Vücut ve Esneme");
            sb.AppendLine();
            sb.AppendLine("Beslenme:");
            sb.AppendLine("• Günde 2.5 Litre su için.");
            sb.AppendLine("• İşlenmiş gıdalardan uzak durun.");
            return sb.ToString();
        }
    }
}