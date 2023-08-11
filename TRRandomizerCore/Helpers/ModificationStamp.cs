using TRGE.Core;

namespace TRRandomizerCore.Helpers;

public class ModificationStamp
{
    public string English { get; set; }
    public string German { get; set; }
    public string French { get; set; }

    internal void ApplyTo(GameStamp stamp)
    {
        stamp[TRLanguage.English] = English;
        stamp[TRLanguage.German] = German;
        stamp[TRLanguage.French] = French;
    }
}
