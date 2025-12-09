using Microsoft.AspNetCore.Mvc;

namespace FitnessCenterApp.Controllers
{
    public class UsersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
