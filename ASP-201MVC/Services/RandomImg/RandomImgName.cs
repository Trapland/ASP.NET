using ASP_201MVC.Services.Hash;

namespace ASP_201MVC.Services.RandomImg
{
    public class RandomImgName : IRandomImgName
    {
        private readonly IHashService _hashService;

        public RandomImgName(IHashService hashService)
        {
            _hashService = hashService;
        }

        public String RandomNameImg(String FileName)
        {
            String savedName = null!;
            String ext = Path.GetExtension(FileName);
            savedName = _hashService.Hash(FileName + DateTime.Now + System.Random.Shared.Next())[..16] + ext;
            String folderName = "wwwroot/avatars/";
            IEnumerable<string> files = Directory.EnumerateFiles(folderName);
            String filePath = folderName + savedName;
            while (files.Contains(filePath))
            {
                savedName = _hashService.Hash(FileName + DateTime.Now + System.Random.Shared.Next())[..16] + ext;
                filePath = folderName + savedName;
            }
            return savedName;
        }
    }
}
