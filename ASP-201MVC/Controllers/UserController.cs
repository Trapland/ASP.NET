using ASP_201MVC.Models.User;
using ASP_201MVC.Services.Hash;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace ASP_201MVC.Controllers
{
    public class UserController : Controller
    {
        private readonly IHashService _hashService;
        private readonly ILogger<UserController> _logger;

        public UserController(IHashService hashService, ILogger<UserController> logger)
        {
            _hashService = hashService;
            _logger = logger;
        }

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
                    String ext = Path.GetExtension(registrationModel.Avatar.FileName);
                    String savedName = _hashService.Hash(registrationModel.Avatar.FileName + DateTime.Now + Random.Shared.Next())[..16] + ext;
                    String folderName = "wwwroot/avatars/";
                    IEnumerable<string> files = Directory.EnumerateFiles(folderName);
                    String FileName = folderName + savedName;
                    while(true)
                    {
                        if (files.Contains(FileName))
                        {
                            savedName = _hashService.Hash(registrationModel.Avatar.FileName + DateTime.Now + Random.Shared.Next())[..16] + ext;
                            FileName = folderName + savedName;
                            continue;
                        }
                        break;
                    }
                    String path = folderName + savedName;
                    using FileStream fs = new(path, FileMode.Create);
                    registrationModel.Avatar.CopyTo(fs);
                    ViewData["savedName"] = savedName;
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
