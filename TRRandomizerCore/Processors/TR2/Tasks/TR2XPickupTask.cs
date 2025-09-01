using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;

namespace TRRandomizerCore.Processors.TR2.Tasks;

public class TR2XPickupTask : ITR2ProcessorTask
{
    private static readonly PickupInfo _info =
        JsonUtils.ReadFile<PickupInfo>("Resources/TR2/pickup_info.json");

    public void Run(TR2CombinedLevel level)
    {
        var typeTargets = _info.LevelTargets.TryGetValue(level.Name, out var levelTypes)
            ? _info.DefaultTargets.Union(levelTypes)
            : _info.DefaultTargets;
        foreach (var (type, factor) in typeTargets)
        {
            ScalePickup(type, level.Data.Models[type], factor);
            FixPickup(type, level.Data.Models[type], level.Name);
        }
    }

    public static void ScalePickup(TR2Type type, TRModel model, float factor)
    {
        if (factor == 1.0f || model == null)
        {
            return;
        }

        model.Meshes.ForEach(m => m.Scale(factor));

        var frameInfo = _info.FrameInfo.TryGetValue(type, out var value)
            ? value
            : new() { Bounds = model.Meshes.First().GetBounds(), Rotations = [new()] };

        if (model.Animations.Count == 0)
        {
            model.Animations.Add(new()
            {
                Accel = new(),
                Speed = new(),
                FrameRate = 1,
                Frames = [frameInfo],
            });
        }

        foreach (var frame in model.Animations.SelectMany(a => a.Frames))
        {
            frame.Bounds = frameInfo.Bounds;
            frame.OffsetX = frameInfo.OffsetX;
            frame.OffsetY = frameInfo.OffsetY;
            frame.OffsetZ = frameInfo.OffsetZ;
        }
    }

    public static void FixPickup(TR2Type type, TRModel model, string levelName)
    {
        if (model == null)
        {
            return;
        }

        if (type == TR2Type.SmallMed_M_H)
        {
            FixSmallMed(model);
        }
        else if (type == TR2Type.LargeMed_M_H)
        {
            FixLargeMed(model);
        }
        else if (type == TR2Type.Grenades_M_H)
        {
            FixGrenades(model);
        }
        else if ((levelName == TR2LevelNames.RIG || levelName == TR2LevelNames.DA || levelName == TR2LevelNames.FOOLGOLD)
            && type >= TR2Type.Key1_M_H && type <= TR2Type.Key4_M_H)
        {
            FixYaw(model);
        }
        else if (levelName == TR2LevelNames.XIAN && type == TR2Type.Puzzle1_M_H)
        {
            FixYaw(model);
        }
        else if (levelName == TR2LevelNames.FLOATER && (type == TR2Type.Puzzle1_M_H || type == TR2Type.Puzzle2_M_H))
        {
            FixYaw(model);
        }
    }

    private static void FixSmallMed(TRModel model)
    {
        var meshIds = new[] { 1, 5, 6 };
        foreach (var mesh in meshIds.Select(m => model.Meshes[m]))
        {
            mesh.ColouredRectangles[2].Texture = mesh.ColouredRectangles[1].Texture;
        }
    }

    private static void FixLargeMed(TRModel model)
    {
        var mesh = model.Meshes[1];
        foreach (var faceIdx in new[] { 9, 15 })
        {
            mesh.ColouredRectangles[faceIdx].Texture = mesh.ColouredRectangles[8].Texture;
        }
    }

    private static void FixGrenades(TRModel model)
    {
        // The OG tree is a mess and doesn't work well with scaling
        model.MeshTrees[0].OffsetX = -54;
        model.MeshTrees[0].OffsetY = 17;
        model.MeshTrees[0].OffsetZ = 6;
        model.MeshTrees[1].OffsetX = -8;
        model.MeshTrees[1].OffsetY = 17;
        model.MeshTrees[1].OffsetZ = 6;

        // Fix a broken face
        TRMesh mesh = model.Meshes[1];
        TRMeshFace texFace = mesh.TexturedRectangles.First();
        mesh.ColouredRectangles.ForEach(f => f.Texture = texFace.Texture);
        mesh.TexturedRectangles.AddRange(mesh.ColouredRectangles);
        mesh.ColouredRectangles.Clear();

        // The second grenade isn't identical to the first, so just clone
        model.Meshes[2] = mesh.Clone();
        model.Animations.SelectMany(a => a.Frames)
            .ToList()
            .ForEach(f => f.Rotations[2] = f.Rotations[1]);
    }

    private static void FixYaw(TRModel model)
    {
        // Make models face the correct way in the inventory
        foreach (var frame in model.Animations.SelectMany(a => a.Frames))
        {
            frame.Rotations.ForEach(r => r.Y = (short)(r.Y == 0 ? 512 : 0));
        }
    }

    private class PickupInfo
    {
        public Dictionary<TR2Type, float> DefaultTargets { get; set; }
        public Dictionary<string, Dictionary<TR2Type, float>> LevelTargets { get; set; }
        public Dictionary<TR2Type, TRAnimFrame> FrameInfo { get; set; }
    }
}
