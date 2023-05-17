﻿namespace ASP_201MVC.Models.Forum
{
    public class ForumSectionViewModel
    {
        public String Title { get; set; } = null!;

        public String Description { get; set; } = null!;

        public String LogoUrl { get; set; } = null!;

        public String CreatedDtString { get; set; } = null!;

        public String UrlIdString { get; set; } = null!;

        public String Id { get; set; } = null!;



        // Another data

        public String AuthorName { get; set; } = null!;

        public String AuthorAvatarUrl { get; set; }

        // Rating data

        public int LikesCount { get; set; }

        public int DislikesCount { get; set; }

        public int? GivenRating { get; set; }
    }
}
