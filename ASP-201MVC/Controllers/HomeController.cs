using ASP_201MVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ASP_201MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Intro()
        {
            return View();
        }
        public IActionResult Scheme()
        {
            ViewBag.bagdata = "Data in ViewBag";     // Способи передачі даних
            ViewData["data"] = "Data in ViewData";   // до представлення

            return View();
        }
        public IActionResult Razor()
        {
            return View();
        }
        public IActionResult URL()
        {
            ViewBag.bagdata = "Data in ViewBag";     // Способи передачі даних
            ViewData["data"] = "Data in ViewData";   // до представлення
            return View();
        }
        public IActionResult PassData()
        {
            Models.Home.PassDataModel model = new()
            {
                Header = "Моделі",
                Title = "Моделі передачі даних",
                Products = new()
                {
                    new() {Name = "Кабель",     Price = 20,      },
                    new() {Name = "Миша",       Price = 420,     },
                    new() {Name = "Серветки",   Price = 14,      },
                    new() {Name = "Наліпка",    Price = 45,      },
                    new() {Name = "Клавіатура", Price = 2100.22, },
                    new() {Name = "Монітор",    Price = 54320,   },
                    new() {Name = "Болт",       Price = 7.50,    }
                }
            };
            return View(model);
        }
        public IActionResult DisplayTemplates()
        {
            Models.Home.PassDataModel model = new()
            {
                Header = "Шаблони",
                Title = "Шаблони відображення даних",
                Products = new()
                {
                    new() {Name = "Кабель",     Price = 20,       Src = "/img/img1.png" },
                    new() {Name = "Миша",       Price = 420,      Src = "/img/img9.jpg" },
                    new() {Name = "Серветки",   Price = 14,       Src = "/img/img8.jpg" },
                    new() {Name = "Наліпка",    Price = 45,       Src = "/img/img4.jpg" },
                    new() {Name = "Клавіатура", Price = 2100.22,  Src = "/img/img5.jpg" },
                    new() {Name = "Монітор",    Price = 54320,     },
                    new() {Name = "Болт",       Price = 7.50,      }
                }
            };
            return View(model);
        }

        public IActionResult TagHelpers()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return null!;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}