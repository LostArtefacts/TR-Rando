using TRLevelControl.Model;

namespace TRLevelControl.Helpers;

public static class TRAngleUtils
{
    private const float _deg360 = 360.0f;

    public static int GetRange(TRGameVersion version)
    {
        return version >= TRGameVersion.TR4 ? 4096 : 1024;
    }

    public static float FromGame(short rot, TRGameVersion version = TRGameVersion.TR1)
    {
        var range = GetRange(version);
        return (rot & (range - 1)) * (_deg360 / range);
    }

    public static short ToGame(float rot, TRGameVersion version = TRGameVersion.TR1)
    {
        rot %= _deg360;
        if (rot < 0)
        {
            rot += _deg360;
        }
        var range = GetRange(version);
        return (short)((int)Math.Round(rot * (range / _deg360)) & (range - 1));
    }
}
