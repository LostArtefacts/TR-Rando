using System.Diagnostics;
using TRLevelControl.Build;
using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRLevelControl;

public class TR3LevelControl : TRLevelControlBase<TR3Level>
{
    private readonly TRObjectMeshBuilder<TR3Type> _meshBuilder;
    private readonly TRSpriteBuilder<TR3Type> _spriteBuilder;

    public TR3LevelControl(ITRLevelObserver observer = null)
        : base(observer)
    {
        _meshBuilder = new(TRGameVersion.TR3, _observer);
        _spriteBuilder = new(TRGameVersion.TR3);
    }

    protected override TR3Level CreateLevel(TRFileVersion version)
    {
        TR3Level level = new()
        {
            Version = new()
            {
                Game = TRGameVersion.TR3,
                File = version
            }
        };

        TestVersion(level, TRFileVersion.TR3a, TRFileVersion.TR3b);
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

        //Rooms
        ushort numRooms = reader.ReadUInt16();
        _level.Rooms = new();
        for (int i = 0; i < numRooms; i++)
        {
            TR3Room room = new()
            {
                //Grab info
                Info = new TRRoomInfo
                {
                    X = reader.ReadInt32(),
                    Z = reader.ReadInt32(),
                    YBottom = reader.ReadInt32(),
                    YTop = reader.ReadInt32()
                },
            };
            _level.Rooms.Add(room);

            uint numWords = reader.ReadUInt32();
            room.RoomData = ConvertToRoomData(reader.ReadUInt16s(numWords));

            //Portals
            ushort numPortals = reader.ReadUInt16();
            room.Portals = new();
            for (int j = 0; j < numPortals; j++)
            {
                room.Portals.Add(TR2FileReadUtilities.ReadRoomPortal(reader));
            }

            //Sectors
            room.NumZSectors = reader.ReadUInt16();
            room.NumXSectors = reader.ReadUInt16();
            room.Sectors = new();
            for (int j = 0; j < (room.NumXSectors * room.NumZSectors); j++)
            {
                room.Sectors.Add(TR2FileReadUtilities.ReadRoomSector(reader));
            }

            //Lighting
            room.AmbientIntensity = reader.ReadInt16();
            room.LightMode = reader.ReadInt16();
            ushort numLights = reader.ReadUInt16();
            room.Lights = new();
            for (int j = 0; j < numLights; j++)
            {
                room.Lights.Add(TR3FileReadUtilities.ReadRoomLight(reader));
            }

            //Static meshes
            ushort numStaticMeshes = reader.ReadUInt16();
            room.StaticMeshes = new();
            for (int j = 0; j < numStaticMeshes; j++)
            {
                room.StaticMeshes.Add(TR3FileReadUtilities.ReadRoomStaticMesh(reader));
            }

            room.AlternateRoom = reader.ReadInt16();
            room.Flags = reader.ReadInt16();

            //New TR3 room info
            room.WaterScheme = reader.ReadByte();
            room.ReverbInfo = reader.ReadByte();
            room.Filler = reader.ReadByte();
        }

        uint numFloorData = reader.ReadUInt32();
        _level.FloorData = reader.ReadUInt16s(numFloorData).ToList();

        ReadMeshData(reader);
        ReadModelData(reader);

        ReadStaticMeshes(reader);

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

        //Object Textures - in TR3 this is now after animated textures
        uint numObjectTextures = reader.ReadUInt32();
        _level.ObjectTextures = new();
        for (int i = 0; i < numObjectTextures; i++)
        {
            _level.ObjectTextures.Add(TR2FileReadUtilities.ReadObjectTexture(reader));
        }

        //Entities
        uint numEntities = reader.ReadUInt32();
        _level.Entities = reader.ReadTR3Entities(numEntities);

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

        _spriteBuilder.CacheSpriteOffsets(_level.Sprites);
        writer.Write((ushort)_level.Rooms.Count);
        foreach (TR3Room room in _level.Rooms) { writer.Write(room.Serialize()); }

        writer.Write((uint)_level.FloorData.Count);
        writer.Write(_level.FloorData);

        WriteMeshData(writer);
        WriteModelData(writer);

        WriteStaticMeshes(writer);

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

        writer.Write((uint)_level.ObjectTextures.Count);
        foreach (TRObjectTexture tex in _level.ObjectTextures) { writer.Write(tex.Serialize()); }

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
        TRModelBuilder<TR3Type> builder = new(TRGameVersion.TR3, _observer);
        _level.Models = builder.ReadModelData(reader, _meshBuilder);
    }

    private void WriteModelData(TRLevelWriter writer)
    {
        TRModelBuilder<TR3Type> builder = new(TRGameVersion.TR3, _observer);
        builder.WriteModelData(writer, _level.Models);
    }

    private void ReadStaticMeshes(TRLevelReader reader)
    {
        _level.StaticMeshes = _meshBuilder.ReadStaticMeshes(reader, TR3Type.SceneryBase);
    }

    private void WriteStaticMeshes(TRLevelWriter writer)
    {
        _meshBuilder.WriteStaticMeshes(writer, _level.StaticMeshes, TR3Type.SceneryBase);
    }

    private void ReadSprites(TRLevelReader reader)
    {
        _level.Sprites = _spriteBuilder.ReadSprites(reader);
    }

    private void WriteSprites(TRLevelWriter writer)
    {
        _spriteBuilder.WriteSprites(writer, _level.Sprites);
    }

    private static TR3RoomData ConvertToRoomData(ushort[] rawData)
    {
        int offset = 0;

        //Grab detailed room data
        TR3RoomData roomData = new()
        {
            //Room vertices
            NumVertices = UnsafeConversions.UShortToShort(rawData[offset])
        };
        roomData.Vertices = new TR3RoomVertex[roomData.NumVertices];

        offset++;

        for (int j = 0; j < roomData.NumVertices; j++)
        {
            TR3RoomVertex vertex = new()
            {
                Vertex = new TRVertex()
            };

            vertex.Vertex.X = UnsafeConversions.UShortToShort(rawData[offset]);
            offset++;
            vertex.Vertex.Y = UnsafeConversions.UShortToShort(rawData[offset]);
            offset++;
            vertex.Vertex.Z = UnsafeConversions.UShortToShort(rawData[offset]);
            offset++;
            vertex.Lighting = UnsafeConversions.UShortToShort(rawData[offset]);
            offset++;
            vertex.Attributes = rawData[offset];
            offset++;
            vertex.Colour = rawData[offset];
            offset++;

            roomData.Vertices[j] = vertex;
        }

        //Room rectangles
        roomData.NumRectangles = UnsafeConversions.UShortToShort(rawData[offset]);
        roomData.Rectangles = new TRFace4[roomData.NumRectangles];

        offset++;

        for (int j = 0; j < roomData.NumRectangles; j++)
        {
            TRFace4 face = new()
            {
                Vertices = new ushort[4]
            };
            face.Vertices[0] = rawData[offset];
            offset++;
            face.Vertices[1] = rawData[offset];
            offset++;
            face.Vertices[2] = rawData[offset];
            offset++;
            face.Vertices[3] = rawData[offset];
            offset++;
            face.Texture = rawData[offset];
            offset++;

            roomData.Rectangles[j] = face;
        }

        //Room triangles
        roomData.NumTriangles = UnsafeConversions.UShortToShort(rawData[offset]);
        roomData.Triangles = new TRFace3[roomData.NumTriangles];

        offset++;

        for (int j = 0; j < roomData.NumTriangles; j++)
        {
            TRFace3 face = new()
            {
                Vertices = new ushort[3]
            };
            face.Vertices[0] = rawData[offset];
            offset++;
            face.Vertices[1] = rawData[offset];
            offset++;
            face.Vertices[2] = rawData[offset];
            offset++;
            face.Texture = rawData[offset];
            offset++;

            roomData.Triangles[j] = face;
        }

        //Room sprites
        roomData.NumSprites = UnsafeConversions.UShortToShort(rawData[offset]);
        roomData.Sprites = new TRRoomSprite[roomData.NumSprites];

        offset++;

        for (int j = 0; j < roomData.NumSprites; j++)
        {
            TRRoomSprite face = new()
            {
                Vertex = UnsafeConversions.UShortToShort(rawData[offset])
            };
            offset++;
            face.Texture = UnsafeConversions.UShortToShort(rawData[offset]);
            offset++;

            roomData.Sprites[j] = face;
        }

        Debug.Assert(offset == rawData.Length);

        return roomData;
    }

    private void ReadSoundEffects(TRLevelReader reader)
    {
        _level.SoundEffects = new();
        short[] soundMap = reader.ReadInt16s(Enum.GetValues<TR3SFX>().Length);

        uint numSoundDetails = reader.ReadUInt32();
        List<TR3SoundEffect> sfx = new();

        Dictionary<int, ushort> sampleMap = new();
        for (int i = 0; i < numSoundDetails; i++)
        {
            sampleMap[i] = reader.ReadUInt16();
            sfx.Add(new()
            {
                Volume = reader.ReadByte(),
                Range = reader.ReadByte(),
                Chance = reader.ReadByte(),
                Pitch = reader.ReadByte(),
                Samples = new()
            });

            sfx[i].SetFlags(reader.ReadUInt16());
        }

        uint numSampleIndices = reader.ReadUInt32();
        uint[] sampleIndices = reader.ReadUInt32s(numSampleIndices);

        foreach (int soundID in sampleMap.Keys)
        {
            TR3SoundEffect effect = sfx[soundID];
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

            _level.SoundEffects[(TR3SFX)i] = sfx[soundMap[i]];
        }
    }

    private void WriteSoundEffects(TRLevelWriter writer)
    {
        short detailsIndex = 0;
        foreach (TR3SFX id in Enum.GetValues<TR3SFX>())
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
            writer.Write(effect.Range);
            writer.Write(effect.Chance);
            writer.Write(effect.Pitch);
            writer.Write(effect.GetFlags());
        }

        writer.Write((uint)samplePointers.Count);
        writer.Write(samplePointers);
    }
}
