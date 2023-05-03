namespace ASP_201MVC.Models.Forum
{
    public class ForumIndexModel
    {
        public List<Data.Entity.Section> Sections { get; set; } = null!;

        public Boolean UserCanCreate { get; set; }

        public String? CreateMessage { get; set; }

        public bool? IsMessagePositive { get; set; }

        public ForumSectionFormModel? FormModel { get; set; }
    }
}
