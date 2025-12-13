using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

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
            bool hasApiKey = !string.IsNullOrWhiteSpace(apiKey);
            ViewBag.Provider = "OpenAI";
            ViewBag.HasApiKey = hasApiKey;

            // Prompt oluşturma
            string prompt = "Sen profesyonel bir fitness koçusun. ";
            if (height.HasValue && weight.HasValue) prompt += $"Boy: {height} cm, Kilo: {weight} kg. ";
            if (!string.IsNullOrEmpty(bodyType)) prompt += $"Vücut Tipi: {bodyType}. ";
            if (!string.IsNullOrEmpty(goal)) prompt += $"Hedef: {goal}. ";
            if (!string.IsNullOrEmpty(description)) prompt += $"Açıklama: {description}. ";
            prompt += "Kişiye özel haftalık egzersiz planı ve beslenme önerisi hazırla. Liste formatında yaz.";

            string result;

            if (hasApiKey)
            {
                result = await CallOpenAI(apiKey, prompt);
            }
            else
            {
                result = GenerateMockSuggestions(prompt, goal);
                ViewBag.Error = "API Key bulunamadı, simülasyon modu çalışıyor.";
            }

            ProcessResponse(result, height, weight, photo != null);
            return View();
        }

        private async Task<string> CallOpenAI(string apiKey, string prompt)
        {
            try
            {
                var body = new
                {
                    model = "gpt-4.1-mini",
                    messages = new[]
                    {
                        new { role = "system", content = "You are a fitness expert." },
                        new { role = "user", content = prompt }
                    }
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(body),
                    Encoding.UTF8,
                    "application/json"
                );

                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

                var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
                var json = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(json);
                return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString()
                    ?? "Yanıt alınamadı.";
            }
            catch (Exception ex)
            {
                return "AI isteği başarısız: " + ex.Message;
            }
        }

        private void ProcessResponse(string text, double? height, double? weight, bool photo)
        {
            ViewBag.SuggestionsList = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            ViewBag.HasPhoto = photo;

            if (height.HasValue && weight.HasValue)
            {
                double bmi = weight.Value / Math.Pow(height.Value / 100.0, 2);
                ViewBag.BMI = bmi;
                ViewBag.Height = height.Value;
                ViewBag.Weight = weight.Value;
            }
        }

        private string GenerateMockSuggestions(string prompt, string? goal)
        {
            return @"--- DEMO MODU ---
AI bağlantısı yok. Örnek plan:
• Pazartesi: Göğüs + Kardiyo
• Salı: Sırt + Core
• Çarşamba: Dinlenme
• Perşembe: Omuz + Kol
• Cuma: Bacak + Kalça
• Beslenme: Yüksek protein, şeker azalt";
        }
    }
}
