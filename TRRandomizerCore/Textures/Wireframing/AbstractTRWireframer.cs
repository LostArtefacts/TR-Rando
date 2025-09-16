using RectanglePacker.Organisation;
using System.Drawing;
using TRDataControl;
using TRImageControl;
using TRImageControl.Packing;
using TRLevelControl.Model;

namespace TRRandomizerCore.Textures;

public abstract class AbstractTRWireframer<E, L>
    where E : Enum
    where L : TRLevelBase
{
    protected static readonly TRSize _nullSize = new(0, 0);
    protected static readonly int _ladderRungs = 4;
    protected static readonly List<FDTrigType> _highlightTriggerTypes = new()
    {
        FDTrigType.HeavyTrigger, FDTrigType.Pad
    };

    private Dictionary<TRFace, TRSize> _roomFace3s;
    private Dictionary<TRFace, TRSize> _roomFace4s;
    private Dictionary<TRFace, List<TRVertex>> _ladderFace4s;
    private List<TRFace> _triggerFaces, _deathFaces;

    private Dictionary<TRMeshFace, TRSize> _meshFaces;

    private ISet<ushort> _allTextures;
    protected WireframeData<E> _data;

    protected virtual bool IsTextureExcluded(ushort texture)
    {
        return _data.ExcludedTextures.Contains(texture) || _data.DeathTextures.Contains(texture);
    }

    protected virtual bool IsTextureOverriden(ushort texture)
    {
        return _data.ForcedOverrides.Contains(texture);
    }

    public void Apply(L level, WireframeData<E> data)
    {
        _roomFace3s = new();
        _roomFace4s = new();
        _meshFaces = new();
        _ladderFace4s = data.HighlightLadders ? CollectLadders(level) : new();
        _triggerFaces = data.HighlightTriggers ? CollectTriggerFaces(level, _highlightTriggerTypes) : new();
        _deathFaces = data.HighlightDeathTiles ? CollectDeathFaces(level) : new();
        _allTextures = new SortedSet<ushort>();
        _data = data;

        RetainCustomTextures(level);
        ScanRooms(level);
        ScanMeshes(level);

        _triggerFaces.RemoveAll(f => IsTextureExcluded(f.Texture));
        _deathFaces.RemoveAll(f => IsTextureExcluded(f.Texture));

        SortedSet<TRSize> roomSizes = new(_roomFace3s.Values.Concat(_roomFace4s.Values));
        SortedSet<TRSize> meshSizes = new(_meshFaces.Values);
        
        DeleteAnimatedTextures(level);

        TRTexturePacker packer = CreatePacker(level);
        DeleteTextures(packer);
        Queue<int> textureSlots = new(level.GetFreeTextureSlots());

        TRSize roomSize = GetLargestSize(roomSizes);
        roomSize.RoundDown();

        TRTextileSegment roomTexture = CreateWireframe(packer, roomSize, _data.HighlightColour);
        TRTextileSegment ladderTexture = CreateLadderWireframe(packer, roomSize, _data.HighlightColour);
        TRTextileSegment triggerTexture = CreateTriggerWireframe(packer, roomSize, _data.TriggerColour);
        TRTextileSegment deathTexture = CreateDeathWireframe(packer, roomSize, _data.DeathColour);
        Dictionary<ushort, TRTextileSegment> specialTextures = CreateSpecialTextures(packer, level, _data.HighlightColour);
        ProcessClips(packer, level, _data.HighlightColour);

        Dictionary<TRSize, TRTextileSegment> modelRemap = new();
        foreach (TRSize size in meshSizes)
        {
            modelRemap[size] = CreateWireframe(packer, size, _data.HighlightColour);
        }

        packer.Options.StartMethod = PackingStartMethod.FirstTile;
        packer.Pack(true);

        ushort StoreTexture(TRTextileSegment segment)
        {
            if (textureSlots.TryDequeue(out int slot))
            {
                level.ObjectTextures[slot] = segment.Texture as TRObjectTexture;
            }
            else
            {
                slot = level.ObjectTextures.Count;
                level.ObjectTextures.Add(segment.Texture as TRObjectTexture);
            }

            return (ushort)slot;
        }

        ushort roomTextureIndex = StoreTexture(roomTexture);
        ushort ladderTextureIndex = StoreTexture(ladderTexture);
        ushort triggerTextureIndex = StoreTexture(triggerTexture);
        ushort deathTextureIndex = StoreTexture(deathTexture);

        Dictionary<ushort, ushort> specialTextureRemap = new();
        foreach (ushort originalTexture in specialTextures.Keys)
        {
            specialTextureRemap[originalTexture] = StoreTexture(specialTextures[originalTexture]);
        }

        foreach (TRSize size in modelRemap.Keys)
        {
            if (!size.Equals(_nullSize))
            {
                modelRemap[size].Index = StoreTexture(modelRemap[size]);
            }
        }

        ResetRoomTextures(roomTextureIndex, ladderTextureIndex, triggerTextureIndex, deathTextureIndex, specialTextureRemap);
        ResetMeshTextures(modelRemap, specialTextureRemap);
        TidyModels(level);
        SetSkyboxVisible(level);
        level.ResetUnusedTextures();
    }

    private void RetainCustomTextures(L level)
    {
        for (ushort i = 0; i < level.ObjectTextures.Count; i++)
        {
            if (level.ObjectTextures[i].BlendingMode == TRBlendingMode.Unused01)
            {
                _data.ExcludedTextures.Add(i);
            }
        }
    }

    private void ScanRooms(L level)
    {
        foreach (IEnumerable<TRFace> roomRects in GetRoomFace4s(level))
        {
            ScanRoomFace4s(level, roomRects);
        }
        foreach (IEnumerable<TRFace> roomTris in GetRoomFace3s(level))
        {
            ScanRoomFace3s(level, roomTris);
        }
    }

    private void ScanRoomFace4s(L level, IEnumerable<TRFace> faces)
    {
        foreach (TRFace face in faces)
        {
            if (_ladderFace4s.ContainsKey(face) || _triggerFaces.Contains(face) || _deathFaces.Contains(face))
                continue;

            if (!IsTextureExcluded(face.Texture) || (_data.DeathTextures.Contains(face.Texture) && !_deathFaces.Contains(face)))
            {
                _roomFace4s[face] = GetTextureSize(level, face.Texture);
            }
        }
    }

    private void ScanRoomFace3s(L level, IEnumerable<TRFace> faces)
    {
        foreach (TRFace face in faces)
        {
            if (!IsTextureExcluded(face.Texture) || _data.DeathTextures.Contains(face.Texture))
            {
                _roomFace3s[face] = GetTextureSize(level, face.Texture);
            }
        }
    }

    private void ScanMeshes(L level)
    {
        foreach (TRMesh mesh in level.DistinctMeshes)
        {
            ScanMesh(level, mesh);
        }
    }

    private void ScanMesh(L level, TRMesh mesh)
    {
        foreach (TRMeshFace face in mesh.TexturedFaces)
        {
            _meshFaces[face] = GetTextureSize(level, face.Texture);
        }
    }

    private TRSize GetTextureSize(L level, ushort textureIndex)
    {
        TRObjectTexture texture = level.ObjectTextures[textureIndex];
        _allTextures.Add(textureIndex);
        TRTextileSegment itext = new()
        {
            Texture = texture
        };
        return new(itext.Bounds.Width, itext.Bounds.Height);
    }

    private void DeleteTextures(TRTexturePacker packer)
    {
        List<int> textures = new();
        foreach (ushort t in _allTextures)
        {
            if (!IsTextureExcluded(t) && !IsTextureOverriden(t))
            {
                textures.Add(t);
            }
        }

        packer.RemoveObjectRegions(textures);
    }

    private static TRSize GetLargestSize(IEnumerable<TRSize> sizes)
    {
        List<TRSize> compSizes = new(sizes);
        if (compSizes.Count > 0)
        {
            compSizes.Sort();
            compSizes.Reverse();

            return compSizes[0];
        }
        return _nullSize;
    }

    private TRTextileSegment CreateWireframe(TRTexturePacker packer, TRSize size, Color colour)
    {
        if (size.Equals(_nullSize))
        {
            return null;
        }

        TRTextileSegment texture = CreateSegment(new(0, 0, size.W, size.H));
        TRImage frame = CreateFrame(size.W, size.H, colour, true);

        packer.AddRectangle(new(texture, frame));

        return texture;
    }

    private TRTextileSegment CreateLadderWireframe(TRTexturePacker packer, TRSize size, Color colour)
    {
        if (size.Equals(_nullSize))
        {
            return null;
        }

        TRTextileSegment texture = CreateSegment(new(0, 0, size.W, size.H));
        TRImage frame = CreateFrame(size.W, size.H, colour, false);

        int rungSplit = size.H / _ladderRungs;
        for (int i = 0; i < _ladderRungs; i++)
        {
            int y = i * rungSplit;
            // Horizontal bar for the rung
            frame.DrawLine(colour, 0, y, size.W, y, 2);
            // Diagonal bar to the next rung
            frame.DrawLine(colour, 0, y, size.W, y + rungSplit);
        }

        packer.AddRectangle(new(texture, frame));

        return texture;
    }

    private TRTextileSegment CreateTriggerWireframe(TRTexturePacker packer, TRSize size, Color colour)
    {
        if (size.Equals(_nullSize))
        {
            return null;
        }

        TRTextileSegment texture = CreateSegment(new(0, 0, size.W, size.H));
        TRImage frame = CreateFrame(size.W, size.H, colour, true);

        // X marks the spot
        frame.DrawLine(colour, 0, size.H, size.W, 0);

        packer.AddRectangle(new(texture, frame));

        return texture;
    }

    private TRTextileSegment CreateDeathWireframe(TRTexturePacker packer, TRSize size, Color colour)
    {
        if (size.Equals(_nullSize))
        {
            return null;
        }

        TRTextileSegment texture = CreateSegment(new(0, 0, size.W, size.H));
        TRImage frame = CreateFrame(size.W, size.H, colour, true);

        // Star symbol
        frame.DrawLine(colour, 0, size.H, size.W, 0);
        frame.DrawLine(colour, size.W / 2, 0, size.W / 2, size.H);
        frame.DrawLine(colour, 0, size.H / 2, size.W, size.H / 2);

        packer.AddRectangle(new(texture, frame));

        return texture;
    }

    private Dictionary<ushort, TRTextileSegment> CreateSpecialTextures(TRTexturePacker packer, L level, Color colour)
    {
        Dictionary<ushort, TRTextileRegion> specialSegments = CreateSpecialSegments(level, colour);
        Dictionary<ushort, TRTextileSegment> specialTextures = new();
        foreach (ushort textureIndex in specialSegments.Keys)
        {
            packer.AddRectangle(specialSegments[textureIndex]);
            specialTextures[textureIndex] = specialSegments[textureIndex].Segments.First();
        }
        return specialTextures;
    }

    protected virtual Dictionary<ushort, TRTextileRegion> CreateSpecialSegments(L level, Color colour)
        => new();

    private void ProcessClips(TRTexturePacker packer, L level, Color colour)
    {
        // Some animated textures are shared in segments e.g. 4 32x32 segments within a 64x64 container,
        // so in instances where we only want to wireframe a section of these, we use manual clipping.
        foreach (WireframeClip clip in _data.ManualClips)
        {
            TRImage frame = CreateFrame(clip.Clip.Width, clip.Clip.Height, colour, true);

            foreach (ushort texture in clip.Textures)
            {
                TRTextileSegment indexedTexture = new()
                {
                    Index = texture,
                    Texture = level.ObjectTextures[texture]
                };
                TRImage bmp = packer.Tiles[indexedTexture.Atlas].Image;

                List<TRTextileRegion> segments = packer.Tiles[indexedTexture.Atlas].GetObjectRegions(new int[] { texture });
                foreach (TRTextileRegion segment in segments)
                {
                    bmp.Import(frame, new
                    (
                        segment.Bounds.X + clip.Clip.X, 
                        segment.Bounds.Y + clip.Clip.Y
                    ));
                }
            }
        }

        // Ensure these clipped textures support transparency
        foreach (TRObjectTexture texture in level.ObjectTextures)
        {
            if (texture.BlendingMode == TRBlendingMode.Opaque)
            {
                texture.BlendingMode = TRBlendingMode.AlphaTesting;
            }
        }
    }

    protected TRImage CreateFrame(int width, int height, Color colour, bool addDiagonal)
    {
        TRImage image = new(width, height);
        image.DrawRectangle(colour, 0, 0, width - 1, height - 1);
        image.DrawRectangle(colour, 0, 0, width - 2, height - 2);

        if (addDiagonal)
        {
            image.DrawLine(colour, 0, 0, width, height, 2);
        }
        return image;
    }

    protected TRTextileSegment CreateSegment(Rectangle rectangle)
    {
        return new()
        {
            Texture = new TRObjectTexture(rectangle)
            {
                BlendingMode = TRBlendingMode.AlphaTesting
            }
        };
    }

    private void ResetRoomTextures(ushort wireframeIndex, ushort ladderIndex, ushort triggerIndex, ushort deathIndex, Dictionary<ushort, ushort> specialTextureRemap)
    {
        foreach (TRFace face in _roomFace3s.Keys)
        {
            face.Texture = specialTextureRemap.ContainsKey(face.Texture)
                ? specialTextureRemap[face.Texture]
                : wireframeIndex;
        }

        foreach (TRFace face in _roomFace4s.Keys)
        {
            if (!_ladderFace4s.ContainsKey(face) && !_triggerFaces.Contains(face) && !_deathFaces.Contains(face))
            {
                face.Texture = specialTextureRemap.ContainsKey(face.Texture)
                    ? specialTextureRemap[face.Texture]
                    : wireframeIndex;
            }
        }

        foreach (TRFace face in _ladderFace4s.Keys)
        {
            face.Texture = ladderIndex;

            // Ensure the ladder isn't sideways - if the first two vertices don't have
            // the same Y val and it's a wall, rotate the face once.
            List<TRVertex> vertices = _ladderFace4s[face];
            if (vertices.Count > 1 &&
                vertices[0].Y != vertices[1].Y &&
                (vertices.All(v => v.X == vertices[0].X) || vertices.All(v => v.Z == vertices[0].Z)))
            {
                Queue<ushort> vertIndices = new(face.Vertices);
                vertIndices.Enqueue(vertIndices.Dequeue());
                face.Vertices = new(vertIndices);
            }
        }

        foreach (TRFace face in _triggerFaces)
        {
            face.Texture = triggerIndex;
        }

        foreach (TRFace face in _deathFaces)
        {
            face.Texture = deathIndex;
        }
    }

    private void ResetMeshTextures(Dictionary<TRSize, TRTextileSegment> sizeRemap, Dictionary<ushort, ushort> specialTextureRemap)
    {
        foreach (TRMeshFace face in _meshFaces.Keys)
        {
            if (IsTextureExcluded(face.Texture))
            {
                continue;
            }
            if (specialTextureRemap.ContainsKey(face.Texture))
            {
                face.Texture = specialTextureRemap[face.Texture];
            }
            else
            {
                TRSize size = _meshFaces[face];
                if (!size.Equals(_nullSize))
                {
                    if (!sizeRemap.ContainsKey(size))
                    {
                        size = Find(size, sizeRemap);
                    }
                    face.Texture = (ushort)sizeRemap[size].Index;
                }
            }
        }
    }

    private static TRSize Find(TRSize s, Dictionary<TRSize, TRTextileSegment> map)
    {
        foreach (TRSize size in map.Keys)
        {
            if (s.Equals(size))
            {
                return size;
            }
        }
        return s;
    }

    private void TidyModels(L level)
    {
        int blackIndex = GetBlackPaletteIndex(level);

        // For most meshes, replace any colours with the default background
        foreach (TRMesh mesh in level.DistinctMeshes)
        {
            SetFace4Colours(mesh.ColouredRectangles, blackIndex);
            SetFace3Colours(mesh.ColouredTriangles, blackIndex);
        }

        HashSet<TRMesh> processedModelMeshes = new();
        foreach (var (type, model) in GetModels(level))
        {
            if (IsSkybox(type))
            {
                // Solidify the skybox as it will become the backdrop for every room
                foreach (TRMesh mesh in model.Meshes)
                {
                    foreach (TRMeshFace rect in mesh.TexturedRectangles)
                    {
                        rect.Texture = mesh.ColouredTriangles[0].Texture;
                        mesh.ColouredRectangles.Add(rect);
                    }
                    mesh.TexturedRectangles.Clear();
                }
            }
            else if
            (
                (_data.SolidLara && IsLaraModel(type)) ||
                (_data.SolidEnemies && (IsEnemyModel(type) || _data.SolidModels.Contains(type)) && !IsEnemyPlaceholderModel(type)) ||
                (_data.SolidInteractables && IsInteractableModel(type)) ||
                ShouldSolidifyModel(type)
            )
            {
                int paletteIndex = ImportColour(level, !IsLaraModel(type) && _data.ModelColours.ContainsKey(type) ?
                    _data.ModelColours[type] :
                    _data.HighlightColour);

                if (paletteIndex == -1)
                {
                    paletteIndex = blackIndex;
                }

                foreach (TRMesh mesh in model.Meshes)
                {
                    if (processedModelMeshes.Add(mesh))
                    {
                        // Convert all textured polygons to coloured ones, and reset the
                        // palette index they point to.
                        mesh.ColouredRectangles.AddRange(mesh.TexturedRectangles);
                        mesh.ColouredTriangles.AddRange(mesh.TexturedTriangles);

                        SetFace4Colours(mesh.ColouredRectangles, paletteIndex);
                        SetFace3Colours(mesh.ColouredTriangles, paletteIndex);

                        mesh.TexturedRectangles.Clear();
                        mesh.TexturedTriangles.Clear();
                    }
                }
            }
        }

        // In case we have imported any colours
        ResetPaletteTracking(level);
    }

    private void SetFace4Colours(IEnumerable<TRMeshFace> faces, int colourIndex)
    {
        foreach (TRMeshFace face in faces)
        {
            face.Texture = (ushort)(Is8BitPalette ? colourIndex : (colourIndex << 8 | (face.Texture & 0xFF)));
        }
    }

    private void SetFace3Colours(IEnumerable<TRMeshFace> faces, int colourIndex)
    {
        foreach (TRMeshFace face in faces)
        {
            face.Texture = (ushort)(Is8BitPalette ? colourIndex : (colourIndex << 8 | (face.Texture & 0xFF)));
        }
    }

    private void DeleteAnimatedTextures(L level)
    {
        for (int i = level.AnimatedTextures.Count - 1; i >= 0; i--)
        {
            TRAnimatedTexture animatedTexture = level.AnimatedTextures[i];
            for (int j = animatedTexture.Textures.Count - 1; j >= 0; j--)
            {
                if (!IsTextureExcluded(animatedTexture.Textures[j]))
                {
                    animatedTexture.Textures.RemoveAt(j);
                }
            }

            if (animatedTexture.Textures.Count < 2)
            {
                level.AnimatedTextures.RemoveAt(i);
            }
        }
    }

    protected abstract Dictionary<TRFace, List<TRVertex>> CollectLadders(L level);
    protected abstract List<TRFace> CollectTriggerFaces(L level, List<FDTrigType> triggerTypes);
    protected abstract List<TRFace> CollectDeathFaces(L level);
    protected abstract TRTexturePacker CreatePacker(L level);
    protected abstract IEnumerable<IEnumerable<TRFace>> GetRoomFace4s(L level);
    protected abstract IEnumerable<IEnumerable<TRFace>> GetRoomFace3s(L level);
    protected abstract int GetBlackPaletteIndex(L level);
    protected abstract int ImportColour(L level, Color c);
    protected abstract TRDictionary<E, TRModel> GetModels(L level);
    protected abstract bool IsSkybox(E type);
    protected abstract bool IsLaraModel(E type);
    protected abstract bool IsEnemyModel(E type);
    protected virtual bool IsEnemyPlaceholderModel(E type) => false;
    protected abstract bool IsInteractableModel(E type);
    protected virtual bool ShouldSolidifyModel(E type) => false;
    protected abstract void SetSkyboxVisible(L level);

    public virtual bool Is8BitPalette { get; }
    protected virtual void ResetPaletteTracking(L level) { }
}
