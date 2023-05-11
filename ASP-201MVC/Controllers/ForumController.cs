using ASP_201MVC.Data;
using ASP_201MVC.Models.Forum;
using ASP_201MVC.Services.RandomImg;
using ASP_201MVC.Services.Transliteration;
using ASP_201MVC.Services.Validation;
using Microsoft.AspNetCore.Http;
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
        private readonly ITransliterate _transliterate;
        private readonly IRandomImgName _randomImgService;

        public ForumController(DataContext dataContext, ILogger<ForumController> logger, IValidationService validationService, ITransliterate transliterate, IRandomImgName randomImgService)
        {
            _dataContext = dataContext;
            _logger = logger;
            _validationService = validationService;
            _transliterate = transliterate;
            _randomImgService = randomImgService;
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
                    UrlIdString = s.UrlId ?? s.Id.ToString(),
                    AuthorAvatarUrl = s.Author.Avatar == null ? "/avatars/no-avatar.png" : $"/avatars/{s.Author.Avatar}"
                })
                .ToList()
            };
            if (HttpContext.Session.GetString("CreateSectionMessage") is String message)
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
                HttpContext.Session.Remove("CreateSectionMessage");
                HttpContext.Session.Remove("IsMessagePositive");

            }
            return View(model);
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
                    String trans = _transliterate.transliterate(formModel.Title);
                    String urlId = trans;
                    int n = 2;
                    while (_dataContext.Sections.Where(s => s.UrlId == urlId).Count() > 0)
                    {
                        urlId = $"{trans}{n++}";
                    }

                    _dataContext.Sections.Add(new()
                    {
                        Id = Guid.NewGuid(),
                        Title = formModel.Title,
                        Description = formModel.Description,
                        AuthorId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value), //userId
                        CreatedDt = DateTime.Now,
                        UrlId = urlId
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

        //private static Guid currSectionId = new();
        //public String IdChange(String id)
        //{
        //    if (Guid.TryParse(id, out currSectionId))
        //    {
        //        var currSection = _dataContext.Sections.FirstOrDefault(s => s.Id == currSectionId);
        //        return _transliterate.transliterate(currSection.Title);
        //    }
        //    else
        //    {
        //        return id;
        //    }
        //}

        public ViewResult Sections([FromRoute] String id)
        {
            Guid sectionId;
            try
            {
                sectionId = Guid.Parse(id);
            }
            catch (Exception)
            {
                sectionId = _dataContext.Sections.First(s => s.UrlId == id).Id;
            }
            //if(currSectionId == Guid.Empty)
            //    Response.Redirect(IdChange(id));
            ForumSectionsModel model = new()
            {
                UserCanCreate = HttpContext.User.Identity?.IsAuthenticated == true,
                SectionId = sectionId.ToString(),
                Themes = _dataContext
                .Themes
                .Include(t => t.Author)
                .Where(t => t.DeletedDt == null && t.SectionId == sectionId)
                .Select(t => new ForumThemeViewModel()
                {
                    Title = t.Title,
                    Description = t.Description,
                    CreatedDtString = DateTime.Today == t.CreatedDt.Date ? "Cьогодні " + t.CreatedDt.ToString("HH:mm") : t.CreatedDt.ToString("dd.MM.yyyy HH:mm"),
                    UrlIdString = t.Id.ToString(),
                    SectionId = t.SectionId.ToString(),
                    AvatarUrl = $"/img/logos/{t.ThemeImg}",
                    AuthorName = t.Author.IsNamePublic ? t.Author.Name : t.Author.Login,
                    AuthorAvatarUrl = $"/avatars/{t.Author.Avatar ?? "no-avatar.png"}",
                    AuthorCreatedDt = t.Author.IsDatePublic ? t.Author.RegisterDt.ToString() : "Hidden"
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
            ViewData["Id"] = id;
            return View(model);
        }


        public IActionResult Themes([FromRoute] String id)
        {
            Guid themeId;

            try
            {
                themeId = Guid.Parse(id);
            }
            catch (Exception)
            {
                themeId = Guid.Empty;
            }

            var theme = _dataContext.Themes.Find(themeId);

            if(theme == null)
            {
                return NotFound();
            }

            ForumThemesModel model = new()
            {
                UserCanCreate = HttpContext.User.Identity?.IsAuthenticated == true,
                Title = theme.Title,
                ThemeId = id,
                Topics = _dataContext
                .Topics
                .Include(t => t.Author)
                .Where(t => t.DeletedDt == null && t.ThemeId == themeId)
                .AsEnumerable()
                .Select(t => new ForumTopicViewModel()
                {
                    Title = t.Title,
                    Description =(t.Description.Length > 100 ? t.Description[..100] + "..." : t.Description),
                    UrlIdString = t.Id.ToString(),
                    CreatedDtString = DateTime.Today == t.CreatedDt.Date ? "Cьогодні " + t.CreatedDt.ToString("HH:mm") : t.CreatedDt.ToString("dd.MM.yyyy HH:mm"),
                    AuthorName = t.Author.IsNamePublic ? t.Author.Name : t.Author.Login,
                    AuthorAvatarUrl = $"/avatars/{t.Author.Avatar ?? "no-avatar.png"}",
                    AuthorCreatedDt = t.Author.IsDatePublic ? t.Author.RegisterDt.ToString() : "Hidden"

                })
                .ToList()
            };

            if (HttpContext.Session.GetString("CreateTopicMessage") is String message)
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
                HttpContext.Session.Remove("CreateTopicMessage");
                HttpContext.Session.Remove("IsMessagePositive");

            }

            return View(model);
        }

        public IActionResult Topics([FromRoute] String id)
        {
            Guid topicId;

            try
            {
                topicId = Guid.Parse(id);
            }
            catch (Exception)
            {
                topicId = Guid.Empty;
            }

            var topic = _dataContext.Topics.Find(topicId);

            if (topic == null)
            {
                return NotFound();
            }
            ForumTopicsModel model = new()
            {
                UserCanCreate = HttpContext.User.Identity.IsAuthenticated == true,
                Title = topic.Title,
                Description = topic.Description,
                TopicId = id,
                Posts = _dataContext
                .Posts
                .Include(p => p.Author)
                .Where(p => p.DeletedDt == null && p.TopicId == topicId)
                .Select(p => new ForumPostViewModel
                {
                    Content = p.Content,
                    CreatedDtString = DateTime.Today == p.CreatedDt.Date ? "Cьогодні " + p.CreatedDt.ToString("HH:mm") : p.CreatedDt.ToString("dd.MM.yyyy HH:mm"),
                    AuthorName = p.Author.IsNamePublic ? p.Author.Name : p.Author.Login,
                    AuthorAvatarUrl = $"/avatars/{p.Author.Avatar ?? "no-avatar.png"}",
                }).ToList()
            };
            if (HttpContext.Session.GetString("CreatePostMessage") is String message)
            {
                model.CreateMessage = message;
                model.IsMessagePositive = HttpContext.Session.GetInt32("IsMessagePositive") == 1;
                if (model.IsMessagePositive == false)
                {
                    model.FormModel = new()
                    {
                        Content = HttpContext.Session.GetString("SavedContent")!,
                        ReplyId = HttpContext.Session.GetString("SavedReplyId")!
                    };
                    HttpContext.Session.Remove("SavedContent");
                    HttpContext.Session.Remove("SavedReplyId");
                }
                HttpContext.Session.Remove("CreatePostMessage");
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
                    String savedName;
                    if (formModel.Avatar != null)
                    {
                        savedName = _randomImgService.RandomNameImg(formModel.Avatar.FileName);
                        String folderName = "wwwroot/img/logos/";
                        String path = folderName + savedName;
                        using FileStream fs = new(path, FileMode.Create);
                        formModel.Avatar.CopyTo(fs);
                    }
                    else
                    {
                        savedName = $"section{Random.Shared.Next(0, 9)}.png";
                    }
                    _dataContext.Themes.Add(new()
                    {
                        Id = Guid.NewGuid(),
                        Title = formModel.Title,
                        Description = formModel.Description,
                        ThemeImg = savedName,
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
        public RedirectToActionResult CreateTopic(ForumTopicFormModel formModel)
        {
            _logger.LogInformation("Title: {t}, Description: {d}", formModel.Title, formModel.Description);

            if (!_validationService.Validate(formModel.Title, ValidationTerms.NotEmpty) ||
                !_validationService.Validate(formModel.Description, ValidationTerms.NotEmpty))
            {
                HttpContext.Session.SetString("CreateTopicMessage", "Поля не можуть бути порожніми");
                HttpContext.Session.SetInt32("IsMessagePositive", 0);
                HttpContext.Session.SetString("SavedTitle", formModel.Title ?? String.Empty);
                HttpContext.Session.SetString("SavedDescription", formModel.Description ?? String.Empty);
            }
            else
            {
                try
                {
                    _dataContext.Topics.Add(new()
                    {
                        Id = Guid.NewGuid(),
                        Title = formModel.Title,
                        Description = formModel.Description,
                        AuthorId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value),  //userId
                        CreatedDt = DateTime.Now,
                        ThemeId = Guid.Parse(formModel.ThemeId)
                    });
                    _dataContext.SaveChanges();
                    HttpContext.Session.SetString("CreateTopicMessage", "Додано успішно");
                    HttpContext.Session.SetInt32("IsMessagePositive", 1);

                }
                catch
                {
                    HttpContext.Session.SetString("CreateTopicMessage", "Відмовлено в авторизації");
                    HttpContext.Session.SetInt32("IsMessagePositive", 0);
                }
            }

            return RedirectToAction(nameof(Themes), new {id = formModel.ThemeId});
        }

        [HttpPost]
        public RedirectToActionResult CreatePost(ForumPostFormModel formModel)
        {
            _logger.LogInformation("Content: {c}", formModel.Content);

            if (!_validationService.Validate(formModel.Content, ValidationTerms.NotEmpty))
            {
                HttpContext.Session.SetString("CreatePostMessage", "Поля не можуть бути порожніми");
                HttpContext.Session.SetInt32("IsMessagePositive", 0);
                HttpContext.Session.SetString("SavedContent", formModel.Content ?? String.Empty);
                HttpContext.Session.SetString("SavedReplyId", formModel.ReplyId ?? String.Empty);
            }
            else
            {
                try
                {
                    _dataContext.Posts.Add(new()
                    {
                        Id = Guid.NewGuid(),
                        Content = formModel.Content,
                        ReplyId = String.IsNullOrEmpty(formModel.ReplyId)
                        ? null
                        : Guid.Parse(formModel.ReplyId),
                        AuthorId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value),  //userId
                        CreatedDt = DateTime.Now,
                        TopicId = Guid.Parse(formModel.TopicId)
                    });
                    _dataContext.SaveChanges();
                    HttpContext.Session.SetString("CreatePostMessage", "Додано успішно");
                    HttpContext.Session.SetInt32("IsMessagePositive", 1);

                }
                catch
                {
                    HttpContext.Session.SetString("CreatePostMessage", "Відмовлено в авторизації");
                    HttpContext.Session.SetInt32("IsMessagePositive", 0);
                }
            }

            return RedirectToAction(nameof(Topics), new { id = formModel.TopicId });
        }
    }
}
