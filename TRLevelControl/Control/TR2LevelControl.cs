using System.Diagnostics;
using TRLevelControl.Build;
using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRLevelControl;

public class TR2LevelControl : TRLevelControlBase<TR2Level>
{
    private readonly TRObjectMeshBuilder<TR2Type> _meshBuilder;
    private readonly TRSpriteBuilder<TR2Type> _spriteBuilder;
    private readonly TR2RoomBuilder _roomBuilder;

    public TR2LevelControl(ITRLevelObserver observer = null)
        : base(observer)
    {
        _meshBuilder = new(TRGameVersion.TR2, _observer);
        _spriteBuilder = new(TRGameVersion.TR2);
        _roomBuilder = new();
    }

    protected override TR2Level CreateLevel(TRFileVersion version)
    {
        TR2Level level = new()
        {
            Version = new()
            {
                Game = TRGameVersion.TR2,
                File = version
            }
        };

        TestVersion(level, TRFileVersion.TR2);
        return level;
    }

    protected override void Read(TRLevelReader reader)
    {
        // Colour palettes and textures
        _level.Palette = reader.ReadColours(TRConsts.PaletteSize);
        _level.Palette16 = reader.ReadColour4s(TRConsts.PaletteSize);

        uint numImages = reader.ReadUInt32();
        _level.Images8 = reader.ReadImage8s(numImages);
        _level.Images16 = reader.ReadImage16s(numImages);

        // Unused, always 0 in OG
        _level.Version.LevelNumber = reader.ReadUInt32();

        ReadRooms(reader);

        ReadMeshData(reader);
        ReadModelData(reader);

        ReadStaticMeshes(reader);

        uint numObjectTextures = reader.ReadUInt32();
        _level.ObjectTextures = new();
        for (int i = 0; i < numObjectTextures; i++)
        {
            _level.ObjectTextures.Add(TR2FileReadUtilities.ReadObjectTexture(reader));
        }

        ReadSprites(reader);

        uint numCameras = reader.ReadUInt32();
        _level.Cameras = new();
        for (int i = 0; i < numCameras; i++)
        {
            _level.Cameras.Add(TR2FileReadUtilities.ReadCamera(reader));
        }

        uint numSoundSources = reader.ReadUInt32();
        _level.SoundSources = new();
        for (int i = 0; i < numSoundSources; i++)
        {
            _level.SoundSources.Add(TR2FileReadUtilities.ReadSoundSource(reader));
        }

        //Boxes
        uint numBoxes = reader.ReadUInt32();
        _level.Boxes = new();
        for (int i = 0; i < numBoxes; i++)
        {
            _level.Boxes.Add(TR2FileReadUtilities.ReadBox(reader));
        }

        //Overlaps & Zones
        uint numOverlaps = reader.ReadUInt32();
        _level.Overlaps = reader.ReadUInt16s(numOverlaps).ToList();

        ushort[] zoneData = reader.ReadUInt16s(numBoxes * 10);
        _level.Zones = TR2BoxUtilities.ReadZones(numBoxes, zoneData);

        reader.ReadUInt32(); // Total count of ushorts
        ushort numGroups = reader.ReadUInt16();
        _level.AnimatedTextures = new();
        for (int i = 0; i < numGroups; i++)
        {
            _level.AnimatedTextures.Add(TR2FileReadUtilities.ReadAnimatedTexture(reader));
        }

        //Entities
        uint numEntities = reader.ReadUInt32();
        _level.Entities = reader.ReadTR2Entities(numEntities);

        _level.LightMap = new(reader.ReadBytes(TRConsts.LightMapSize));

        //Cinematic Frames
        ushort numCinematicFrames = reader.ReadUInt16();
        _level.CinematicFrames = new();
        for (int i = 0; i < numCinematicFrames; i++)
        {
            _level.CinematicFrames.Add(TR2FileReadUtilities.ReadCinematicFrame(reader));
        }

        ushort numDemoData = reader.ReadUInt16();
        _level.DemoData = reader.ReadBytes(numDemoData);

        ReadSoundEffects(reader);
    }

    protected override void Write(TRLevelWriter writer)
    {
        Debug.Assert(_level.Palette.Count == TRConsts.PaletteSize);
        Debug.Assert(_level.Palette16.Count == TRConsts.PaletteSize);
        writer.Write(_level.Palette);
        writer.Write(_level.Palette16);

        Debug.Assert(_level.Images8.Count == _level.Images16.Count);
        writer.Write((uint)_level.Images8.Count);
        writer.Write(_level.Images8);
        writer.Write(_level.Images16);

        writer.Write(_level.Version.LevelNumber);

        WriteRooms(writer);

        WriteMeshData(writer);
        WriteModelData(writer);

        WriteStaticMeshes(writer);

        writer.Write((uint)_level.ObjectTextures.Count);
        foreach (TRObjectTexture tex in _level.ObjectTextures) { writer.Write(tex.Serialize()); }
        WriteSprites(writer);

        writer.Write((uint)_level.Cameras.Count);
        foreach (TRCamera cam in _level.Cameras) { writer.Write(cam.Serialize()); }

        writer.Write((uint)_level.SoundSources.Count);
        foreach (TRSoundSource src in _level.SoundSources) { writer.Write(src.Serialize()); }

        writer.Write((uint)_level.Boxes.Count);
        foreach (TR2Box box in _level.Boxes) { writer.Write(box.Serialize()); }
        writer.Write((uint)_level.Overlaps.Count);
        writer.Write(_level.Overlaps);
        writer.Write(TR2BoxUtilities.FlattenZones(_level.Zones));

        byte[] animTextureData = _level.AnimatedTextures.SelectMany(a => a.Serialize()).ToArray();
        writer.Write((uint)(animTextureData.Length / sizeof(ushort)) + 1);
        writer.Write((ushort)_level.AnimatedTextures.Count);
        writer.Write(animTextureData);

        writer.Write((uint)_level.Entities.Count);
        writer.Write(_level.Entities);

        Debug.Assert(_level.LightMap.Count == TRConsts.LightMapSize);
        writer.Write(_level.LightMap.ToArray());

        writer.Write((ushort)_level.CinematicFrames.Count);
        foreach (TRCinematicFrame cineframe in _level.CinematicFrames) { writer.Write(cineframe.Serialize()); }

        writer.Write((ushort)_level.DemoData.Length);
        writer.Write(_level.DemoData);

        WriteSoundEffects(writer);
    }

    private void ReadRooms(TRLevelReader reader)
    {
        ushort numRooms = reader.ReadUInt16();
        _level.Rooms = new();
        for (int i = 0; i < numRooms; i++)
        {
            TR2Room room = new()
            {
                Info = reader.ReadRoomInfo(_level.Version.Game)
            };
            _level.Rooms.Add(room);

            _roomBuilder.ReadRawMesh(reader);

            ushort numPortals = reader.ReadUInt16();
            room.Portals = reader.ReadRoomPortals(numPortals);

            room.NumZSectors = reader.ReadUInt16();
            room.NumXSectors = reader.ReadUInt16();
            room.Sectors = reader.ReadRoomSectors(room.NumXSectors * room.NumZSectors);

            room.AmbientIntensity = reader.ReadInt16();
            room.AmbientIntensity2 = reader.ReadInt16();
            room.LightMode = reader.ReadInt16();
            ushort numLights = reader.ReadUInt16();
            room.Lights = new();
            for (int j = 0; j < numLights; j++)
            {
                room.Lights.Add(new()
                {
                    X = reader.ReadInt32(),
                    Y = reader.ReadInt32(),
                    Z = reader.ReadInt32(),
                    Intensity1 = reader.ReadUInt16(),
                    Intensity2 = reader.ReadUInt16(),
                    Fade1 = reader.ReadUInt32(),
                    Fade2 = reader.ReadUInt32()
                });
            }

            ushort numStaticMeshes = reader.ReadUInt16();
            room.StaticMeshes = new();
            for (int j = 0; j < numStaticMeshes; j++)
            {
                room.StaticMeshes.Add(new()
                {
                    X = reader.ReadInt32(),
                    Y = reader.ReadInt32(),
                    Z = reader.ReadInt32(),
                    Angle = reader.ReadInt16(),
                    Intensity1 = reader.ReadUInt16(),
                    Intensity2 = reader.ReadUInt16(),
                    ID = TR2Type.SceneryBase + reader.ReadUInt16()
                });
            }

            room.AlternateRoom = reader.ReadInt16();
            room.Flags = reader.ReadInt16();
        }

        uint numFloorData = reader.ReadUInt32();
        _level.FloorData = reader.ReadUInt16s(numFloorData).ToList();
    }

    private void WriteRooms(TRLevelWriter writer)
    {
        _spriteBuilder.CacheSpriteOffsets(_level.Sprites);

        writer.Write((ushort)_level.Rooms.Count);
        foreach (TR2Room room in _level.Rooms)
        {
            writer.Write(room.Info, _level.Version.Game);

            _roomBuilder.WriteMesh(writer, room.Mesh);

            writer.Write((ushort)room.Portals.Count);
            writer.Write(room.Portals);

            writer.Write(room.NumZSectors);
            writer.Write(room.NumXSectors);
            writer.Write(room.Sectors);

            writer.Write(room.AmbientIntensity);
            writer.Write(room.AmbientIntensity2);
            writer.Write(room.LightMode);

            writer.Write((ushort)room.Lights.Count);
            foreach (TR2RoomLight light in room.Lights)
            {
                writer.Write(light.X);
                writer.Write(light.Y);
                writer.Write(light.Z);
                writer.Write(light.Intensity1);
                writer.Write(light.Intensity2);
                writer.Write(light.Fade1);
                writer.Write(light.Fade2);
            }

            writer.Write((ushort)room.StaticMeshes.Count);
            foreach (TR2RoomStaticMesh mesh in room.StaticMeshes)
            {
                writer.Write(mesh.X);
                writer.Write(mesh.Y);
                writer.Write(mesh.Z);
                writer.Write(mesh.Angle);
                writer.Write(mesh.Intensity1);
                writer.Write(mesh.Intensity2);
                writer.Write((ushort)(mesh.ID - TR2Type.SceneryBase));
            }

            writer.Write(room.AlternateRoom);
            writer.Write(room.Flags);
        }

        writer.Write((uint)_level.FloorData.Count);
        writer.Write(_level.FloorData);
    }

    private void ReadMeshData(TRLevelReader reader)
    {
        _meshBuilder.BuildObjectMeshes(reader);
    }

    private void WriteMeshData(TRLevelWriter writer)
    {
        _meshBuilder.WriteObjectMeshes(writer, _level.Models.Values.SelectMany(m => m.Meshes), _level.StaticMeshes);
    }

    private void ReadModelData(TRLevelReader reader)
    {
        TRModelBuilder<TR2Type> builder = new(TRGameVersion.TR2, _observer);
        _level.Models = builder.ReadModelData(reader, _meshBuilder);
    }

    private void WriteModelData(TRLevelWriter writer)
    {
        TRModelBuilder<TR2Type> builder = new(TRGameVersion.TR2, _observer);
        builder.WriteModelData(writer, _level.Models);
    }

    private void ReadStaticMeshes(TRLevelReader reader)
    {
        _level.StaticMeshes = _meshBuilder.ReadStaticMeshes(reader, TR2Type.SceneryBase);
    }

    private void WriteStaticMeshes(TRLevelWriter writer)
    {
        _meshBuilder.WriteStaticMeshes(writer, _level.StaticMeshes, TR2Type.SceneryBase);
    }

    private void ReadSprites(TRLevelReader reader)
    {
        _level.Sprites = _spriteBuilder.ReadSprites(reader);

        for (int i = 0; i < _level.Rooms.Count; i++)
        {
            _level.Rooms[i].Mesh = _roomBuilder.BuildMesh(i);
        }
    }

    private void WriteSprites(TRLevelWriter writer)
    {
        _spriteBuilder.WriteSprites(writer, _level.Sprites);
    }

    private void ReadSoundEffects(TRLevelReader reader)
    {
        _level.SoundEffects = new();
        short[] soundMap = reader.ReadInt16s(Enum.GetValues<TR2SFX>().Length);

        uint numSoundDetails = reader.ReadUInt32();
        List<TR2SoundEffect> sfx = new();

        Dictionary<int, ushort> sampleMap = new();
        for (int i = 0; i < numSoundDetails; i++)
        {
            sampleMap[i] = reader.ReadUInt16();
            sfx.Add(new()
            {
                Volume = reader.ReadUInt16(),
                Chance = reader.ReadUInt16(),
                Samples = new()
            });

            sfx[i].SetFlags(reader.ReadUInt16());
        }

        uint numSampleIndices = reader.ReadUInt32();
        uint[] sampleIndices = reader.ReadUInt32s(numSampleIndices);

        foreach (int soundID in sampleMap.Keys)
        {
            TR2SoundEffect effect = sfx[soundID];
            ushort baseIndex = sampleMap[soundID];
            for (int i = 0; i < effect.Samples.Capacity; i++)
            {
                effect.Samples.Add(sampleIndices[baseIndex + i]);
            }
        }

        for (int i = 0; i < soundMap.Length; i++)
        {
            if (soundMap[i] < 0 || soundMap[i] >= sfx.Count)
            {
                continue;
            }

            _level.SoundEffects[(TR2SFX)i] = sfx[soundMap[i]];
        }
    }

    private void WriteSoundEffects(TRLevelWriter writer)
    {
        short detailsIndex = 0;
        foreach (TR2SFX id in Enum.GetValues<TR2SFX>())
        {
            writer.Write(_level.SoundEffects.ContainsKey(id) ? detailsIndex++ : (short)-1);
        }

        List<uint> samplePointers = new();
        foreach (var (_, effect) in _level.SoundEffects)
        {
            if (!samplePointers.Contains(effect.Samples.First()))
            {
                samplePointers.AddRange(effect.Samples);
            }
        }
        samplePointers.Sort();

        writer.Write((uint)_level.SoundEffects.Count);
        foreach (var (_, effect) in _level.SoundEffects)
        {
            uint firstSample = effect.Samples.First();
            writer.Write((ushort)samplePointers.IndexOf(firstSample));
            writer.Write(effect.Volume);
            writer.Write(effect.Chance);
            writer.Write(effect.GetFlags());
        }

        writer.Write((uint)samplePointers.Count);
        writer.Write(samplePointers);
    }
}
