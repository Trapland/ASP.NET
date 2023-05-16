﻿namespace ASP_201MVC.Data.Entity
{
    public class Section
    {
        public Guid Id { get; set; }

        public String Title { get; set; } = null!;

        public String Description { get; set; } = null!;

        public Guid AuthorId { get; set; }

        public DateTime CreatedDt { get; set; }

        public DateTime? DeletedDt { get; set; }

        public String? UrlId { get; set; }

        public User Author { get; set; } = null!;

        public List<Rate> RateList { get; set; } = null!;


    }
}
