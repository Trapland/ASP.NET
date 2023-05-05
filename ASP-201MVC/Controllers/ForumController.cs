using ASP_201MVC.Data;
using ASP_201MVC.Models.Forum;
using ASP_201MVC.Services.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Metrics;
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
        private int _counter = 0;

        private int Counter { get => _counter++; set => _counter = value; }

        public IActionResult Index()
        {
            ForumIndexModel model = new()
            {
                UserCanCreate = HttpContext.User.Identity?.IsAuthenticated == true,
                Sections = _dataContext.Sections
                .Include(s => s.Author)
                .Where(s => s.DeletedDt == null)
                .OrderByDescending(s => s.CreatedDt)
                .AsEnumerable() // IQueriable -> IEnumerable
                .Select(s => new ForumSectionViewModel()
                {
                    Title = s.Title,
                    Description = s.Description,
                    LogoUrl = $"/img/logos/section{Counter}.png",
                    AuthorName = s.Author.IsNamePublic ? s.Author.Name : s.Author.Login,
                    CreatedDtString = DateTime.Today == s.CreatedDt.Date ? "Cьогодні " + s.CreatedDt.ToString("HH:mm") : s.CreatedDt.ToString("dd.MM.yyyy HH:mm"),
                    UrlIdString = s.Id.ToString(),
                    AuthorAvatarUrl = s.Author.Avatar == null ? "/avatars/no-avatar.png" : $"/avatars/{s.Author.Avatar}"
                })
                .ToList()
            };
            if (HttpContext.Session.GetString("CreateSectionMessage") is String message)
            {
                model.CreateMessage = message;
                model.IsMessagePositive = HttpContext.Session.GetInt32("IsMessagePositive") == 1;
                if (/*model.IsMessagePositive == false*/ true)
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

        public ViewResult Sections([FromRoute] String id)
        {
            ForumSectionsModel model = new()
            {
                UserCanCreate = HttpContext.User.Identity?.IsAuthenticated == true,
                SectionId = id,
                Themes = _dataContext
                .Themes
                .Where(t => t.DeletedDt == null && t.SectionId == Guid.Parse(id))
                .Select(t => new ForumThemeViewModel()
                {
                    Title = t.Title,
                    Description = t.Description,
                    CreatedDtString = DateTime.Today == t.CreatedDt.Date ? "Cьогодні " + t.CreatedDt.ToString("HH:mm") : t.CreatedDt.ToString("dd.MM.yyyy HH:mm"),
                    UrlIdString = t.Id.ToString(),
                    SectionId = t.SectionId.ToString(),
                })
                .ToList()
            };
            if (HttpContext.Session.GetString("CreateThemeMessage") is String message)
            {
                model.CreateMessage = message;
                model.IsMessagePositive = HttpContext.Session.GetInt32("IsMessagePositive") == 1;
                if (model.IsMessagePositive == false)
                {
                    model.FormModel = new()
                    {
                        Title = HttpContext.Session.GetString("SavedTitle")!,
                        Description = HttpContext.Session.GetString("SavedDescription")!
                    };
                    HttpContext.Session.Remove("SavedTitle");
                    HttpContext.Session.Remove("SavedDescription");
                }
                HttpContext.Session.Remove("CreateThemeMessage");
                HttpContext.Session.Remove("IsMessagePositive");

            }

            return View(model);
        }
        [HttpPost]
        public RedirectToActionResult CreateTheme(ForumThemeFormModel formModel)
        {
            _logger.LogInformation("Title: {t}, Description: {d}", formModel.Title, formModel.Description);
            if (!_validationService.Validate(formModel.Title, ValidationTerms.NotEmpty) ||
                !_validationService.Validate(formModel.Description, ValidationTerms.NotEmpty))
            {
                HttpContext.Session.SetString("CreateThemeMessage", "Поля не можуть бути порожніми");
                HttpContext.Session.SetInt32("IsMessagePositive", 0);
                HttpContext.Session.SetString("SavedTitle", formModel.Title ?? String.Empty);
                HttpContext.Session.SetString("SavedDescription", formModel.Description ?? String.Empty);
            }
            else
            {
                try
                {
                    _dataContext.Themes.Add(new()
                    {
                        Id = Guid.NewGuid(),
                        Title = formModel.Title,
                        Description = formModel.Description,
                        AuthorId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value),  //userId
                        CreatedDt = DateTime.Now,
                        SectionId = Guid.Parse(formModel.SectionId)
                    });
                    _dataContext.SaveChanges();
                    HttpContext.Session.SetString("CreateThemeMessage", "Додано успішно");
                    HttpContext.Session.SetInt32("IsMessagePositive", 1);

                }
                catch
                {
                    HttpContext.Session.SetString("CreateThemeMessage", "Відмовлено в авторизації");
                    HttpContext.Session.SetInt32("IsMessagePositive", 0);
                }
            }
            return RedirectToAction(nameof(Sections), new { id = formModel.SectionId });
        }

        [HttpPost]
        public RedirectToActionResult CreateSection(ForumSectionFormModel formModel)
        {
            _logger.LogInformation("Title: {t}, Description: {d}", formModel.Title, formModel.Description);
            if (!_validationService.Validate(formModel.Title, ValidationTerms.NotEmpty) ||
                !_validationService.Validate(formModel.Description, ValidationTerms.NotEmpty))
            {
                HttpContext.Session.SetString("CreateSectionMessage", "Поля не можуть бути порожніми");
                HttpContext.Session.SetInt32("IsMessagePositive", 0);
                HttpContext.Session.SetString("SavedTitle", formModel.Title ?? String.Empty);
                HttpContext.Session.SetString("SavedDescription", formModel.Description ?? String.Empty);
            }
            else
            {
                try
                {
                    _dataContext.Sections.Add(new()
                    {
                        Id = Guid.NewGuid(),
                        Title = formModel.Title,
                        Description = formModel.Description,
                        AuthorId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value), //userId
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
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
