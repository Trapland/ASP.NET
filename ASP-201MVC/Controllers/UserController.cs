using ASP_201MVC.Models.User;
using Microsoft.AspNetCore.Mvc;

namespace ASP_201MVC.Controllers
{
    public class UserController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Registration()
        {
            return View();
        }
        public ActionResult Register(Registration registrationModel)
        {
            ViewData["regModel"] = registrationModel;

            // спосіб перейти на View з іншою назвою, ніж метод
            return View("Registration");
        }
    }
}
