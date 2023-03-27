using System.Globalization;
using System.Text;

namespace TRRandomizerCore.Globalisation
{
    public static class TextUtilities
    {
        public static string Normalise(string s)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in s)
            {
                sb.Append(Normalise(c));
            }
            return sb.ToString();
        }

        public static string Normalise(char c)
        {
            StringBuilder sb = new StringBuilder();
            string data = c.ToString().Normalize(NormalizationForm.FormD);
            foreach (char d in data)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(d) != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(d);
                }
            }
            return sb.ToString();
        }
    }
}
