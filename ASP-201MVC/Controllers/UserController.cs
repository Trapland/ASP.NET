using ASP_201MVC.Data;
using ASP_201MVC.Data.Entity;
using ASP_201MVC.Models.User;
using ASP_201MVC.Services.Hash;
using ASP_201MVC.Services.Kdf;
using ASP_201MVC.Services.Random;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.Extensions.Primitives;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace ASP_201MVC.Controllers
{
    public class UserController : Controller
    {
        private readonly IHashService _hashService;
        private readonly ILogger<UserController> _logger;
        private readonly DataContext _dataContext;
        private readonly IRandomService _randomService;
        private readonly IKdfService _kdfService;

        public UserController(IHashService hashService, ILogger<UserController> logger, DataContext dataContext, IRandomService randomService, IKdfService kdfService)
        {
            _hashService = hashService;
            _logger = logger;
            _dataContext = dataContext;
            _randomService = randomService;
            _kdfService = kdfService;
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
                if (!Regex.IsMatch(registrationModel.Email, emailRegex))
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

            String savedName = null!;
            if (registrationModel.Avatar is not null)
            {
                if (registrationModel.Avatar.Length > 1024)
                {
                    String ext = Path.GetExtension(registrationModel.Avatar.FileName);
                    savedName = _hashService.Hash(registrationModel.Avatar.FileName + DateTime.Now + Random.Shared.Next())[..16] + ext;
                    String folderName = "wwwroot/avatars/";
                    IEnumerable<string> files = Directory.EnumerateFiles(folderName);
                    String FileName = folderName + savedName;
                    while (true)
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
            if (isModelValid)
            {
                String salt = _randomService.RandomString(16);
                User user = new()
                {
                    Id = Guid.NewGuid(),
                    Login = registrationModel.Login,
                    Name = registrationModel.Name,
                    Email = registrationModel.Email,
                    EmailCode = _randomService.ConfirmCode(6),
                    PasswordSalt = salt,
                    PasswordHash = _kdfService.GetDerivedKey(registrationModel.Password, salt),
                    Avatar = savedName,
                    RegisterDt = DateTime.Now,
                    LastEnterDt = null
                };
                _dataContext.Users.Add(user);
                _dataContext.SaveChangesAsync();
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
        [HttpPost]
        public String AuthUser()
        {
            // альтернатертивний до моделей спосіб отримання параметрів запиту
            StringValues loginValues = Request.Form["user-login"];
            // колекція loginValues формується при будь-якому ключі, але для
            // неправильних (відсутніх) ключів вона порожня
            if (loginValues.Count == 0)
            {
                // немає логіну у складі полів
                return "Missed required parameter: user-login";
            }
            String login = loginValues[0] ?? "";

            StringValues passwordValues = Request.Form["user-password"];
            // колекція loginValues формується при будь-якому ключі, але для
            // неправильних (відсутніх) ключів вона порожня
            if (passwordValues.Count == 0)
            {
                // немає логіну у складі полів
                return "Missed required parameter: user-password";
            }
            String password = passwordValues[0] ?? "";

            //шукаємо користувача за логіном
            User? user = _dataContext.Users.Where(u => u.Login == login).FirstOrDefault();
            if (user is not null)
            {
                // якщо знайшли - перевіряємо пароль(derived key)
                if (user.PasswordHash == _kdfService.GetDerivedKey(password, user.PasswordSalt))
                {
                    //дані перевірені - користувач автентифікований
                    HttpContext.Session.SetString("authUserId",user.Id.ToString());
                    return "OK";
                }

            }

            return "Авторизацію відхилено";
        }
    }
}
