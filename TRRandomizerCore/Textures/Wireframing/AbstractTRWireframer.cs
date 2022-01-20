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

namespace TRRandomizerCore.Textures
{
    public abstract class AbstractTRWireframer<E, L>
        where E : Enum
        where L : class
    {
        protected static readonly TRSize _nullSize = new TRSize(0, 0);

        private Dictionary<TRFace3, TRSize> _solidRoomFace3s, _clearRoomFace3s, _solidMeshFace3s, _clearMeshFace3s;
        private Dictionary<TRFace4, TRSize> _solidRoomFace4s, _clearRoomFace4s, _solidMeshFace4s, _clearMeshFace4s;

        private ISet<ushort> _allTextures;
        private WireframeData<E> _data;

        protected virtual bool IsModelTransparent(E modelType)
        {
            return !_data.OpaqueModels.Contains(modelType);
        }

        protected virtual bool IsStaticMeshTransparent(uint meshID)
        {
            return !_data.OpaqueStaticMeshes.Contains(meshID);
        }

        protected virtual bool IsTextureTransparent(ushort texture)
        {
            return _data.TransparentTextures.Contains(texture);
        }

        protected virtual bool IsTextureExcluded(ushort texture)
        {
            return _data.ExcludedTextures.Contains(texture);
        }

        public void Apply(L level, WireframeData<E> data)
        {
            _solidRoomFace3s = new Dictionary<TRFace3, TRSize>();
            _clearRoomFace3s = new Dictionary<TRFace3, TRSize>();
            _solidRoomFace4s = new Dictionary<TRFace4, TRSize>();
            _clearRoomFace4s = new Dictionary<TRFace4, TRSize>();
            _solidMeshFace3s = new Dictionary<TRFace3, TRSize>();
            _clearMeshFace3s = new Dictionary<TRFace3, TRSize>();
            _solidMeshFace4s = new Dictionary<TRFace4, TRSize>();
            _clearMeshFace4s = new Dictionary<TRFace4, TRSize>();
            _allTextures = new SortedSet<ushort>();
            _data = data;

            ScanTransparentTextures(level);
            ScanRooms(level);
            ScanMeshes(level);

            ISet<TRSize> clearRoomSizes = new SortedSet<TRSize>(_clearRoomFace3s.Values.Concat(_clearRoomFace4s.Values));
            ISet<TRSize> solidRoomSizes = new SortedSet<TRSize>(_solidRoomFace3s.Values.Concat(_solidRoomFace4s.Values));
            ISet<TRSize> clearMeshSizes = new SortedSet<TRSize>(_clearMeshFace3s.Values.Concat(_clearMeshFace4s.Values));
            ISet<TRSize> solidMeshSizes = new SortedSet<TRSize>(_solidMeshFace3s.Values.Concat(_solidMeshFace4s.Values));

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

                TRSize clearRoomSize = GetLargestSize(clearRoomSizes);
                TRSize solidRoomSize = GetLargestSize(solidRoomSizes);

                IndexedTRObjectTexture transpRoomTexture = CreateWireframe(packer, clearRoomSize, Color.Transparent, roomPen, SmoothingMode.AntiAlias);
                IndexedTRObjectTexture opaqueRoomTexture = CreateWireframe(packer, solidRoomSize, _data.BackgroundColour, roomPen, SmoothingMode.AntiAlias);

                Dictionary<TRSize, IndexedTRObjectTexture> clearModelRemap = new Dictionary<TRSize, IndexedTRObjectTexture>();
                Dictionary<TRSize, IndexedTRObjectTexture> solidModelRemap = new Dictionary<TRSize, IndexedTRObjectTexture>();

                foreach (TRSize size in clearMeshSizes)
                {
                    clearModelRemap[size] = CreateWireframe(packer, size, Color.Transparent, modelPen, SmoothingMode.None);
                }
                foreach (TRSize size in solidMeshSizes)
                {
                    solidModelRemap[size] = CreateWireframe(packer, size, _data.BackgroundColour, modelPen, SmoothingMode.None);
                }

                packer.Options.StartMethod = PackingStartMethod.FirstTile;
                packer.Pack(true);

                Queue<int> reusableTextures = new Queue<int>(GetInvalidObjectTextureIndices(level));
                List<TRObjectTexture> levelObjectTextures = GetObjectTextures(level).ToList();

                ushort clearTextureIndex = (ushort)reusableTextures.Dequeue();
                ushort solidTextureIndex = (ushort)reusableTextures.Dequeue();
                if (transpRoomTexture != null)
                {
                    levelObjectTextures[clearTextureIndex] = transpRoomTexture.Texture;
                }
                levelObjectTextures[solidTextureIndex] = opaqueRoomTexture.Texture;

                foreach (TRSize size in clearModelRemap.Keys)
                {
                    if (!size.Equals(_nullSize))
                    {
                        ushort texture = (ushort)reusableTextures.Dequeue();
                        levelObjectTextures[texture] = clearModelRemap[size].Texture;
                        clearModelRemap[size].Index = texture;
                    }
                }
                foreach (TRSize size in solidModelRemap.Keys)
                {
                    if (!size.Equals(_nullSize))
                    {
                        ushort texture = (ushort)reusableTextures.Dequeue();
                        levelObjectTextures[texture] = solidModelRemap[size].Texture;
                        solidModelRemap[size].Index = texture;
                    }
                }

                SetObjectTextures(level, levelObjectTextures);

                ResetRoomTextures(clearTextureIndex, solidTextureIndex);
                ResetMeshTextures(clearModelRemap, solidModelRemap);
                TidyModels(level);
            }
        }

        private void ScanTransparentTextures(L level)
        {
            // Ensure if any excluded textures are transparent, that
            // they're added to the data list to avoid replacement.
            TRObjectTexture[] texts = GetObjectTextures(level);
            foreach (ushort excTexture in _data.ExcludedTextures)
            {
                if (texts[excTexture].Attribute > 0)
                {
                    _data.TransparentTextures.Add(excTexture);
                }
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
                ushort texture = (ushort)(face.Texture & 0x0fff);
                TRSize size = GetTextureSize(level, texture);
                if (IsTextureTransparent(texture))
                {
                    if (!IsTextureExcluded(texture))
                    {
                        _clearRoomFace4s[face] = size;
                    }
                }
                else
                {
                    _solidRoomFace4s[face] = size;
                }
            }
        }

        private void ScanRoomFace3s(L level, IEnumerable<TRFace3> faces)
        {
            foreach (TRFace3 face in faces)
            {
                ushort texture = (ushort)(face.Texture & 0x0fff);
                TRSize size = GetTextureSize(level, texture);
                if (IsTextureTransparent(texture))
                {
                    if (!IsTextureExcluded(texture))
                    {
                        _clearRoomFace3s[face] = size;
                    }
                }
                else
                {
                    _solidRoomFace3s[face] = size;
                }
            }
        }

        private void ScanMeshes(L level)
        {
            Dictionary<E, TRMesh[]> modelMeshes = GetModelMeshes(level);
            ISet<TRMesh> processedMeshes = new HashSet<TRMesh>();
            foreach (E modelID in modelMeshes.Keys)
            {
                bool isTransparent = IsModelTransparent(modelID);
                foreach (TRMesh mesh in modelMeshes[modelID])
                {
                    if (processedMeshes.Add(mesh))
                    {
                        ScanMesh(level, mesh, isTransparent);
                    }
                }
            }

            foreach (TRStaticMesh staticMesh in GetStaticMeshes(level))
            {
                TRMesh mesh = GetStaticMesh(level, staticMesh);
                if (processedMeshes.Add(mesh))
                {
                    ScanMesh(level, mesh, IsStaticMeshTransparent(staticMesh.ID));
                }
            }
        }

        private void ScanMesh(L level, TRMesh mesh, bool isClear)
        {
            foreach (TRFace4 face in mesh.TexturedRectangles)
            {
                TRSize size = GetTextureSize(level, (ushort)(face.Texture & 0x0fff));
                if (isClear)
                {
                    _clearMeshFace4s[face] = size;
                }
                else
                {
                    _solidMeshFace4s[face] = size;
                }
            }

            foreach (TRFace3 face in mesh.TexturedTriangles)
            {
                TRSize size = GetTextureSize(level, (ushort)(face.Texture & 0x0fff));
                if (isClear)
                {
                    _clearMeshFace3s[face] = size;
                }
                else
                {
                    _solidMeshFace3s[face] = size;
                }
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
                if (!IsTextureExcluded(t))
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

        private IndexedTRObjectTexture CreateWireframe(AbstractTexturePacker<E, L> packer, TRSize size, Color background, Pen pen, SmoothingMode mode)
        {
            if (size.Equals(_nullSize))
            {
                return null;
            }

            Rectangle r = new Rectangle(0, 0, size.W, size.H);
            IndexedTRObjectTexture texture = CreateTexture(r);
            if (background == Color.Transparent)
            {
                texture.Texture.Attribute = 1;
            }

            Bitmap image = new Bitmap(size.W, size.H);
            Graphics g = Graphics.FromImage(image);
            g.SmoothingMode = mode;

            g.FillRectangle(new SolidBrush(background), r);
            g.DrawRectangle(pen, 0, 0, r.Width - 1, r.Height - 1);
            g.DrawLine(pen, 0, 0, size.W, size.H);

            packer.AddRectangle(new TexturedTileSegment(texture, image));

            return texture;
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
                Attribute = 0,
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

        private void ResetRoomTextures(ushort clearIndex, ushort solidIndex)
        {
            _clearRoomFace3s.Keys.ToList().ForEach(f => f.Texture = RemapTexture(f.Texture, clearIndex));
            _clearRoomFace4s.Keys.ToList().ForEach(f => f.Texture = RemapTexture(f.Texture, clearIndex));
            _solidRoomFace3s.Keys.ToList().ForEach(f => f.Texture = RemapTexture(f.Texture, solidIndex));
            _solidRoomFace4s.Keys.ToList().ForEach(f => f.Texture = RemapTexture(f.Texture, solidIndex));
        }

        private void ResetMeshTextures(Dictionary<TRSize, IndexedTRObjectTexture> clearRemap, Dictionary<TRSize, IndexedTRObjectTexture> solidRemap)
        {
            List<TRSize> clearSizes = clearRemap.Keys.ToList();
            List<TRSize> solidSizes = solidRemap.Keys.ToList();
            clearSizes.Sort(); clearSizes.Reverse();
            solidSizes.Sort(); solidSizes.Reverse();

            foreach (TRFace3 face in _clearMeshFace3s.Keys)
            {
                TRSize size = _clearMeshFace3s[face];
                if (!size.Equals(_nullSize))
                {
                    if (!clearRemap.ContainsKey(size))
                    {
                        size = Find(size, clearRemap);
                    }
                    face.Texture = RemapTexture(face.Texture, (ushort)clearRemap[size].Index);
                }
            }
            foreach (TRFace4 face in _clearMeshFace4s.Keys)
            {
                TRSize size = _clearMeshFace4s[face];
                if (!size.Equals(_nullSize))
                {
                    if (!clearRemap.ContainsKey(size))
                    {
                        size = Find(size, clearRemap);
                    }
                    face.Texture = RemapTexture(face.Texture, (ushort)clearRemap[size].Index);
                }
            }

            foreach (TRFace3 face in _solidMeshFace3s.Keys)
            {
                TRSize size = _solidMeshFace3s[face];
                if (!size.Equals(_nullSize))
                {
                    if (!solidRemap.ContainsKey(size))
                    {
                        size = Find(size, clearRemap);
                    }
                    face.Texture = RemapTexture(face.Texture, (ushort)solidRemap[size].Index);
                }
            }
            foreach (TRFace4 face in _solidMeshFace4s.Keys)
            {
                TRSize size = _solidMeshFace4s[face];
                if (!size.Equals(_nullSize))
                {
                    if (!solidRemap.ContainsKey(size))
                    {
                        size = Find(size, clearRemap);
                    }
                    face.Texture = RemapTexture(face.Texture, (ushort)solidRemap[size].Index);
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
            int solidLaraIndex = -1;
            if (_data.SolidLara)
            {
                solidLaraIndex = ImportColour(level, _data.HighlightColour);
            }

            foreach (TRStaticMesh smesh in GetStaticMeshes(level))
            {
                TRMesh mesh = GetStaticMesh(level, smesh);
                SetFace4Colours(mesh.ColouredRectangles, blackIndex);
                SetFace3Colours(mesh.ColouredTriangles, blackIndex);
            }

            ISet<TRMesh> processedMeshes = new HashSet<TRMesh>();
            foreach (TRModel model in GetModels(level))
            {
                bool solidLara = _data.SolidLara && IsLaraModel(model);
                TRMesh[] meshes = GetModelMeshes(level, model);
                if (meshes != null)
                {
                    foreach (TRMesh mesh in meshes)
                    {
                        if (processedMeshes.Add(mesh))
                        {
                            SetFace4Colours(mesh.ColouredRectangles, solidLara ? solidLaraIndex : blackIndex);
                            SetFace3Colours(mesh.ColouredTriangles, solidLara ? solidLaraIndex : blackIndex);
                        }
                    }

                    if (IsSkybox(model))
                    {
                        // Completely clear the skybox otherwise the frame bleeds through
                        foreach (TRMesh mesh in meshes)
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
                    else if (solidLara || ShouldSolidifyModel(model))
                    {
                        if (solidLaraIndex == -1)
                        {
                            solidLaraIndex = ImportColour(level, _data.HighlightColour);
                        }

                        foreach (TRMesh mesh in meshes)
                        {
                            List<TRFace4> rects = mesh.ColouredRectangles.ToList();
                            foreach (TRFace4 rect in mesh.TexturedRectangles)
                            {
                                rect.Texture = (ushort)(solidLaraIndex << 8);
                                rects.Add(rect);
                            }
                            mesh.TexturedRectangles = new TRFace4[] { };
                            mesh.NumTexturedRectangles = 0;
                            mesh.ColouredRectangles = rects.ToArray();
                            mesh.NumColouredRectangles = (short)rects.Count;

                            List<TRFace3> tris = mesh.ColouredTriangles.ToList();
                            foreach (TRFace3 tri in mesh.TexturedTriangles)
                            {
                                tri.Texture = (ushort)(solidLaraIndex << 8);
                                tris.Add(tri);
                            }
                            mesh.TexturedTriangles = new TRFace3[] { };
                            mesh.NumTexturedTriangles = 0;
                            mesh.ColouredTriangles = tris.ToArray();
                            mesh.NumColouredTriangles = (short)tris.Count;
                        }
                    }
                }
            }

            if (solidLaraIndex != -1)
            {
                ResetPaletteTracking(level);
            }
        }

        private void SetFace4Colours(IEnumerable<TRFace4> faces, int colourIndex)
        {
            foreach (TRFace4 face in faces)
            {
                face.Texture = (ushort)((colourIndex << 8) | (face.Texture & 0xFF));
            }
        }

        private void SetFace3Colours(IEnumerable<TRFace3> faces, int colourIndex)
        {
            foreach (TRFace3 face in faces)
            {
                face.Texture = (ushort)((colourIndex << 8) | (face.Texture & 0xFF));
            }
        }

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
        protected abstract TRStaticMesh[] GetStaticMeshes(L level);
        protected abstract TRMesh GetStaticMesh(L level, TRStaticMesh staticMesh);
        protected abstract bool IsSkybox(TRModel model);
        protected abstract bool IsLaraModel(TRModel model);
        protected virtual bool ShouldSolidifyModel(TRModel model) => false;
    }
}