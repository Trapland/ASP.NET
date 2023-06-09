﻿namespace ASP_201MVC.Models.Forum
{
    public class ForumTopicViewModel
    {
        public String Title { get; set; } = null!;

        public String Description { get; set; } = null!;

        public String CreatedDtString { get; set; } = null!;

        public String UrlIdString { get; set; } = null!;


        // Another data

        public String AuthorName { get; set; } = null!;

        public String AuthorAvatarUrl { get; set; }

        public String AuthorCreatedDt { get; set; } = null!;
    }
}
