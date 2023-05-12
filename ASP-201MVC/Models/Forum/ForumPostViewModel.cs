namespace ASP_201MVC.Models.Forum
{
    public class ForumPostViewModel
    {

        public String IdString { get; set; }

        public Boolean UserCanCreate { get; set; }

        public String Content { get; set; } = null!;

        public String CreatedDtString { get; set; } = null!;

        public ForumPostViewModel? Reply { get; set; }

        public String AuthorName { get; set; } = null!;

        public String AuthorAvatarUrl { get; set; }
    }
}
