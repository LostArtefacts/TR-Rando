﻿using TRLevelControl.Model;

namespace TRDataControl.Environment;

public class EMModifyEntityFunction : BaseEMFunction
{
    public int EntityIndex { get; set; }
    public EMEntityFinder EntityFinder { get; set; }
    public bool? Invisible { get; set; }
    public bool? ClearBody { get; set; }
    public short? Intensity1 { get; set; }
    public short? Intensity2 { get; set; }
    public ushort? Flags { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);
        int entityIndex = EntityFinder?.GetEntity(level) ?? EntityIndex;
        ModifyEntity(level.Entities[data.ConvertEntity(entityIndex)]);
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);
        int entityIndex = EntityFinder?.GetEntity(level) ?? EntityIndex;
        ModifyEntity(level.Entities[data.ConvertEntity(entityIndex)]);
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);
        int entityIndex = EntityFinder?.GetEntity(level) ?? EntityIndex;
        ModifyEntity(level.Entities[data.ConvertEntity(entityIndex)]);
    }

    private void ModifyEntity(TR1Entity entity)
    {
        ModifyEntity<TR1Type>(entity);
        if (Intensity1.HasValue)
        {
            entity.Intensity = Intensity1.Value;
        }
        if (Intensity2.HasValue)
        {
            entity.Intensity = Intensity2.Value;
        }
    }

    private void ModifyEntity(TR2Entity entity)
    {
        ModifyEntity<TR2Type>(entity);
        if (Intensity1.HasValue)
        {
            entity.Intensity1 = Intensity1.Value;
        }
        if (Intensity2.HasValue)
        {
            entity.Intensity2 = Intensity2.Value;
        }
    }

    private void ModifyEntity(TR3Entity entity)
    {
        ModifyEntity<TR3Type>(entity);
        if (Intensity1.HasValue)
        {
            entity.Intensity1 = Intensity1.Value;
        }
        if (Intensity2.HasValue)
        {
            entity.Intensity2 = Intensity2.Value;
        }
    }

    private void ModifyEntity<T>(TREntity<T> entity)
        where T : Enum
    {
        if (Invisible.HasValue)
        {
            entity.Invisible = Invisible.Value;
        }
        if (ClearBody.HasValue)
        {
            entity.ClearBody = ClearBody.Value;
        }
        if (Flags.HasValue)
        {
            entity.Flags = Flags.Value;
        }
    }
}
