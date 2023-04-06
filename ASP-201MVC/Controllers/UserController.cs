using ASP_201MVC.Models.User;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace ASP_201MVC.Controllers
{
    public class UserController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult RegistrationBootstrap(/*Registration registrationModel*/)
        {
            //RegisterValidationResult registerValidation = new();
            //ViewData["regModel"] = registrationModel;
            //ViewData["RegisterValidationResult"] = registerValidation;
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
                string emailRegex = @"^[\w\.%+-]+@([\w-]+\.)+(\w{2,})$";
                if(!Regex.IsMatch(registrationModel.Email, emailRegex))
                {
                    registerValidation.EmailMessage = "Email введено не корректно";
                    isModelValid = false;
                }
                //try
                //{
                //    MailAddress m = new MailAddress(registrationModel.Email);
                //}
                //catch (FormatException)
                //{
                //    registerValidation.EmailMessage = "Email введено не корректно";
                //    isModelValid = false;
                //}
            }
            if (String.IsNullOrEmpty(registrationModel.Name))
            {
                registerValidation.NameMessage = "Name не може бути порожним";
                isModelValid = false;
            }
            if (registrationModel.IsAgree == false)
            {
                registerValidation.IsAgreeMessage = "Для реєстрації слід прийняти правила сайту";
                isModelValid = false;
            }
            
            if(registrationModel.Avatar is not null)
            {
                if (registrationModel.Avatar.Length > 1024)
                {
                    String path = "wwwroot/avatars/" + registrationModel.Avatar.FileName;
                    using FileStream fs = new(path, FileMode.Create);
                    registrationModel.Avatar.CopyTo(fs);
                }
                else
                {
                    isModelValid = false;
                    registerValidation.AvatarMessage = "Avatar size is too small";
                }
            }
            //якщо всі перевірки пройдено то переходимо на нову сторінку
            if(isModelValid)
            {
                return View(registrationModel);
            }
            else
            {
                ViewData["regModel"] = registrationModel;
                ViewData["RegisterValidationResult"] = registerValidation;

                // спосіб перейти на View з іншою назвою, ніж метод
                return View("RegistrationBootstrap");
            }

        }
    }
}
