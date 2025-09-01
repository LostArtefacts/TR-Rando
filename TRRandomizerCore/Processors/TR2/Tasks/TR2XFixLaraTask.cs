using TRDataControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Textures;

namespace TRRandomizerCore.Processors.TR2.Tasks;

public class TR2XFixLaraTask : ITR2ProcessorTask
{
    private static readonly Dictionary<TR2Type, int[]> _hshHandMeshMap = new()
    {
        [TR2Type.LaraShotgunAnim_H] = [10, 13],
        [TR2Type.LaraFlareAnim_H] = [13],
        [TR2Type.LaraMiscAnim_H] = [10],
    };

    public required TR2TextureMonitorBroker TextureMonitor { get; set; }

    public void Run(TR2CombinedLevel level)
    {
        if (level.IsAssault)
        {
            FixGymLara(level);
        }
        else if (level.Is(TR2LevelNames.HOME))
        {
            FixHSHLara(level);
            FixHSHCutscene(level.Data);
        }
    }

    private void FixGymLara(TR2CombinedLevel level)
    {
        ImportLara(level, TR2Type.LaraAssault);
    }

    private void FixHSHLara(TR2CombinedLevel level)
    {
        ImportLara(level, TR2Type.LaraHome);
        FixHSHHands(level.Data);

        // Clear the dagger from Lara's hips in the misc anim set
        var hips = level.Data.Models[TR2Type.Lara].Meshes[0].Clone();
        hips.TexturedRectangles.RemoveAll(f => f.Vertices.All(v => v > 19));
        hips.TexturedTriangles.RemoveAll(f => f.Vertices.All(v => v > 19));
        hips.Vertices.RemoveRange(20, hips.Vertices.Count - 20);
        hips.Normals.RemoveRange(20, hips.Normals.Count - 20);
        level.Data.Models[TR2Type.LaraMiscAnim_H].Meshes[0] = hips;
    }

    private void ImportLara(TR2CombinedLevel level, TR2Type type)
    {
        var importer = new TR2DataImporter(isCommunityPatch: true)
        {
            DataFolder = "Resources/TR2/Objects",
            Level = level.Data,
            TypesToImport = [type],
        };
        importer.TextureMonitor = TextureMonitor.CreateMonitor(level.Name, importer.TypesToImport);
        importer.Import();
    }

    public static void FixHSHHands(TR2Level level)
    {
        // Ensures Lara's gloves are consistent between all models. Only needed
        // for HSH outfit as it's wildly inconsistent.
        var lara = level.Models[TR2Type.Lara];
        foreach (var (type, meshIds) in _hshHandMeshMap)
        {
            foreach (var meshIdx in meshIds)
            {
                var mesh = level.Models[type].Meshes[meshIdx];
                mesh.TexturedRectangles.RemoveAll(f => f.Vertices.All(v => v < 8));
                mesh.TexturedTriangles.RemoveAll(f => f.Vertices.All(v => v < 8));
                mesh.TexturedRectangles.AddRange(lara.Meshes[meshIdx].TexturedRectangles
                    .Where(f => f.Vertices.All(v => v < 8)));
                mesh.TexturedTriangles.AddRange(lara.Meshes[meshIdx].TexturedTriangles
                    .Where(f => f.Vertices.All(v => v < 8)));
            }
        }
    }

    private static void FixHSHCutscene(TR2Level level)
    {
        // Get the camera out of the wall when Lara shoots
        for (int i = 1; i < 4; i++)
        {
            level.CinematicFrames[^i].Target.X += 208;
            level.CinematicFrames[^i].Position.X -= 144;
        }

        // Extend the ending to allow the fade to play out
        const int extraLength = 120;

        var endAnim = level.Models[TR2Type.LaraMiscAnim_H].Animations[2];
        var lastFrame = endAnim.Frames[^1];
        var lastCine = level.CinematicFrames[^1];

        for (int i = 0; i < extraLength; i++)
        {
            level.CinematicFrames.Add(lastCine.Clone());
            endAnim.Frames.Add(lastFrame.Clone());
            endAnim.FrameEnd++;
        }
    }
}
