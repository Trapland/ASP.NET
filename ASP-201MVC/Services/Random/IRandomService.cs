namespace ASP_201MVC.Services.Random
{
    public interface IRandomService
    {
        String ConfirmCode(int length);

        String RandomString(int length);
    }
}
