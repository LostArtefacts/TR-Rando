using System.Security.Cryptography;
using System.Text;
using TRModelTransporter.Model.Textures;

namespace TRModelTransporter.Utilities;

public class TRTextureClassifier : ITextureClassifier
{
    private readonly string _levelClassification;

    public TRTextureClassifier(string levelPath)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(Path.GetFileNameWithoutExtension(levelPath).ToUpper()));
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            _levelClassification = sb.ToString();
        }
    }

    public string GetClassification()
    {
        return _levelClassification;
    }
}
