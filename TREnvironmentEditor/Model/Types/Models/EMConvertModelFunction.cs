using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMConvertModelFunction : BaseEMFunction
{
    public uint OldModelID { get; set; }
    public uint NewModelID { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        ConvertModel(level.Models);
        UpdateModelEntities(level.Entities);
    }

    public override void ApplyToLevel(TR2Level level)
    {
        ConvertModel(level.Models.ToList());
        UpdateModelEntities(level.Entities);
    }

    public override void ApplyToLevel(TR3Level level)
    {
        ConvertModel(level.Models.ToList());
        UpdateModelEntities(level.Entities);
    }

    private void ConvertModel(List<TRModel> models)
    {
        if (models.Find(m => m.ID == NewModelID) == null)
        {
            TRModel oldModel = models.Find(m => m.ID == OldModelID);
            if (oldModel != null)
            {
                oldModel.ID = NewModelID;
            }
        }
    }

    private void UpdateModelEntities(List<TR1Entity> entities)
    {
        foreach (TR1Entity entity in entities)
        {
            if (entity.TypeID == (TR1Type)OldModelID)
            {
                entity.TypeID = (TR1Type)NewModelID;
            }
        }
    }

    private void UpdateModelEntities(IEnumerable<TR2Entity> entities)
    {
        foreach (TR2Entity entity in entities)
        {
            if (entity.TypeID == (TR2Type)OldModelID)
            {
                entity.TypeID = (TR2Type)NewModelID;
            }
        }
    }

    private void UpdateModelEntities(IEnumerable<TR3Entity> entities)
    {
        foreach (TR3Entity entity in entities)
        {
            if (entity.TypeID == (TR3Type)OldModelID)
            {
                entity.TypeID = (TR3Type)NewModelID;
            }
        }
    }
}
