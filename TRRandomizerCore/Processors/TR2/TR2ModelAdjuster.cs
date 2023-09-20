using TRGE.Core;
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
        foreach (TR2ScriptedLevel lvl in Levels)
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

        List<TRModel> models = _levelInstance.Data.Models.ToList();
        List<TRSpriteSequence> sprites = _levelInstance.Data.SpriteSequences.ToList();

        // Point the old models to the new ones, and any matching entities that are instances
        // of the old model, should also point to the new model type.
        foreach (TR2Type oldEntity in _modelRemap.Keys)
        {
            TRModel model = models.Find(m => m.ID == (short)oldEntity);
            if (model != null)
            {
                model.ID = (uint)_modelRemap[oldEntity];

                List<TR2Entity> modelEntities = _levelInstance.Data.Entities.FindAll(e => e.TypeID == oldEntity);
                foreach (TR2Entity entity in modelEntities)
                {
                    entity.TypeID = _modelRemap[oldEntity];
                }
            }
        }

        // Repeat for sprites
        foreach (TR2Type oldEntity in _spriteRemap.Keys)
        {
            TRSpriteSequence sprite = sprites.Find(s => s.SpriteID == (short)oldEntity);
            if (sprite != null)
            {
                sprite.SpriteID = (short)_spriteRemap[oldEntity];

                List<TR2Entity> spriteEntities = _levelInstance.Data.Entities.FindAll(e => e.TypeID == oldEntity);
                foreach (TR2Entity entity in spriteEntities)
                {
                    entity.TypeID = _spriteRemap[oldEntity];
                }
            }
        }
    }
}
