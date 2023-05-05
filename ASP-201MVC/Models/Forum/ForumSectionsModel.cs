namespace ASP_201MVC.Models.Forum
{
    public class ForumSectionsModel
    {
        public Boolean UserCanCreate { get; set; }

        public String SectionId { get; set; } = null!;

        public List<ForumThemeViewModel> Themes { get; set; } = null!;

        public String? CreateMessage { get; set; }

        public bool? IsMessagePositive { get; set; }

        public ForumThemeFormModel FormModel { get; set; } = null!;
    }
}
