using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;

namespace TRRandomizerCore.Processors.TR2.Tasks;

public class TR2XPickupTask : ITR2ProcessorTask
{
    private static readonly PickupInfo _info =
        JsonUtils.ReadFile<PickupInfo>("Resources/TR2/pickup_info.json");

    public bool ReassignPuzzleItems { get; set; }

    public void Run(TR2CombinedLevel level)
    {
        var typeTargets = _info.LevelTargets.TryGetValue(level.Name, out var levelTypes)
            ? _info.DefaultTargets.Union(levelTypes)
            : _info.DefaultTargets;
        foreach (var (type, factor) in typeTargets)
        {
            ScalePickup(type, level.Data.Models[type], factor);
            FixPickup(type, level);
        }

        AlterPuzzles(level);
        AlterStopwatch(level);
        AlterGuns(level);
    }

    private void AlterPuzzles(TR2CombinedLevel level)
    {
        if (level.Is(TR2LevelNames.LAIR) || !ReassignPuzzleItems)
        {
            return;
        }

        // P2 items are converted to P3 in case the dragon is present to ensure the dagger can be safely imported.
        // OG P2 key items must be zoned based on being P3.
        foreach (var (oldType, newType) in new Dictionary<TR2Type, TR2Type>
        {
            [TR2Type.Puzzle2_M_H] = TR2Type.Puzzle3_M_H,
            [TR2Type.PuzzleHole2] = TR2Type.PuzzleHole3,
            [TR2Type.PuzzleDone2] = TR2Type.PuzzleDone3,
        })
        {
            level.Data.Models.ChangeKey(oldType, newType);
            level.Data.Entities.FindAll(e => e.TypeID == oldType)
                .ForEach(e => e.TypeID = newType);
        }

        level.Data.Sprites.ChangeKey(TR2Type.Puzzle2_S_P, TR2Type.Puzzle3_S_P);
        level.Data.Entities.FindAll(e => e.TypeID == TR2Type.Puzzle2_S_P)
                .ForEach(e => e.TypeID = TR2Type.Puzzle3_S_P);
    }

    private static void AlterStopwatch(TR2CombinedLevel level)
    {
        if (level.Data.Models.TryGetValue(TR2Type.Stopwatch_M_H, out var model))
        {
            model.Meshes[7].TexturedRectangles.Clear();
            model.Meshes[7].TexturedTriangles.Clear();
        }
    }

    private static void AlterGuns(TR2CombinedLevel level)
    {
        var gunModels = new[] { TR2Type.Pistols_M_H, TR2Type.Autos_M_H, TR2Type.Uzi_M_H };
        foreach (var model in gunModels.Select(t => level.Data.Models[t]).Where(m => m != null))
        {
            model.Meshes[0].ColouredRectangles.Clear();
            model.Meshes[0].ColouredTriangles.Clear();
            model.Meshes[0].TexturedRectangles.Clear();
            model.Meshes[0].TexturedTriangles.Clear();
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

    public static void FixPickup(TR2Type type, TR2CombinedLevel level)
    {
        if (!level.Data.Models.TryGetValue(type, out var model))
        {
            return;
        }

        if (type == TR2Type.SmallMed_M_H)
        {
            FixSmallMed(model, level);
        }
        else if (type == TR2Type.LargeMed_M_H)
        {
            FixLargeMed(model, level);
        }
        else if (type == TR2Type.Grenades_M_H)
        {
            FixGrenades(model);
        }
        else if ((level.Is(TR2LevelNames.RIG) || level.Is(TR2LevelNames.DA) || level.Is(TR2LevelNames.FOOLGOLD))
            && type >= TR2Type.Key1_M_H && type <= TR2Type.Key4_M_H)
        {
            FixYaw(model);
        }
        else if (level.Is(TR2LevelNames.XIAN) && type == TR2Type.Puzzle1_M_H)
        {
            FixYaw(model);
        }
        else if (level.Is(TR2LevelNames.FLOATER) && (type == TR2Type.Puzzle1_M_H || type == TR2Type.Puzzle2_M_H))
        {
            FixYaw(model);
        }
    }

    private static void FixSmallMed(TRModel model, TR2CombinedLevel level)
    {
        var meshIds = new[] { 1, 5, 6 };
        foreach (var mesh in meshIds.Select(m => model.Meshes[m]))
        {
            if (level.IsExpansion && mesh.ColouredRectangles.Count == 1)
            {
                var face = mesh.ColouredRectangles[0];
                face.Texture = mesh.TexturedRectangles[0].Texture;
                mesh.TexturedRectangles.Add(face);
                mesh.ColouredRectangles.Clear();
            }
            else
            {
                mesh.ColouredRectangles[2].Texture = mesh.ColouredRectangles[1].Texture;
            }
        }
    }

    private static void FixLargeMed(TRModel model, TR2CombinedLevel level)
    {
        var mesh = model.Meshes[1];
        foreach (var faceIdx in new[] { 9, 15 })
        {
            mesh.ColouredRectangles[faceIdx].Texture = mesh.ColouredRectangles[8].Texture;
        }

        if (level.IsExpansion)
        {
            var smallMedTex = level.Data.Models[TR2Type.SmallMed_M_H].Meshes[5].TexturedRectangles[0].Texture;
            for (int i = 17; i >= 6; i--)
            {
                var face = mesh.ColouredRectangles[i];
                face.Texture = smallMedTex;
                mesh.TexturedRectangles.Add(face);
                mesh.ColouredRectangles.RemoveAt(i);
            }            
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
