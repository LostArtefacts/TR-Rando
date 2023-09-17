using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRRandomizerCore.Processors;

public class TR2ModelAdjuster : TR2LevelProcessor
{
    private static readonly Dictionary<TR2Entities, TR2Entities> _modelRemap = new()
    {
        [TR2Entities.Puzzle2_M_H] = TR2Entities.Puzzle3_M_H,
        [TR2Entities.PuzzleHole2] = TR2Entities.PuzzleHole3,
        [TR2Entities.PuzzleDone2] = TR2Entities.PuzzleDone3
    };

    private static readonly Dictionary<TR2Entities, TR2Entities> _spriteRemap = new()
    {
        [TR2Entities.Puzzle2_S_P] = TR2Entities.Puzzle3_S_P
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

        List<TR2Entity> entities = _levelInstance.Data.Entities.ToList();
        List<TRModel> models = _levelInstance.Data.Models.ToList();
        List<TRSpriteSequence> sprites = _levelInstance.Data.SpriteSequences.ToList();

        // Point the old models to the new ones, and any matching entities that are instances
        // of the old model, should also point to the new model type.
        foreach (TR2Entities oldEntity in _modelRemap.Keys)
        {
            TRModel model = models.Find(m => m.ID == (short)oldEntity);
            if (model != null)
            {
                model.ID = (uint)_modelRemap[oldEntity];

                List<TR2Entity> modelEntities = entities.FindAll(e => e.TypeID == (short)oldEntity);
                foreach (TR2Entity entity in modelEntities)
                {
                    entity.TypeID = (short)_modelRemap[oldEntity];
                }
            }
        }

        // Repeat for sprites
        foreach (TR2Entities oldEntity in _spriteRemap.Keys)
        {
            TRSpriteSequence sprite = sprites.Find(s => s.SpriteID == (short)oldEntity);
            if (sprite != null)
            {
                sprite.SpriteID = (short)_spriteRemap[oldEntity];

                List<TR2Entity> spriteEntities = entities.FindAll(e => e.TypeID == (short)oldEntity);
                foreach (TR2Entity entity in spriteEntities)
                {
                    entity.TypeID = (short)_spriteRemap[oldEntity];
                }
            }
        }
    }
}
