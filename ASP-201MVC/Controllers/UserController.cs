using ASP_201MVC.Models.User;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;

namespace ASP_201MVC.Controllers
{
    public class UserController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Registration(Registration registrationModel)
        {
            RegisterValidationResult registerValidation = new();
            ViewData["regModel"] = registrationModel;
            ViewData["regValid"] = registerValidation;
            return View();
        }
        public ActionResult Register(Registration registrationModel)
        {
            bool isModelValid = true;
            RegisterValidationResult registerValidation = new();

            if (String.IsNullOrEmpty(registrationModel.Login))
            {
                registerValidation.LoginMessage = "Логін не може бути порожним";
                isModelValid = false;
            }
            if (String.IsNullOrEmpty(registrationModel.Password))
            {
                registerValidation.PasswordMessage = "Пароль не може бути порожним";
                isModelValid = false;
            }
            if (String.IsNullOrEmpty(registrationModel.RepeatPassword))
            {
                registerValidation.RepeatPasswordMessage = "Повтор пароля не може бути порожним";
                isModelValid = false;
            }
            else if (registrationModel.RepeatPassword != registrationModel.Password)
            {
                registerValidation.RepeatPasswordMessage = "Паролі не співпадають";
                isModelValid = false;
            }
            if (String.IsNullOrEmpty(registrationModel.Email))
            {
                registerValidation.EmailMessage = "Email не може бути порожним";
                isModelValid = false;
            }
            else
            {
                try
                {
                    MailAddress m = new MailAddress(registrationModel.Email);
                }
                catch (FormatException)
                {
                    registerValidation.EmailMessage = "Email введено не корректно";
                    isModelValid = false;
                }
            }
            if (String.IsNullOrEmpty(registrationModel.Name))
            {
                registerValidation.NameMessage = "Name не може бути порожним";
                isModelValid = false;
            }
            ViewData["regModel"] = registrationModel;
            ViewData["regValid"] = registerValidation;

            // спосіб перейти на View з іншою назвою, ніж метод
            return View("Registration");
        }
    }
}
