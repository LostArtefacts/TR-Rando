using RectanglePacker.Organisation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using TRLevelReader.Model;
using TRModelTransporter.Helpers;
using TRModelTransporter.Model.Textures;
using TRModelTransporter.Packing;
using TRTexture16Importer.Helpers;

namespace TRRandomizerCore.Textures
{
    public abstract class AbstractTRWireframer<E, L>
        where E : Enum
        where L : class
    {
        protected static readonly TRSize _nullSize = new TRSize(0, 0);
        protected static readonly int _ladderRungs = 4;

        private Dictionary<TRFace3, TRSize> _roomFace3s, _meshFace3s;
        private Dictionary<TRFace4, TRSize> _roomFace4s, _meshFace4s;
        private Dictionary<TRFace4, List<TRVertex>> _ladderFace4s;

        private ISet<ushort> _allTextures;
        protected WireframeData _data;

        protected virtual bool IsTextureExcluded(ushort texture)
        {
            return _data.ExcludedTextures.Contains(texture);
        }

        protected virtual bool IsTextureOverriden(ushort texture)
        {
            return _data.ForcedOverrides.Contains(texture);
        }

        public void Apply(L level, WireframeData data)
        {
            _roomFace3s = new Dictionary<TRFace3, TRSize>();
            _roomFace4s = new Dictionary<TRFace4, TRSize>();
            _meshFace3s = new Dictionary<TRFace3, TRSize>();
            _meshFace4s = new Dictionary<TRFace4, TRSize>();
            _ladderFace4s = data.HighlightLadders ? CollectLadders(level) : new Dictionary<TRFace4, List<TRVertex>>();
            _allTextures = new SortedSet<ushort>();
            _data = data;

            ScanRooms(level);
            ScanMeshes(level);

            ISet<TRSize> roomSizes = new SortedSet<TRSize>(_roomFace3s.Values.Concat(_roomFace4s.Values));
            ISet<TRSize> meshSizes = new SortedSet<TRSize>(_meshFace3s.Values.Concat(_meshFace4s.Values));

            Pen roomPen = new Pen(_data.HighlightColour, 1)
            {
                Alignment = PenAlignment.Inset,
                DashCap = DashCap.Round
            };            
            Pen modelPen = new Pen(_data.HighlightColour, 1)
            {
                Alignment = PenAlignment.Inset
            };

            using (AbstractTexturePacker<E, L> packer = CreatePacker(level))
            {
                DeleteTextures(packer);
                ResetUnusedTextures(level);

                TRSize roomSize = GetLargestSize(roomSizes);

                IndexedTRObjectTexture roomTexture = CreateWireframe(packer, roomSize, roomPen, SmoothingMode.AntiAlias);
                IndexedTRObjectTexture ladderTexture = CreateLadderWireframe(packer, roomSize, roomPen, SmoothingMode.AntiAlias);
                ProcessClips(packer, level, roomPen, SmoothingMode.AntiAlias);

                Dictionary<TRSize, IndexedTRObjectTexture> modelRemap = new Dictionary<TRSize, IndexedTRObjectTexture>();
                foreach (TRSize size in meshSizes)
                {
                    modelRemap[size] = CreateWireframe(packer, size, modelPen, SmoothingMode.None);
                }

                packer.Options.StartMethod = PackingStartMethod.FirstTile;
                packer.Pack(true);

                Queue<int> reusableTextures = new Queue<int>(GetInvalidObjectTextureIndices(level));
                List<TRObjectTexture> levelObjectTextures = GetObjectTextures(level).ToList();

                ushort roomTextureIndex = (ushort)reusableTextures.Dequeue();
                levelObjectTextures[roomTextureIndex] = roomTexture.Texture;

                ushort ladderTextureIndex = (ushort)reusableTextures.Dequeue();
                levelObjectTextures[ladderTextureIndex] = ladderTexture.Texture;

                foreach (TRSize size in modelRemap.Keys)
                {
                    if (!size.Equals(_nullSize))
                    {
                        ushort texture = (ushort)reusableTextures.Dequeue();
                        levelObjectTextures[texture] = modelRemap[size].Texture;
                        modelRemap[size].Index = texture;
                    }
                }

                SetObjectTextures(level, levelObjectTextures);

                ResetRoomTextures(roomTextureIndex, ladderTextureIndex);
                ResetMeshTextures(modelRemap);
                TidyModels(level);
                SetSkyboxVisible(level);
                DeleteAnimatedTextures(level);
            }
        }

        private void ScanRooms(L level)
        {
            foreach (IEnumerable<TRFace4> roomRects in GetRoomFace4s(level))
            {
                ScanRoomFace4s(level, roomRects);
            }
            foreach (IEnumerable<TRFace3> roomTris in GetRoomFace3s(level))
            {
                ScanRoomFace3s(level, roomTris);
            }
        }

        private void ScanRoomFace4s(L level, IEnumerable<TRFace4> faces)
        {
            foreach (TRFace4 face in faces)
            {
                if (_ladderFace4s.ContainsKey(face))
                    continue;

                ushort texture = (ushort)(face.Texture & 0x0fff);
                if (!IsTextureExcluded(texture))
                {
                    _roomFace4s[face] = GetTextureSize(level, texture);
                }
            }
        }

        private void ScanRoomFace3s(L level, IEnumerable<TRFace3> faces)
        {
            foreach (TRFace3 face in faces)
            {
                ushort texture = (ushort)(face.Texture & 0x0fff);
                if (!IsTextureExcluded(texture))
                {
                    _roomFace3s[face] = GetTextureSize(level, texture);
                }
            }
        }

        private void ScanMeshes(L level)
        {
            foreach (TRMesh mesh in GetLevelMeshes(level))
            {
                ScanMesh(level, mesh);
            }
        }

        private void ScanMesh(L level, TRMesh mesh)
        {
            foreach (TRFace4 face in mesh.TexturedRectangles)
            {
                _meshFace4s[face] = GetTextureSize(level, (ushort)(face.Texture & 0x0fff));
            }

            foreach (TRFace3 face in mesh.TexturedTriangles)
            {
                _meshFace3s[face] = GetTextureSize(level, (ushort)(face.Texture & 0x0fff));
            }
        }

        private TRSize GetTextureSize(L level, ushort textureIndex)
        {
            TRObjectTexture texture = GetObjectTextures(level)[textureIndex];
            if (texture.IsValid())
            {
                _allTextures.Add(textureIndex);
                IndexedTRObjectTexture itext = new IndexedTRObjectTexture
                {
                    Texture = texture
                };
                return new TRSize(itext.Bounds.Width, itext.Bounds.Height);
            }

            return _nullSize;
        }

        private void DeleteTextures(AbstractTexturePacker<E, L> packer)
        {
            List<int> textures = new List<int>();
            foreach (ushort t in _allTextures)
            {
                if (!IsTextureExcluded(t) && !IsTextureOverriden(t))
                {
                    textures.Add(t);
                }
            }

            packer.RemoveObjectTextureSegments(textures);
        }

        private TRSize GetLargestSize(IEnumerable<TRSize> sizes)
        {
            List<TRSize> compSizes = new List<TRSize>(sizes);
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

        private void ProcessClips(AbstractTexturePacker<E, L> packer, L level, Pen pen, SmoothingMode mode)
        {
            // Some animated textures are shared in segments e.g. 4 32x32 segments within a 64x64 container,
            // so in instances where we only want to wireframe a section of these, we use manual clipping.
            TRObjectTexture[] textures = GetObjectTextures(level);
            foreach (WireframeClip clip in _data.ManualClips)
            {
                BitmapGraphics frame = CreateFrame(clip.Clip.Width, clip.Clip.Height, pen, mode, true);

                foreach (ushort texture in clip.Textures)
                {
                    IndexedTRObjectTexture indexedTexture = new IndexedTRObjectTexture
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
                if (texture.Attribute == 0)
                {
                    texture.Attribute = 1;
                }
            }
        }

        private BitmapGraphics CreateFrame(int width, int height, Pen pen, SmoothingMode mode, bool addDiagonal)
        {
            BitmapGraphics image = new BitmapGraphics(new Bitmap(width, height));
            image.Graphics.SmoothingMode = mode;

            image.Graphics.FillRectangle(new SolidBrush(Color.Transparent), new Rectangle(0, 0, width, height));
            image.Graphics.DrawRectangle(pen, 0, 0, width - 1, height - 1);
            if (addDiagonal)
            {
                image.Graphics.DrawLine(pen, 0, 0, width, height);
            }

            return image;
        }

        private IndexedTRObjectTexture CreateTexture(Rectangle rectangle)
        {
            // Configure the points
            List<TRObjectTextureVert> vertices = new List<TRObjectTextureVert>
            {
                CreatePoint(0, 0),
                CreatePoint(rectangle.Width, 0),
                CreatePoint(rectangle.Width, rectangle.Height),
                CreatePoint(0, rectangle.Height)
            };

            // Make a dummy texture object with the given bounds
            TRObjectTexture texture = new TRObjectTexture
            {
                AtlasAndFlag = 0,
                Attribute = 1,
                Vertices = vertices.ToArray()
            };

            return new IndexedTRObjectTexture
            {
                Index = 0,
                Texture = texture
            };
        }

        private TRObjectTextureVert CreatePoint(int x, int y)
        {
            return new TRObjectTextureVert
            {
                XCoordinate = new FixedFloat16
                {
                    Whole = (byte)(x == 0 ? 1 : 255),
                    Fraction = (byte)(x == 0 ? 0 : x - 1)
                },
                YCoordinate = new FixedFloat16
                {
                    Whole = (byte)(y == 0 ? 1 : 255),
                    Fraction = (byte)(y == 0 ? 0 : y - 1)
                }
            };
        }

        private void ResetRoomTextures(ushort wireframeIndex, ushort ladderIndex)
        {
            foreach (TRFace3 face in _roomFace3s.Keys)
            {
                face.Texture = RemapTexture(face.Texture, wireframeIndex);
            }

            foreach (TRFace4 face in _roomFace4s.Keys)
            {
                if (!_ladderFace4s.ContainsKey(face))
                {
                    face.Texture = RemapTexture(face.Texture, wireframeIndex);
                }
            }

            foreach (TRFace4 face in _ladderFace4s.Keys)
            {
                face.Texture = RemapTexture(face.Texture, ladderIndex);

                // Ensure the ladder isn't sideways - if the first two vertices don't have
                // the same Y val and it's a wall, rotate the face once.
                List<TRVertex> vertices = _ladderFace4s[face];
                if (vertices.Count > 1 &&
                    vertices[0].Y != vertices[1].Y &&
                    (vertices.All(v => v.X == vertices[0].X) || vertices.All(v => v.Z == vertices[0].Z)))
                {
                    Queue<ushort> vertIndices = new Queue<ushort>(face.Vertices);
                    vertIndices.Enqueue(vertIndices.Dequeue());
                    face.Vertices = vertIndices.ToArray();
                }
            }
        }

        private void ResetMeshTextures(Dictionary<TRSize, IndexedTRObjectTexture> sizeRemap)
        {
            foreach (TRFace3 face in _meshFace3s.Keys)
            {
                TRSize size = _meshFace3s[face];
                if (!size.Equals(_nullSize))
                {
                    if (!sizeRemap.ContainsKey(size))
                    {
                        size = Find(size, sizeRemap);
                    }
                    face.Texture = RemapTexture(face.Texture, (ushort)sizeRemap[size].Index);
                }
            }

            foreach (TRFace4 face in _meshFace4s.Keys)
            {
                TRSize size = _meshFace4s[face];
                if (!size.Equals(_nullSize))
                {
                    if (!sizeRemap.ContainsKey(size))
                    {
                        size = Find(size, sizeRemap);
                    }
                    face.Texture = RemapTexture(face.Texture, (ushort)sizeRemap[size].Index);
                }
            }
        }

        private TRSize Find(TRSize s, Dictionary<TRSize, IndexedTRObjectTexture> map)
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

        private ushort RemapTexture(ushort currentTexture, ushort newTexture)
        {
            // Make sure double-sided textures are retained
            if ((currentTexture & 0x8000) > 0)
            {
                newTexture |= 0x8000;
            }
            return newTexture;
        }

        private void TidyModels(L level)
        {
            int blackIndex = GetBlackPaletteIndex(level);

            // For most meshes, replace any colours with the default background
            foreach (TRMesh mesh in GetLevelMeshes(level))
            {
                SetFace4Colours(mesh.ColouredRectangles, blackIndex);
                SetFace3Colours(mesh.ColouredTriangles, blackIndex);
            }

            ISet<TRMesh> processedModelMeshes = new HashSet<TRMesh>();
            foreach (TRModel model in GetModels(level))
            {
                if (IsSkybox(model))
                {
                    // Solidify the skybox as it will become the backdrop for every room
                    foreach (TRMesh mesh in GetModelMeshes(level, model))
                    {
                        List<TRFace4> rects = mesh.ColouredRectangles.ToList();
                        foreach (TRFace4 rect in mesh.TexturedRectangles)
                        {
                            rect.Texture = mesh.ColouredTriangles[0].Texture;
                            rects.Add(rect);
                        }
                        mesh.TexturedRectangles = new TRFace4[] { };
                        mesh.NumTexturedRectangles = 0;
                        mesh.ColouredRectangles = rects.ToArray();
                        mesh.NumColouredRectangles = (short)rects.Count;
                    }
                }
                else if
                (
                    (_data.SolidLara && IsLaraModel(model)) ||
                    (_data.SolidEnemies && (IsEnemyModel(model) || _data.SolidModels.Contains(model.ID)) && !IsEnemyPlaceholderModel(model)) ||
                    ShouldSolidifyModel(model)
                )
                {
                    int paletteIndex = ImportColour(level, !IsLaraModel(model) && _data.ModelColours.ContainsKey(model.ID) ?
                        _data.ModelColours[model.ID] :
                        _data.HighlightColour);

                    if (paletteIndex == -1)
                    {
                        paletteIndex = blackIndex;
                    }

                    foreach (TRMesh mesh in GetModelMeshes(level, model))
                    {
                        if (processedModelMeshes.Add(mesh))
                        {
                            // Convert all textured polygons to coloured ones, and reset the
                            // palette index they point to.
                            List<TRFace4> rects = mesh.ColouredRectangles.ToList();
                            rects.AddRange(mesh.TexturedRectangles);

                            List<TRFace3> tris = mesh.ColouredTriangles.ToList();
                            tris.AddRange(mesh.TexturedTriangles);

                            SetFace4Colours(rects, paletteIndex);
                            SetFace3Colours(tris, paletteIndex);

                            mesh.TexturedRectangles = new TRFace4[] { };
                            mesh.NumTexturedRectangles = 0;
                            mesh.ColouredRectangles = rects.ToArray();
                            mesh.NumColouredRectangles = (short)rects.Count;

                            mesh.TexturedTriangles = new TRFace3[] { };
                            mesh.NumTexturedTriangles = 0;
                            mesh.ColouredTriangles = tris.ToArray();
                            mesh.NumColouredTriangles = (short)tris.Count;
                        }
                    }
                }
            }

            // In case we have imported any colours
            ResetPaletteTracking(level);
        }

        private void SetFace4Colours(IEnumerable<TRFace4> faces, int colourIndex)
        {
            foreach (TRFace4 face in faces)
            {
                face.Texture = (ushort)(Is8BitPalette ? colourIndex : (colourIndex << 8 | (face.Texture & 0xFF)));
            }
        }

        private void SetFace3Colours(IEnumerable<TRFace3> faces, int colourIndex)
        {
            foreach (TRFace3 face in faces)
            {
                face.Texture = (ushort)(Is8BitPalette ? colourIndex : (colourIndex << 8 | (face.Texture & 0xFF)));
            }
        }

        private void DeleteAnimatedTextures(L level)
        {
            List<TRAnimatedTexture> animatedTextures = GetAnimatedTextures(level).ToList();

            for (int i = animatedTextures.Count - 1; i >= 0; i--)
            {
                TRAnimatedTexture animatedTexture = animatedTextures[i];
                List<ushort> textures = animatedTexture.Textures.ToList();
                for (int j = textures.Count - 1; j >= 0; j--)
                {
                    if (!IsTextureExcluded(textures[j]))
                    {
                        textures.RemoveAt(j);
                    }
                }

                if (textures.Count < 2)
                {
                    animatedTextures.RemoveAt(i);
                }
                else
                {
                    animatedTexture.Textures = textures.ToArray();
                }
            }

            int length = 1;
            foreach (TRAnimatedTexture animatedTexture in animatedTextures)
            {
                length += animatedTexture.NumTextures + 2;
            }

            SetAnimatedTextures(level, animatedTextures.ToArray(), (ushort)length);
        }

        protected abstract Dictionary<TRFace4, List<TRVertex>> CollectLadders(L level);
        protected abstract AbstractTexturePacker<E, L> CreatePacker(L level);
        protected abstract IEnumerable<IEnumerable<TRFace4>> GetRoomFace4s(L level);
        protected abstract IEnumerable<IEnumerable<TRFace3>> GetRoomFace3s(L level);
        protected abstract void ResetUnusedTextures(L level);
        protected abstract IEnumerable<int> GetInvalidObjectTextureIndices(L level);
        protected abstract TRObjectTexture[] GetObjectTextures(L level);
        protected abstract void SetObjectTextures(L level, IEnumerable<TRObjectTexture> textures);
        protected abstract Dictionary<E, TRMesh[]> GetModelMeshes(L level);
        protected abstract int GetBlackPaletteIndex(L level);
        protected abstract int ImportColour(L level, Color c);
        protected abstract void ResetPaletteTracking(L level);
        protected abstract TRModel[] GetModels(L level);
        protected abstract TRMesh[] GetModelMeshes(L level, TRModel model);
        protected abstract TRMesh[] GetLevelMeshes(L level);
        protected abstract TRStaticMesh[] GetStaticMeshes(L level);
        protected abstract TRMesh GetStaticMesh(L level, TRStaticMesh staticMesh);
        protected abstract bool IsSkybox(TRModel model);
        protected abstract bool IsLaraModel(TRModel model);
        protected abstract bool IsEnemyModel(TRModel model);
        protected virtual bool IsEnemyPlaceholderModel(TRModel model) => false;
        protected virtual bool ShouldSolidifyModel(TRModel model) => false;
        protected abstract void SetSkyboxVisible(L level);
        protected abstract TRAnimatedTexture[] GetAnimatedTextures(L level);
        protected abstract void SetAnimatedTextures(L level, TRAnimatedTexture[] animatedTextures, ushort length);

        public virtual bool Is8BitPalette { get; }
    }
}