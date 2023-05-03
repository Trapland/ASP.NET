using Microsoft.AspNetCore.Mvc;

namespace ASP_201MVC.Models.Forum
{
    public class ForumSectionFormModel
    {
        [FromForm(Name = "section-title")]
        public string Title { get; set; } = null!;
        [FromForm(Name = "section-description")]
        public string Description { get; set; } = null!;
    }
}
