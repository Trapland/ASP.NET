namespace ASP_201MVC.Data.Entity
{
    public class User
    {
        public Guid Id { get; set; }

        public String Name { get; set; } = null!;

        public String Login { get; set; } = null!;

        public String Email { get; set; } = null!;

        public String PasswordHash { get; set; } = null!;

        public String PasswordSalt { get; set; } = null!;

        public String? Avatar { get; set; }

        public DateTime RegisterDt { get; set; } // момент реєстрації

        public DateTime? LastEnterDt { get; set; } // останній вхід до сайту

        public String? EmailCode { get; set; }

        // Додано 2023-04-19 робота з Profile
        public Boolean IsEmailPublic { get; set; } = false;

        public Boolean IsNamePublic { get; set; } = false;

        public Boolean IsDatePublic { get; set; } = false;


    }
}
