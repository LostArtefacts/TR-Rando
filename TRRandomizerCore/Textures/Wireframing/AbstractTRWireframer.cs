using RectanglePacker.Organisation;
using System.Drawing;
using System.Drawing.Drawing2D;
using TRImageControl.Helpers;
using TRImageControl.Packing;
using TRLevelControl.Model;
using TRModelTransporter.Helpers;

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

        ISet<TRSize> roomSizes = new SortedSet<TRSize>(_roomFace3s.Values.Concat(_roomFace4s.Values));
        ISet<TRSize> meshSizes = new SortedSet<TRSize>(_meshFaces.Values);

        Pen roomPen = new(_data.HighlightColour, 1)
        {
            Alignment = PenAlignment.Inset,
            DashCap = DashCap.Round
        };            
        Pen modelPen = new(_data.HighlightColour, 1)
        {
            Alignment = PenAlignment.Inset
        };
        Pen triggerPen = new(_data.TriggerColour, 1)
        {
            Alignment = PenAlignment.Inset,
            DashCap = DashCap.Round
        };
        Pen deathPen = new(_data.DeathColour, 1)
        {
            Alignment = PenAlignment.Inset,
            DashCap = DashCap.Round
        };

        using AbstractTexturePacker<E, L> packer = CreatePacker(level);
        DeleteTextures(packer);
        ResetUnusedTextures(level);

        TRSize roomSize = GetLargestSize(roomSizes);
        roomSize.RoundDown();

        IndexedTRObjectTexture roomTexture = CreateWireframe(packer, roomSize, roomPen, SmoothingMode.AntiAlias);
        IndexedTRObjectTexture ladderTexture = CreateLadderWireframe(packer, roomSize, roomPen, SmoothingMode.AntiAlias);
        IndexedTRObjectTexture triggerTexture = CreateTriggerWireframe(packer, roomSize, triggerPen, SmoothingMode.AntiAlias);
        IndexedTRObjectTexture deathTexture = CreateDeathWireframe(packer, roomSize, deathPen, SmoothingMode.AntiAlias);
        Dictionary<ushort, IndexedTRObjectTexture> specialTextures = CreateSpecialTextures(packer, level, roomPen);
        ProcessClips(packer, level, roomPen, SmoothingMode.AntiAlias);

        Dictionary<TRSize, IndexedTRObjectTexture> modelRemap = new();
        foreach (TRSize size in meshSizes)
        {
            modelRemap[size] = CreateWireframe(packer, size, modelPen, SmoothingMode.None);
        }

        packer.Options.StartMethod = PackingStartMethod.FirstTile;
        packer.Pack(true);

        Queue<int> reusableTextures = new(GetInvalidObjectTextureIndices(level));
        List<TRObjectTexture> levelObjectTextures = GetObjectTextures(level);

        ushort roomTextureIndex = (ushort)reusableTextures.Dequeue();
        levelObjectTextures[roomTextureIndex] = roomTexture.Texture;

        ushort ladderTextureIndex = (ushort)reusableTextures.Dequeue();
        levelObjectTextures[ladderTextureIndex] = ladderTexture.Texture;

        ushort triggerTextureIndex = (ushort)reusableTextures.Dequeue();
        levelObjectTextures[triggerTextureIndex] = triggerTexture.Texture;

        ushort deathTextureIndex = (ushort)reusableTextures.Dequeue();
        levelObjectTextures[deathTextureIndex] = deathTexture.Texture;

        Dictionary<ushort, ushort> specialTextureRemap = new();
        foreach (ushort originalTexture in specialTextures.Keys)
        {
            ushort newIndex = (ushort)reusableTextures.Dequeue();
            levelObjectTextures[newIndex] = specialTextures[originalTexture].Texture;
            specialTextureRemap[originalTexture] = newIndex;
        }

        foreach (TRSize size in modelRemap.Keys)
        {
            if (!size.Equals(_nullSize))
            {
                ushort texture = (ushort)reusableTextures.Dequeue();
                levelObjectTextures[texture] = modelRemap[size].Texture;
                modelRemap[size].Index = texture;
            }
        }

        ResetRoomTextures(roomTextureIndex, ladderTextureIndex, triggerTextureIndex, deathTextureIndex, specialTextureRemap);
        ResetMeshTextures(modelRemap, specialTextureRemap);
        TidyModels(level);
        SetSkyboxVisible(level);
        DeleteAnimatedTextures(level);
    }

    private void RetainCustomTextures(L level)
    {
        List<TRObjectTexture> textures = GetObjectTextures(level);
        for (ushort i = 0; i < textures.Count; i++)
        {
            if (textures[i].BlendingMode == TRBlendingMode.Unused01)
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
        TRObjectTexture texture = GetObjectTextures(level)[textureIndex];
        if (texture.IsValid())
        {
            _allTextures.Add(textureIndex);
            IndexedTRObjectTexture itext = new()
            {
                Texture = texture
            };
            return new TRSize(itext.Bounds.Width, itext.Bounds.Height);
        }

        return _nullSize;
    }

    private void DeleteTextures(AbstractTexturePacker<E, L> packer)
    {
        List<int> textures = new();
        foreach (ushort t in _allTextures)
        {
            if (!IsTextureExcluded(t) && !IsTextureOverriden(t))
            {
                textures.Add(t);
            }
        }

        packer.RemoveObjectTextureSegments(textures);
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

    private IndexedTRObjectTexture CreateWireframe(AbstractTexturePacker<E, L> packer, TRSize size, Pen pen, SmoothingMode mode)
    {
        if (size.Equals(_nullSize))
        {
            return null;
        }

        IndexedTRObjectTexture texture = CreateTexture(new Rectangle(0, 0, size.W, size.H));
        BitmapGraphics frame = CreateFrame(size.W, size.H, pen, mode, true);

        packer.AddRectangle(new TexturedTileSegment(texture, frame.Bitmap));

        return texture;
    }

    private IndexedTRObjectTexture CreateLadderWireframe(AbstractTexturePacker<E, L> packer, TRSize size, Pen pen, SmoothingMode mode)
    {
        if (size.Equals(_nullSize))
        {
            return null;
        }

        IndexedTRObjectTexture texture = CreateTexture(new Rectangle(0, 0, size.W, size.H));
        BitmapGraphics frame = CreateFrame(size.W, size.H, pen, mode, false);

        int rungSplit = size.H / _ladderRungs;
        for (int i = 0; i < _ladderRungs; i++)
        {
            int y = i * rungSplit;
            // Horizontal bar for the rung
            frame.Graphics.DrawLine(pen, 0, y, size.W, y);
            // Diagonal bar to the next rung
            frame.Graphics.DrawLine(pen, 0, y, size.W, y + rungSplit);
        }

        packer.AddRectangle(new TexturedTileSegment(texture, frame.Bitmap));

        return texture;
    }

    private IndexedTRObjectTexture CreateTriggerWireframe(AbstractTexturePacker<E, L> packer, TRSize size, Pen pen, SmoothingMode mode)
    {
        if (size.Equals(_nullSize))
        {
            return null;
        }

        IndexedTRObjectTexture texture = CreateTexture(new Rectangle(0, 0, size.W, size.H));
        BitmapGraphics frame = CreateFrame(size.W, size.H, pen, mode, true);
        // X marks the spot
        frame.Graphics.DrawLine(pen, 0, size.H, size.W, 0);

        packer.AddRectangle(new TexturedTileSegment(texture, frame.Bitmap));

        return texture;
    }

    private IndexedTRObjectTexture CreateDeathWireframe(AbstractTexturePacker<E, L> packer, TRSize size, Pen pen, SmoothingMode mode)
    {
        if (size.Equals(_nullSize))
        {
            return null;
        }

        IndexedTRObjectTexture texture = CreateTexture(new Rectangle(0, 0, size.W, size.H));
        BitmapGraphics frame = CreateFrame(size.W, size.H, pen, mode, true);
        // Star symbol
        frame.Graphics.DrawLine(pen, 0, size.H, size.W, 0);
        frame.Graphics.DrawLine(pen, size.W / 2, 0, size.W / 2, size.H);
        frame.Graphics.DrawLine(pen, 0, size.H / 2, size.W, size.H / 2);

        packer.AddRectangle(new TexturedTileSegment(texture, frame.Bitmap));

        return texture;
    }

    private Dictionary<ushort, IndexedTRObjectTexture> CreateSpecialTextures(AbstractTexturePacker<E, L> packer, L level, Pen pen)
    {
        Dictionary<ushort, TexturedTileSegment> specialSegments = CreateSpecialSegments(level, pen);
        Dictionary<ushort, IndexedTRObjectTexture> specialTextures = new();
        foreach (ushort textureIndex in specialSegments.Keys)
        {
            packer.AddRectangle(specialSegments[textureIndex]);
            specialTextures[textureIndex] = specialSegments[textureIndex].FirstTexture as IndexedTRObjectTexture;
        }
        return specialTextures;
    }

    protected virtual Dictionary<ushort, TexturedTileSegment> CreateSpecialSegments(L level, Pen pen)
    {
        return new Dictionary<ushort, TexturedTileSegment>();
    }

    private void ProcessClips(AbstractTexturePacker<E, L> packer, L level, Pen pen, SmoothingMode mode)
    {
        // Some animated textures are shared in segments e.g. 4 32x32 segments within a 64x64 container,
        // so in instances where we only want to wireframe a section of these, we use manual clipping.
        List<TRObjectTexture> textures = GetObjectTextures(level);
        foreach (WireframeClip clip in _data.ManualClips)
        {
            BitmapGraphics frame = CreateFrame(clip.Clip.Width, clip.Clip.Height, pen, mode, true);

            foreach (ushort texture in clip.Textures)
            {
                IndexedTRObjectTexture indexedTexture = new()
                {
                    Index = texture,
                    Texture = textures[texture]
                };
                BitmapGraphics bmp = packer.Tiles[indexedTexture.Atlas].BitmapGraphics;

                List<TexturedTileSegment> segments = packer.Tiles[indexedTexture.Atlas].GetObjectTextureIndexSegments(new int[] { texture });
                foreach (TexturedTileSegment segment in segments)
                {
                    bmp.Import(frame.Bitmap, new Rectangle
                    (
                        segment.Bounds.X + clip.Clip.X, 
                        segment.Bounds.Y + clip.Clip.Y, 
                        clip.Clip.Width, 
                        clip.Clip.Height
                    ));
                }
            }
        }

        // Ensure these clipped textures support transparency
        foreach (TRObjectTexture texture in textures)
        {
            if (texture.BlendingMode == TRBlendingMode.Opaque)
            {
                texture.BlendingMode = TRBlendingMode.AlphaTesting;
            }
        }
    }

    protected BitmapGraphics CreateFrame(int width, int height, Pen pen, SmoothingMode mode, bool addDiagonal)
    {
        BitmapGraphics image = new(new Bitmap(width, height));
        image.Graphics.SmoothingMode = mode;

        image.Graphics.FillRectangle(new SolidBrush(Color.Transparent), new Rectangle(0, 0, width, height));
        image.Graphics.DrawRectangle(pen, 0, 0, width - 1, height - 1);
        if (addDiagonal)
        {
            image.Graphics.DrawLine(pen, 0, 0, width, height);
        }

        return image;
    }

    protected IndexedTRObjectTexture CreateTexture(Rectangle rectangle)
    {
        return new()
        {
            Texture = new(rectangle)
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
            // Exclusion example is Bacon Lara's heavy trigger - we want to retain the Lava here
            if (!IsTextureExcluded(face.Texture))
            {
                face.Texture = triggerIndex;
            }
        }

        foreach (TRFace face in _deathFaces)
        {
            if (!IsTextureExcluded(face.Texture))
            {
                face.Texture = deathIndex;
            }
        }
    }

    private void ResetMeshTextures(Dictionary<TRSize, IndexedTRObjectTexture> sizeRemap, Dictionary<ushort, ushort> specialTextureRemap)
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

    private static TRSize Find(TRSize s, Dictionary<TRSize, IndexedTRObjectTexture> map)
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

        ISet<TRMesh> processedModelMeshes = new HashSet<TRMesh>();
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
        List<TRAnimatedTexture> animatedTextures = GetAnimatedTextures(level);

        for (int i = animatedTextures.Count - 1; i >= 0; i--)
        {
            TRAnimatedTexture animatedTexture = animatedTextures[i];
            for (int j = animatedTexture.Textures.Count - 1; j >= 0; j--)
            {
                if (!IsTextureExcluded(animatedTexture.Textures[j]))
                {
                    animatedTexture.Textures.RemoveAt(j);
                }
            }

            if (animatedTexture.Textures.Count < 2)
            {
                animatedTextures.RemoveAt(i);
            }
        }
    }

    protected abstract Dictionary<TRFace, List<TRVertex>> CollectLadders(L level);
    protected abstract List<TRFace> CollectTriggerFaces(L level, List<FDTrigType> triggerTypes);
    protected abstract List<TRFace> CollectDeathFaces(L level);
    protected abstract AbstractTexturePacker<E, L> CreatePacker(L level);
    protected abstract IEnumerable<IEnumerable<TRFace>> GetRoomFace4s(L level);
    protected abstract IEnumerable<IEnumerable<TRFace>> GetRoomFace3s(L level);
    protected abstract void ResetUnusedTextures(L level);
    protected abstract IEnumerable<int> GetInvalidObjectTextureIndices(L level);
    protected abstract List<TRObjectTexture> GetObjectTextures(L level);
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
    protected abstract List<TRAnimatedTexture> GetAnimatedTextures(L level);

    public virtual bool Is8BitPalette { get; }
    protected virtual void ResetPaletteTracking(L level) { }
}
