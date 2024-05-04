using TREnvironmentEditor.Helpers;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMMoveSecretFunction : EMMovePickupFunction
{
    public override void ApplyToLevel(TR1Level level)
    {
        Types = new List<short>();
        foreach (EMLocation location in SectorLocations)
        {
            int entityIndex = location.GetContainedSecretEntity(level);
            if (entityIndex != -1)
            {
                Types.Add((short)level.Entities[entityIndex].TypeID);
            }
        }

        base.ApplyToLevel(level);
    }

    public override void ApplyToLevel(TR2Level level)
    {
        Types = new List<short>();
        foreach (EMLocation location in SectorLocations)
        {
            int entityIndex = location.GetContainedSecretEntity(level);
            if (entityIndex != -1)
            {
                Types.Add((short)level.Entities[entityIndex].TypeID);
            }
        }

        base.ApplyToLevel(level);
    }

    public override void ApplyToLevel(TR3Level level)
    {
        Types = new List<short>();
        foreach (EMLocation location in SectorLocations)
        {
            int entityIndex = location.GetContainedSecretEntity(level);
            if (entityIndex != -1)
            {
                Types.Add((short)level.Entities[entityIndex].TypeID);
            }
        }

        base.ApplyToLevel(level);
    }
}
