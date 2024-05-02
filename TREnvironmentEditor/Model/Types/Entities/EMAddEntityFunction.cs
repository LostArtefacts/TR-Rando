using TREnvironmentEditor.Helpers;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMAddEntityFunction : BaseEMFunction
{
    private static readonly int _defaultEntityLimit = 256;

    public short TypeID { get; set; }
    public ushort Flags { get; set; }
    public short? Intensity { get; set; }
    public EMLocation Location { get; set; }
    // If defined, anything else on the same tile will be moved here
    public EMLocation TargetRelocation { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        int limit = _isCommunityPatch ? 10240 : _defaultEntityLimit;
        if (level.Entities.Count < limit)
        {
            EMLevelData data = GetData(level);
            if (TargetRelocation != null)
            {
                EMLocation location = data.ConvertLocation(Location);
                TRRoomSector sector = level.GetRoomSector(location);
                foreach (TR1Entity entity in level.Entities)
                {
                    if (entity.Room == location.Room && level.GetRoomSector(entity) == sector)
                    {
                        entity.X = TargetRelocation.X;
                        entity.Y = TargetRelocation.Y;
                        entity.Z = TargetRelocation.Z;
                        entity.Room = data.ConvertRoom(TargetRelocation.Room);
                    }
                }
            }

            level.Entities.Add(CreateTREntity(data));
        }
    }

    public override void ApplyToLevel(TR2Level level)
    {
        if (level.Entities.Count < _defaultEntityLimit)
        {
            EMLevelData data = GetData(level);
            if (TargetRelocation != null)
            {
                EMLocation location = data.ConvertLocation(Location);
                TRRoomSector sector = level.GetRoomSector(location);
                foreach (TR2Entity entity in level.Entities)
                {
                    if (entity.Room == location.Room && level.GetRoomSector(entity) == sector)
                    {
                        entity.X = TargetRelocation.X;
                        entity.Y = TargetRelocation.Y;
                        entity.Z = TargetRelocation.Z;
                        entity.Room = data.ConvertRoom(TargetRelocation.Room);
                    }
                }
            }

            level.Entities.Add(CreateTR2Entity(data));
        }
    }

    public override void ApplyToLevel(TR3Level level)
    {
        int limit = _isCommunityPatch ? 1024 : _defaultEntityLimit;
        if (level.Entities.Count < limit)
        {
            EMLevelData data = GetData(level);
            if (TargetRelocation != null)
            {
                EMLocation location = data.ConvertLocation(Location);
                TRRoomSector sector = level.GetRoomSector(location);
                foreach (TR3Entity entity in level.Entities)
                {
                    if (entity.Room == location.Room && level.GetRoomSector(entity) == sector)
                    {
                        entity.X = TargetRelocation.X;
                        entity.Y = TargetRelocation.Y;
                        entity.Z = TargetRelocation.Z;
                        entity.Room = data.ConvertRoom(TargetRelocation.Room);
                    }
                }
            }

            level.Entities.Add(CreateTR3Entity(data));
        }
    }

    private TR1Entity CreateTREntity(EMLevelData data)
    {
        return new()
        {
            TypeID = (TR1Type)TypeID,
            X = Location.X,
            Y = Location.Y,
            Z = Location.Z,
            Room = data.ConvertRoom(Location.Room),
            Angle = Location.Angle,
            Flags = Flags,
            Intensity = Intensity ?? 6400
        };
    }

    private TR2Entity CreateTR2Entity(EMLevelData data)
    {
        return new()
        {
            TypeID = (TR2Type)TypeID,
            X = Location.X,
            Y = Location.Y,
            Z = Location.Z,
            Room = data.ConvertRoom(Location.Room),
            Angle = Location.Angle,
            Flags = Flags,
            Intensity1 = Intensity ?? -1,
            Intensity2 = Intensity ?? -1
        };
    }

    private TR3Entity CreateTR3Entity(EMLevelData data)
    {
        return new()
        {
            TypeID = (TR3Type)TypeID,
            X = Location.X,
            Y = Location.Y,
            Z = Location.Z,
            Room = data.ConvertRoom(Location.Room),
            Angle = Location.Angle,
            Flags = Flags,
            Intensity1 = Intensity ?? -1,
            Intensity2 = Intensity ?? -1
        };
    }
}
