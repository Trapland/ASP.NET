namespace ASP_201MVC.Services.Random
{
    public class RandomServiceV2 : IRandomService
    {
        public string ConfirmCode(int length)
        {
            String Code = "";
            for (int i = 0; i < 6; i++)
            {
                if (System.Random.Shared.Next(10) > 2)
                {
                    Code += Convert.ToChar(System.Random.Shared.Next(97, 122));
                }
                else
                {
                    Code += System.Random.Shared.Next(10);
                }
            }
            return Code;
        }

        public string RandomString(int length)
        {
            throw new NotImplementedException();
        }
    }
}
