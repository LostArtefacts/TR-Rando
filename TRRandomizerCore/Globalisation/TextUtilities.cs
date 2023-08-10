using System.Globalization;
using System.Text;

namespace TRRandomizerCore.Globalisation;

public static class TextUtilities
{
    public static string Normalise(char c)
    {
        return Normalise(c.ToString());
    }

    public static string Normalise(string s)
    {
        return new string(s.Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .ToArray());
    }
}
