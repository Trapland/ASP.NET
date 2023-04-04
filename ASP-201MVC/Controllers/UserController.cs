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
    }
}
