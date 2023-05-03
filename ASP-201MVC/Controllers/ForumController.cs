using ASP_201MVC.Data;
using ASP_201MVC.Models.Forum;
using ASP_201MVC.Services.Validation;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ASP_201MVC.Controllers
{
    public class ForumController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<ForumController> _logger;
        private readonly IValidationService _validationService;

        public ForumController(DataContext dataContext, ILogger<ForumController> logger, IValidationService validationService)
        {
            _dataContext = dataContext;
            _logger = logger;
            _validationService = validationService;
        }

        public IActionResult Index()
        {
            ForumIndexModel model = new ForumIndexModel()
            {
                UserCanCreate = HttpContext.User.Identity.IsAuthenticated == true,
                Sections = _dataContext.Sections.Where(s => s.DeletedDt == null).ToList()
            };
            if (HttpContext.Session.GetString("CreateSectionMessage") is String message)
            {
                model.CreateMessage = message;
                model.IsMessagePositive = HttpContext.Session.GetInt32("IsMessagePositive") == 1;
                if(model.IsMessagePositive == false)
                {
                    model.FormModel = new()
                    {
                        Title = HttpContext.Session.GetString("SavedTitle")!,
                        Description = HttpContext.Session.GetString("SavedDescription")!
                    };
                    HttpContext.Session.Remove("SavedTitle");
                    HttpContext.Session.Remove("SavedDescription");
                }
                HttpContext.Session.Remove("CreateSectionMessage");
                HttpContext.Session.Remove("IsMessagePositive");

            }
            return View(model);
        }

        [HttpPost]
        public RedirectToActionResult CreateSection(ForumSectionFormModel formModel)
        {
            _logger.LogInformation("Title: {t}, Description: {d}", formModel.Title, formModel.Description);
            bool isMessagePositive = false;
            if(!_validationService.Validate(formModel.Title, ValidationTerms.NotEmpty))
            {
                HttpContext.Session.SetString("CreateSectionMessage", "Назва не може бути порожньою");
                HttpContext.Session.SetInt32("IsMessagePositive", 0);
                HttpContext.Session.SetString("SavedTitle", formModel.Title ?? String.Empty);
                HttpContext.Session.SetString("SavedDescription", formModel.Description ?? String.Empty);
            }
            else if (!_validationService.Validate(formModel.Description, ValidationTerms.NotEmpty))
            {
                HttpContext.Session.SetString("CreateSectionMessage", "Опис не може бути порожним");
                HttpContext.Session.SetInt32("IsMessagePositive", 0);
                HttpContext.Session.SetString("SavedTitle", formModel.Title ?? String.Empty);
                HttpContext.Session.SetString("SavedDescription", formModel.Description ?? String.Empty);
            }
            else
            {
                Guid userId;
                try
                {
                    userId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value);
                    _dataContext.Sections.Add(new()
                    {
                        Id = Guid.NewGuid(),
                        Title = formModel.Title,
                        Description = formModel.Description,
                        CreatedDt = DateTime.Now
                    });
                    _dataContext.SaveChanges();
                    HttpContext.Session.SetString("CreateSectionMessage", "Додано успішно");
                    HttpContext.Session.SetInt32("IsMessagePositive", 1);

                }
                catch
                {
                    HttpContext.Session.SetString("CreateSectionMessage", "Відмовлено в авторизації");
                    HttpContext.Session.SetInt32("IsMessagePositive", 0);
                    HttpContext.Session.SetString("SavedTitle", formModel.Title ?? String.Empty);
                    HttpContext.Session.SetString("SavedDescription", formModel.Description ?? String.Empty);
                }
            }
                return RedirectToAction(nameof(Index));
        }
    }
}
