using TRDataControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Textures;

namespace TRRandomizerCore.Processors.TR2.Tasks;

public class TR2XFixLaraTask : ITR2ProcessorTask
{
    private static readonly int[] _handMeshIds = [10, 13];

    private static readonly List<TR2Type> _handFixModels =
    [
        TR2Type.LaraShotgunAnim_H,
        TR2Type.LaraFlareAnim_H,
        TR2Type.LaraM16Anim_H,
        TR2Type.LaraGrenadeAnim_H,
        TR2Type.LaraHarpoonAnim_H,
        TR2Type.LaraMiscAnim_H,
    ];

    public required TR2TextureMonitorBroker TextureMonitor { get; set; }

    public void Run(TR2CombinedLevel level)
    {
        if (level.IsCutScene)
        {
            return;
        }

        ImportLara(level, TR2TypeUtilities.GetAliasForLevel(level.Name, TR2Type.Lara));
        if (level.Is(TR2LevelNames.HOME))
        {
            FixHSHLara(level);
            FixHSHCutscene(level.Data);
        }
    }

    private static void FixHSHLara(TR2CombinedLevel level)
    {
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

        FixHands(level.Data);
    }

    public static void FixHands(TR2Level level)
    {
        // Copy the faces from Lara's hands into all other Lara models, avoiding any dummy meshes.
        var lara = level.Models[TR2Type.Lara];
        var dummyMesh = lara.Meshes[0];

        var faceListGetters = new Func<TRMesh, List<TRMeshFace>>[]
        {
            m => m.TexturedRectangles,
            m => m.TexturedTriangles,
            m => m.ColouredRectangles,
            m => m.ColouredTriangles,
        };

        foreach (var type in _handFixModels)
        {
            if (!level.Models.TryGetValue(type, out var model))
            {
                continue;
            }

            foreach (var meshIdx in _handMeshIds)
            {
                var mesh = model.Meshes[meshIdx];
                if (mesh == dummyMesh)
                {
                    continue;
                }

                foreach (var getFaces in faceListGetters)
                {
                    var targetFaces = getFaces(mesh);
                    var sourceFaces = getFaces(lara.Meshes[meshIdx]);

                    targetFaces.RemoveAll(f => f.Vertices.All(v => v < 8));
                    targetFaces.AddRange(sourceFaces.Where(f => f.Vertices.All(v => v < 8)));
                }
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
