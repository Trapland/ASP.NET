using ASP_201MVC.Data;
using ASP_201MVC.Data.Entity;
using ASP_201MVC.Models;
using ASP_201MVC.Models.User;
using ASP_201MVC.Services.Email;
using ASP_201MVC.Services.Hash;
using ASP_201MVC.Services.Kdf;
using ASP_201MVC.Services.Random;
using ASP_201MVC.Services.RandomImg;
using ASP_201MVC.Services.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.Extensions.Primitives;
using System.Net.Mail;
using System.Security.Claims;
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
        private readonly IRandomImgName _randomImgService;
        private readonly IValidationService _validationService;
        private readonly IEmailService _emailService;


        public UserController(IHashService hashService, ILogger<UserController> logger, DataContext dataContext, IRandomService randomService, IKdfService kdfService, IRandomImgName randomImgService, IValidationService validationService, IEmailService emailService = null)
        {
            _hashService = hashService;
            _logger = logger;
            _dataContext = dataContext;
            _randomService = randomService;
            _kdfService = kdfService;
            _randomImgService = randomImgService;
            _validationService = validationService;
            _emailService = emailService;
        }

        public ActionResult Index()
        {
            return View();
        }

        public IActionResult Profile([FromRoute] String id)
        {
            _logger.LogInformation(id);
            User? user = _dataContext.Users.FirstOrDefault(u => u.Login == id);
            if (user is not null)
            {
                ProfileModel model = new(user);
                if (HttpContext.User.Identity is not null &&
                    HttpContext.User.Identity.IsAuthenticated)
                {
                    String userLogin = HttpContext.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
                    if (userLogin == user.Login)
                    {
                        model.isPersonal = true;
                    }
                }
                return View(model);

            }
            else
            {
                return NotFound();
            }
            /*Особиста сторінка / Профиль
             * 1. Чи буде ця сторінка доступна іншим користувачам
             * Так, інші користувачі можуть переглядати профіль інших користувачів
             * але тільки ті дані, що дозволив власник.
             * 2. Як має формуватись адреса /User/Profile/????
             * a) Id
             * б) Login
             */

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
            if (_dataContext.Users.Any(u => u.Login == registrationModel.Login))
            {
                registerValidation.LoginMessage = "Логін вже використовується";
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
            if (!_validationService.Validate(registrationModel.Email, ValidationTerms.NotEmpty))
            {
                registerValidation.EmailMessage = "Email не може бути порожним";
                isModelValid = false;
            }
            else if (!_validationService.Validate(registrationModel.Email, ValidationTerms.Email))
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
            String savedName = "";
            if (registrationModel.Avatar == null)
            {
                savedName = "";
            }
            else
            {
                savedName = _randomImgService.RandomNameImg(registrationModel.Avatar.FileName);
            }
            if (registrationModel.Avatar is not null)
            {
                if (registrationModel.Avatar.Length > 1024)
                {
                    String folderName = "wwwroot/avatars/";
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

                var emailConfirmToken = _GenerateEmailConfirmToken(user);
                _SendConfirmEmail(user, emailConfirmToken);
                _SendCongratsEmail(user);
                _dataContext.SaveChangesAsync();
                // Формуємо посилання: схема://домен(хост)/User/ConfirmToken?token=...
                
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

        public RedirectToActionResult Logout()
        {
            HttpContext.Session.Remove("authUserId");
            return RedirectToAction("Index", "Home");
            /* Redirect та інші питання з перенаправлення
             * Browser            Server
             * GET /home --------> (routing)->Home::Index()->View()
             *   page    <-------- 200 OK <!doctype html>...
             *   
             * <a Logout> -------> User::Logout()->Redirect(...) 
             *   follow  <------- 302 (Redirect) Location: /home
             * GET /home --------> (routing)->Home::Index()->View()
             *   page    <-------- 200 OK <!doctype html>...           
             */
        }

        //[HttpPost]
        //public String Logout()
        //{
        //    HttpContext.Session.Remove("authUserId");
        //    return "OK";
        //}
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
                    HttpContext.Session.SetString("authUserId", user.Id.ToString());
                    return "OK";
                }

            }

            return "Авторизацію відхилено";
        }

        [HttpPut]
        public IActionResult Update([FromBody] UpdateRequestModel model)
        {
            UpdateResponseModel responseModel = new();
            try
            {
                if (model is null) throw new Exception("No or empty data");
                if (HttpContext.User.Identity?.IsAuthenticated == false)
                {
                    throw new Exception("UnAuthenticated");
                }
                User? user = _dataContext.Users.Find(
                    Guid.Parse(
                        HttpContext.User.Claims.First(
                            c => c.Type == ClaimTypes.Sid).Value));
                if (user is null) throw new Exception("UnAuthorized");
                switch (model.Field)
                {
                    case "realname":
                        if (_validationService.Validate(model.Value, ValidationTerms.RealName))
                        {
                            user.Name = model.Value;
                            _dataContext.SaveChanges();
                        }
                        else
                        {
                            throw new Exception(
                                $"Validation error: field '{model.Field}' with value '{model.Value}'");
                        }
                        break;
                    case "email":
                        if (_validationService.Validate(model.Value, ValidationTerms.Email))
                        {
                            user.Email = model.Value;
                            _dataContext.SaveChanges();
                        }
                        else
                        {
                            throw new Exception(
                                $"Validation error: field '{model.Field}' with value '{model.Value}'");
                        }
                        break;
                    default:
                        throw new Exception($"Invalid Field attribute '{model.Field}'");
                }
                responseModel.Status = "OK";
                responseModel.Data = $"Field '{model.Field}' updated by value '{model.Value}'";
            }
            catch (Exception ex)
            {
                responseModel.Status = "Error";
                responseModel.Data = ex.Message;
            }
            return Json(responseModel);
        }

        [HttpPost]
        public JsonResult ConfirmEmail([FromBody] string emailCode)
        {
            StatusDataModel model = new();
            if (String.IsNullOrEmpty(emailCode))
            {
                model.Status = "406";
                model.Data = "Empty code not acceptable";
            }
            else if (HttpContext.User.Identity?.IsAuthenticated == false)
            {
                model.Status = "401";
                model.Data = "UnAuthenticated";

            }
            else
            {
                User? user = _dataContext.Users.Find(
                    Guid.Parse(
                        HttpContext.User.Claims.First(
                            c => c.Type == ClaimTypes.Sid).Value));
                if (user is null)
                {
                    model.Status = "403";
                    model.Data = "Forbidden (UnAuthorized)";
                }
                else if (user.EmailCode is null)
                {
                    model.Status = "208";
                    model.Data = "Already confirmed";
                }
                else if (user.EmailCode != emailCode)
                {
                    model.Status = "406";
                    model.Data = "Code not Accepted";
                }
                else
                {
                    user.EmailCode = null;
                    _dataContext.SaveChanges();
                    model.Status = "200";
                    model.Data = "OK";
                }
            }
            return Json(model);
        }

        [HttpGet]
        public ViewResult ConfirmToken([FromQuery] String token)
        {
            try
            {
                //шукаємо токен за отриманим ID
                var confirmToken = _dataContext.EmailConfirmTokens.Find(Guid.Parse(token)) ?? throw new Exception();
                //шукаємо користувача за UserId
                var user = _dataContext.Users.Find(confirmToken.UserId) ?? throw new Exception();
                // перевіряємо збіг пошт
                if (user.Email != confirmToken.UserEmail)
                {
                    throw new Exception();
                } // оновлюємо дані
                user.EmailCode = null; // Пошта підтверджена
                confirmToken.Used += 1; // ведемо рахунок використання токена
                _dataContext.SaveChangesAsync();
                ViewData["result"] = "Вітаємо, пошта успішно підтверджена";
            }
            catch
            {
                ViewData["result"] = "Перевірка не пройдена, не змінюйте посилання з листа";
            }
            return View();
        }

        [HttpPatch]
        public String ResendConfirmEmail()
        {
            if (HttpContext.User.Identity?.IsAuthenticated == false)
            {
                return "Unauthenticated";
            }
            try
            {
                User? user = _dataContext.Users.Find
                    (Guid.Parse(HttpContext.User.Claims.First
                    (c => c.Type == ClaimTypes.Sid).Value))
                    ?? throw new Exception();

                var emailConfirmToken = _GenerateEmailConfirmToken(user);
                user.EmailCode = _randomService.ConfirmCode(6);
                _dataContext.SaveChangesAsync(); 
                if(_SendConfirmEmail(user, emailConfirmToken))
                {
                    return "OK";
                }
                else
                {
                    return "Send error";
                }
            }
            catch
            {
                return "Unauthorized";
            }

        }

        private EmailConfirmToken _GenerateEmailConfirmToken(Data.Entity.User user)
        {
            Data.Entity.EmailConfirmToken emailConfirmToken = new()
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                UserEmail = user.Email,
                Moment = DateTime.Now,
                Used = 0
            };
            _dataContext.EmailConfirmTokens.Add(emailConfirmToken);
            return emailConfirmToken;
        }

        private bool _SendConfirmEmail(User user, EmailConfirmToken emailConfirmToken)
        {
            String confirmLink = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}/User/ConfirmToken?token={emailConfirmToken.Id}";

            return _emailService.Send("confirm_email",
                new Models.Email.ConfirmEmailModel
                {
                    Email = user.Email,
                    RealName = user.Name,
                    EmailCode = user.EmailCode!,
                    ConfirmLink = confirmLink
                });
        }
        private bool _SendCongratsEmail(User user)
        {
            return _emailService.Send("congrats",
                new Models.Email.ConfirmEmailModel
                {
                    Email = user.Email,
                    RealName = user.Name,
                    EmailCode = user.EmailCode!,
                    ConfirmLink = null!
                });
        }
    }

}
