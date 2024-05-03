using RectanglePacker.Events;
using System.Drawing;
using TRLevelControl;
using TRLevelControl.Model;
using TRModelTransporter.Model.Textures;
using TRModelTransporter.Packing;
using TRTexture16Importer.Helpers;
using TRTexture16Importer.Textures;

namespace TRRandomizerCore.Textures;

public abstract class AbstractLandmarkImporter<E, L>
    where E : Enum
    where L : class
{
    public bool IsCommunityPatch { get; set; }

    protected abstract int MaxTextures { get; }

    protected abstract AbstractTexturePacker<E, L> CreatePacker(L level);
    protected abstract List<TRObjectTexture> GetObjectTextures(L level);
    protected abstract void SetRoomTexture(L level, int roomIndex, int rectangleIndex, ushort textureIndex);
    protected abstract short? GetRoomFromPortal(L level, PortalSector portalSector, bool isLevelMirrored);

    public bool Import(L level, AbstractTextureMapping<E, L> mapping, bool isLevelMirrored)
    {
        // Ensure any changes already made are committed to the level
        mapping.CommitGraphics();

        // If we are already at the maximum number of textures, bail out.
        List<TRObjectTexture> textures = GetObjectTextures(level);
        if (textures.Count == MaxTextures)
        {
            return false;
        }

        using AbstractTexturePacker<E, L> packer = CreatePacker(level);
        Dictionary<LandmarkTextureTarget, TexturedTileSegment> targetSegmentMap = new();

        foreach (StaticTextureSource<E> source in mapping.LandmarkMapping.Keys)
        {
            if (textures.Count == MaxTextures)
            {
                break;
            }

            if (!source.HasVariants)
            {
                continue;
            }

            List<Rectangle> segments = source.VariantMap[source.Variants[0]];
            foreach (int segmentIndex in mapping.LandmarkMapping[source].Keys)
            {
                Dictionary<int, LandmarkTextureTarget> backgroundCache = new();

                foreach (LandmarkTextureTarget target in mapping.LandmarkMapping[source][segmentIndex])
                {
                    if (target.PortalSector != null)
                    {
                        // This target is meant for a room that has been created by environment mods.
                        // Test the portal in the given location to get the room number, traversing
                        // until we reach the target room. If it doesn't exist, the landmark will not
                        // be imported.
                        PortalSector sector = target.PortalSector;
                        bool traverse = true;
                        short? room;
                        do
                        {
                            room = GetRoomFromPortal(level, sector, isLevelMirrored);
                            if (traverse = room.HasValue && sector.NextPortal != null)
                            {
                                sector.NextPortal.Room = room.Value;
                                sector = sector.NextPortal;
                            }
                        }
                        while (traverse);

                        if (room.HasValue)
                        {
                            target.RoomNumber = room.Value;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    if (target.BackgroundIndex != -1 && backgroundCache.ContainsKey(target.BackgroundIndex))
                    {
                        // The same graphic has already been added, so just copy the mapping.
                        // This is most likely for flipped rooms.
                        target.MappedTextureIndex = backgroundCache[target.BackgroundIndex].MappedTextureIndex;
                        targetSegmentMap[target] = targetSegmentMap[backgroundCache[target.BackgroundIndex]];
                        continue;
                    }

                    IndexedTRObjectTexture texture = CreateTexture(segments[segmentIndex], isLevelMirrored);
                    target.MappedTextureIndex = textures.Count;
                    textures.Add(texture.Texture);

                    Bitmap image;
                    if (target.BackgroundIndex != -1)
                    {
                        IndexedTRObjectTexture indexedTexture = new()
                        {
                            Index = target.BackgroundIndex,
                            Texture = textures[target.BackgroundIndex]
                        };
                        BitmapGraphics tile = packer.Tiles[indexedTexture.Atlas].BitmapGraphics;
                        BitmapGraphics clip = new(tile.Extract(indexedTexture.Bounds));
                        clip.Overlay(source.Bitmap);
                        image = clip.Bitmap;

                        backgroundCache[target.BackgroundIndex] = target;
                    }
                    else
                    {
                        image = source.ClonedBitmap;
                    }

                    TexturedTileSegment segment = new(texture, image);
                    packer.AddRectangle(segment);
                    targetSegmentMap[target] = segment;
                }
            }
        }

        if (packer.TotalRectangles == 0)
        {
            return false;
        }

        try
        {
            PackingResult<TexturedTile, TexturedTileSegment> result = packer.Pack(true);

            // Perform the room data remapping
            foreach (StaticTextureSource<E> source in mapping.LandmarkMapping.Keys)
            {
                if (!source.HasVariants)
                {
                    continue;
                }

                foreach (int segmentIndex in mapping.LandmarkMapping[source].Keys)
                {
                    foreach (LandmarkTextureTarget target in mapping.LandmarkMapping[source][segmentIndex])
                    {
                        if (target.MappedTextureIndex == -1 || result.Packer.OrphanedRectangles.Contains(targetSegmentMap[target]))
                        {
                            // There wasn't enough space for this
                            continue;
                        }

                        foreach (int rectIndex in target.RectangleIndices)
                        {
                            SetRoomTexture(level, target.RoomNumber, rectIndex, (ushort)target.MappedTextureIndex);
                        }
                    }
                }
            }

            return true;
        }
        catch (PackingException)
        {
            return false;
        }
    }

    private static IndexedTRObjectTexture CreateTexture(Rectangle rectangle, bool mirrored)
    {
        return new()
        {
            Texture = new(rectangle)
            {
                UVMode = mirrored ? TRUVMode.NE_AntiClockwise : TRUVMode.NW_Clockwise
            }
        };
    }

    protected short? GetSectorPortalRoom(TRRoomSector sector, FDControl floorData, PortalDirection direction)
    {
        if (direction == PortalDirection.Up && sector.RoomAbove != TRConsts.NoRoom)
        {
            return sector.RoomAbove;
        }
        else if (direction == PortalDirection.Down && sector.RoomBelow != TRConsts.NoRoom)
        {
            return sector.RoomBelow;
        }
        else if (sector.FDIndex != 0
            && floorData[sector.FDIndex].Find(e => e is FDPortalEntry) is FDPortalEntry portal)
        {
            return portal.Room;
        }

        return null;
    }
}
