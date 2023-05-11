﻿namespace ASP_201MVC.Models.Forum
{
    public class ForumPostViewModel
    {
        public String Content { get; set; } = null!;

        public String CreatedDtString { get; set; } = null!;


        public String AuthorName { get; set; } = null!;

        public String AuthorAvatarUrl { get; set; }
    }
}
