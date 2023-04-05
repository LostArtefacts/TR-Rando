using System.Collections.Generic;
using System.Text;

namespace TRRandomizerCore.Globalisation
{
    public class TR1GameStrings : IGameStrings
    {
        public Dictionary<string, List<string>> GlobalStrings { get; set; }
        public Dictionary<string, TR1LevelStrings> LevelStrings { get; set; }

        public string Encode(string text)
        {
            // Uppercase accented characters will be normalised in all cases.
            // Some lowercase accented characters are supported.
            // We ignore accented i's because the dot remains.
            StringBuilder sb = new StringBuilder();
            foreach (char c in text)
            {
                string n = TextUtilities.Normalise(c);
                switch (c)
                {
                    case '(':
                    case '[':
                    case '{':
                        sb.Append('<');
                        break;
                    case ')':
                    case ']':
                    case '}':
                        sb.Append('>');
                        break;
                    case '"':
                        sb.Append('\'');
                        break;
                    case '^':
                        sb.Append('[');
                        break;
                    case '&':
                        sb.Append('+');
                        break;
                    case '*':
                    case '£':
                    case '$':
                    case '%':
                        sb.Append("#");
                        break;
                    case '~':
                    case '|':
                    case '_':
                    case '—':
                        sb.Append('-');
                        break;
                    case '@':
                        sb.Append("AT");
                        break;
                    case 'ß':
                        sb.Append('=');
                        break;
                    case '=':
                        sb.Append(':');
                        break;
                    case 'à':
                    case 'è':
                    case 'ò':
                    case 'ù':
                        sb.Append("$").Append(n);
                        break;
                    case 'á':
                    case 'ć':
                    case 'é':
                    case 'ń':
                    case 'ś':
                    case 'ó':
                    case 'ú':
                    case 'ý':
                    case 'ź':
                        sb.Append(")").Append(n);
                        break;
                    case 'â':
                    case 'ĉ':
                    case 'ê':
                    case 'ô':
                    case 'û':
                        sb.Append("(").Append(n);
                        break;
                    case 'ä':
                    case 'ë':
                    case 'ö':
                    case 'ü':
                    case 'ÿ':
                        sb.Append("~").Append(n);
                        break;
                    case 'ł':
                        sb.Append("l");
                        break;
                    default:
                        sb.Append(n);
                        break;
                }
            }
            return sb.ToString();
        }
    }
}
