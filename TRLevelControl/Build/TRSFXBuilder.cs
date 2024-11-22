using TRLevelControl.Model;

namespace TRLevelControl.Build;

public static class TRSFXBuilder
{
    public static List<short> ReadSoundMap(TRLevelReader reader)
    {
        List<short> map = new();
        while (reader.PeekUInt() >= ushort.MaxValue)
        {
            map.Add(reader.ReadInt16());
        }

        return map;
    }

    public static TRDictionary<S, T> Build<S, T>(List<short> soundMap, List<T> soundDetails)
        where S : Enum
        where T : class
    {
        TRDictionary<S, T> result = new();
        for (int i = 0; i < soundMap.Count; i++)
        {
            if (soundMap[i] == -1 || soundMap[i] >= soundDetails.Count)
            {
                continue;
            }

            result[(S)(object)(uint)i] = soundDetails[soundMap[i]];
        }

        return result;
    }
}
