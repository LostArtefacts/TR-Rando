using System.Drawing;
using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers;

public class TR3VfxRandomizer : BaseTR3Randomizer
{
    private List<TR3ScriptedLevel> _filterLevels;

    private Color[] _colors;

    public override void Randomize(int seed)
    {
        _generator = new Random(seed);

        _colors = ColorUtilities.GetAvailableColors();

        ChooseFilterLevels();

        foreach (TR3ScriptedLevel lvl in Levels)
        {
            LoadLevelInstance(lvl);

            if (_filterLevels.Contains(lvl))
            {
                SetVertexFilterMode(_levelInstance);
                SaveLevelInstance();
            }

            if (!TriggerProgress())
            {
                break;
            }
        }
    }

    private void ChooseFilterLevels()
    {
        TR3ScriptedLevel assaultCourse = Levels.Find(l => l.Is(TR3LevelNames.ASSAULT));
        ISet<TR3ScriptedLevel> exlusions = new HashSet<TR3ScriptedLevel> { assaultCourse };

        _filterLevels = Levels.RandomSelection(_generator, (int)Settings.NightModeCount, exclusions: exlusions);
        if (Settings.NightModeAssaultCourse)
        {
            _filterLevels.Add(assaultCourse);
        }
    }

    private void SetVertexFilterMode(TR3CombinedLevel level)
    {
        if (Settings.VfxRoom)
        {
            //Change every room
            FilterVerticesRandomRoom(level.Data);
        }
        else if (Settings.VfxLevel)
        {
            //Change every level
            FilterVerticesRandomLevel(level.Data, _colors[_generator.Next(0, _colors.Length - 1)]);
        }
        else
        {
            //Normal filter
            FilterVertices(level.Data);
        } 

        if (level.HasCutScene)
        {
            SetVertexFilterMode(level.CutSceneLevel);
        }
    }

    private void FilterVertices(TR3Level level)
    {
        foreach (TR3Room room in level.Rooms)
        {
            SetColourFilter(room, Settings.VfxFilterColor, Settings.VfxVivid, Settings.VfxCaustics, Settings.VfxWave);
        }
    }

    private void FilterVerticesRandomLevel(TR3Level level, Color col)
    {
        foreach (TR3Room room in level.Rooms)
        {
            SetColourFilter(room, col, Settings.VfxVivid, Settings.VfxCaustics, Settings.VfxWave);
        }
    }

    private void FilterVerticesRandomRoom(TR3Level level)
    {
        foreach (TR3Room room in level.Rooms)
        {
            Color col = _colors[_generator.Next(0, _colors.Length - 1)];

            SetColourFilter(room, col, Settings.VfxVivid, Settings.VfxCaustics, Settings.VfxWave);
        }
    }

    private static void SetColourFilter(TR3Room room, Color col, bool replace, bool enableCaustics, bool enableWave)
    {
        foreach (TR3RoomVertex vert in room.Mesh.Vertices)
        {
            byte curRed = (byte)((vert.Colour & 0x7C00) >> 10);
            byte curGreen = (byte)((vert.Colour & 0x03E0) >> 5);
            byte curBlue = (byte)(vert.Colour & 0x001F);

            byte newRed = ConvertColorChannelToRGB555(col.R);
            byte newGreen = ConvertColorChannelToRGB555(col.G);
            byte newBlue = ConvertColorChannelToRGB555(col.B);

            if (replace)
            {
                byte applyR = curRed;
                byte applyG = curGreen;
                byte applyB = curBlue;

                if (newRed > 0)
                    applyR = newRed;

                if (newGreen > 0)
                    applyG = newGreen;

                if (newBlue > 0)
                    applyB = newBlue;

                vert.Colour = (ushort)((applyR << 10) | (applyG << 5) | (applyB));
            }
            else
            {
                vert.Colour = (ushort)((Blend(curRed, newRed) << 10) | (Blend(curGreen, newGreen) << 5) | (Blend(curBlue, newBlue)));
            }

            // #296 Retain original caustics and waves for water/swamp rooms
            if (!vert.UseCaustics)
                vert.UseCaustics = enableCaustics;
            if (!vert.UseWaveMovement)
                vert.UseWaveMovement = enableWave;
        }
    }

    private static byte ConvertColorChannelToRGB555(byte col)
    {
        return (byte)(((col - byte.MinValue) * (31 - 0)) / (byte.MaxValue - byte.MinValue) + 0);
    }

    private static byte Blend(byte curChannel, byte newChannel)
    {
        return Math.Min((byte)((newChannel * 0.1) + curChannel * (1 - 0.1)), (byte)31);
    }
}
