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
            StringBuilder sb = new StringBuilder();
            foreach (char c in text)
            {
                switch (char.ToUpper(c))
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
                        sb.Append("#");
                        break;
                    case '~':
                    case '|':
                    case '_':
                        sb.Append('-');
                        break;
                    case '@':
                        sb.Append("AT");
                        break;
                    case 'ß':
                        sb.Append('=');
                        break;
                    default:
                        sb.Append(TextUtilities.Normalise(c));
                        break;
                }
            }
            return sb.ToString();
        }
    }
}
