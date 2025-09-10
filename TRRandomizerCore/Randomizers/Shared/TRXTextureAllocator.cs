using TRGE.Core;
using TRRandomizerCore.Editors;

namespace TRRandomizerCore.Randomizers;

public class TRXTextureAllocator
{
    public required RandomizerSettings Settings { get; set; }
    public required Random Generator { get; set; }

    public void RandomizeWaterColour(TRXScriptedLevel level, bool isWireframe, bool isNightMode)
    {
        if (!Settings.RandomizeWaterColour)
        {
            return;
        }

        int minValue = GetMinWaterValue(isWireframe, isNightMode);
        level.WaterColor = [.. Enumerable.Range(0, 3)
            .Select(_ => Math.Round(Generator.Next(minValue, 101) / 100.0, 2))];

        if (level.CutSceneLevel is TRXScriptedLevel cutscene)
        {
            cutscene.WaterColor = level.WaterColor;
        }
    }

    private static int GetMinWaterValue(bool isWireframe, bool isNightMode)
    {
        int min = isWireframe ? 30 : 10;
        return isNightMode ? min + 10 : min;
    }
}
