namespace ASP_201MVC.Services.Email
{
    public interface IEmailService
    {
        bool Send(string mailTemplate,object model);
    }
}
