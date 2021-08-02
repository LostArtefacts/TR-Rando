using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace TR2RandomizerCore.Globalisation
{
    public class GameStrings
    {
        public GlobalStrings[] GlobalStrings { get; set; }
        public Dictionary<string, LevelStrings> LevelStrings { get; set; }

        // >  => +
        // =  => ß
        // )e => é
        // $e => è
        // (e => ê
        // ~e => ë
        public static string Encode(string text)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in text)
            {
                char d = char.ToUpper(c);
                switch (d)
                {
                    case '+':
                        sb.Append('>');
                        break;
                    case 'ß':
                        sb.Append('=');
                        break;
                    case 'À':
                    case 'È':
                    case 'Ì':
                    case 'Ò':
                    case 'Ù':
                        sb.Append("$").Append(Normalise(c));
                        break;
                    case 'Á':
                    case 'Ć':
                    case 'É':
                    case 'Í':
                    case 'Ń':
                    case 'Ś':
                    case 'Ó':
                    case 'Ú':
                    case 'Ý':
                    case 'Ź':
                        sb.Append(")").Append(Normalise(c));
                        break;
                    case 'Â':
                    case 'Ê':
                    case 'Î':
                    case 'Ô':
                    case 'Û':
                        sb.Append("(").Append(Normalise(c));
                        break;
                    case 'Ä':
                    case 'Ë':
                    case 'Ï':
                    case 'Ö':
                    case 'Ü':
                    case 'Ÿ':
                        sb.Append("~").Append(Normalise(c));
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }

        private static string Normalise(char c)
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