﻿using System.Text;

namespace TRRandomizerCore.Globalisation;

public class TR23GameStrings : IGameStrings
{
    public GlobalStrings[] GlobalStrings { get; set; }
    public Dictionary<string, TR23LevelStrings> LevelStrings { get; set; }

    // >  => +
    // =  => ß
    // )e => é
    // $e => è
    // (e => ê
    // ~e => ë
    //
    // () become []
    // " becomes '
    // ^ becomes an upwards arrow
    // & becomes +
    // * % are ignored
    //
    // Characters with unsupported accents become normalised e.g. å => a.
    //
    // Note that accent support on MultiPatch isn't great. The likes of Fidèle becomes Fid` ele.
    // UKBox, EPC and TR2Main work best.
    public string Encode(string text)
    {
        StringBuilder sb = new();
        foreach (char c in text)
        {
            string n = TextUtilities.Normalise(c);
            char d = char.ToUpper(c);
            switch (d)
            {
                case '(':
                    sb.Append('<');
                    break;
                case ')':
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
                case '%':
                    break;
                case 'ß':
                    sb.Append('=');
                    break;
                case 'À':
                case 'È':
                case 'Ì':
                case 'Ò':
                case 'Ù':
                    sb.Append('$').Append(n);
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
                    sb.Append(')').Append(n);
                    break;
                case 'Â':
                case 'Ê':
                case 'Î':
                case 'Ô':
                case 'Û':
                    sb.Append('(').Append(n);
                    break;
                case 'Ä':
                case 'Ë':
                case 'Ï':
                case 'Ö':
                case 'Ü':
                case 'Ÿ':
                    sb.Append('~').Append(n);
                    break;
                default:
                    sb.Append(n);
                    break;
            }
        }
        return sb.ToString();
    }
}
