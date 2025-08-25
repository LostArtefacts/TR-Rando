using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRRandomizerCore.Processors;

public class TR2ModelAdjuster : TR2LevelProcessor
{
    private static readonly Dictionary<TR2Type, TR2Type> _modelRemap = new()
    {
        [TR2Type.Puzzle2_M_H] = TR2Type.Puzzle3_M_H,
        [TR2Type.PuzzleHole2] = TR2Type.PuzzleHole3,
        [TR2Type.PuzzleDone2] = TR2Type.PuzzleDone3
    };

    private static readonly Dictionary<TR2Type, TR2Type> _spriteRemap = new()
    {
        [TR2Type.Puzzle2_S_P] = TR2Type.Puzzle3_S_P
    };

    public void AdjustModels()
    {
        foreach (var lvl in Levels)
        {
            LoadLevelInstance(lvl);

            AdjustInstanceModels();

            SaveLevelInstance();

            if (!TriggerProgress())
            {
                break;
            }
        }
    }

    private void AdjustInstanceModels()
    {
        if (_levelInstance.Is(TR2LevelNames.LAIR))
        {
            return;
        }

        // Point the old models to the new ones, and any matching entities that are instances
        // of the old model, should also point to the new model type.
        foreach (TR2Type oldEntity in _modelRemap.Keys)
        {
            if (_levelInstance.Data.Models.ChangeKey(oldEntity, _modelRemap[oldEntity]))
            {
                ConvertEntities(oldEntity, _modelRemap[oldEntity]);
            }
        }

        // Repeat for sprites
        foreach (TR2Type oldEntity in _spriteRemap.Keys)
        {
            if (_levelInstance.Data.Sprites.ChangeKey(oldEntity, _spriteRemap[oldEntity]))
            {
                ConvertEntities(oldEntity, _spriteRemap[oldEntity]);
            }
        }
    }

    private void ConvertEntities(TR2Type oldType, TR2Type newType)
    {
        IEnumerable<TR2Entity> spriteEntities = _levelInstance.Data.Entities.Where(e => e.TypeID == oldType);
        foreach (TR2Entity entity in spriteEntities)
        {
            entity.TypeID = newType;
        }
    }
}
