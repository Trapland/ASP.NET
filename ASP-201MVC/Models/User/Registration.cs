namespace ASP_201MVC.Models.User
{
    public class Registration
    {
        public string Login { get; set; } = null!;

        public string Password { get; set; } = null!;

        public string RepeatPassword { get; set; } = null!;

        public string Name { get; set; } = null!;

        public string Email { get; set; } = null!;

        public Boolean IsAgree { get; set; }
    }
}
