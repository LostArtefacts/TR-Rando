using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Model.Textures;

namespace TREnvironmentEditor.Model.Types
{
    public class EMMirrorFunction : BaseEMFunction
    {
        private const int _east = (short.MaxValue + 1) / 2;
        private const int _north = 0;
        private const int _west = _east * -1;
        private const int _south = short.MinValue;

        private int _worldWidth;

        public override void ApplyToLevel(TR2Level level)
        {
            CalculateWorldWidth(level);

            MirrorFloorData(level);
            MirrorRooms(level);
            MirrorBoxes(level);

            MirrorStaticMeshes(level);
            MirrorEntities(level);
            MirrorNullMeshes(level);

            MirrorTextures(level);
        }

        private void CalculateWorldWidth(TR2Level level)
        {
            _worldWidth = 0;
            foreach (TR2Room room in level.Rooms)
            {
                _worldWidth = Math.Max(_worldWidth, room.Info.X + SectorSize * room.NumXSectors);
            }
        }

        private int FlipWorldX(int x)
        {
            // Shift the point 100% to the left, then flip it back to +
            x -= _worldWidth;
            x *= -1;
            Debug.Assert(x >= 0);
            return x;
        }

        private static void Swap<T>(T[] arr, int pos1, int pos2)
        {
            T temp = arr[pos1];
            arr[pos1] = arr[pos2];
            arr[pos2] = temp;
        }

        private void MirrorFloorData(TR2Level level)
        {
            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            foreach (TR2Room room in level.Rooms)
            {
                // Convert the flattened sector list to 2D
                List<TRRoomSector> sectors = room.SectorList.ToList();
                List<List<TRRoomSector>> sectorMap = new List<List<TRRoomSector>>();
                for (int x = 0; x < room.NumXSectors; x++)
                {
                    sectorMap.Add(new List<TRRoomSector>());
                    for (int z = 0; z < room.NumZSectors; z++)
                    {
                        sectorMap[x].Add(sectors[z + x * room.NumZSectors]);
                    }
                }

                // We are flipping X, so we just reverse the list of sector lists
                sectorMap.Reverse();
                sectors.Clear();
                foreach (List<TRRoomSector> sectorList in sectorMap)
                {
                    sectors.AddRange(sectorList);
                }
                room.SectorList = sectors.ToArray();

                // Change slants and climbable entries
                foreach (TRRoomSector sector in sectors)
                {
                    if (sector.FDIndex != 0)
                    {
                        List<FDEntry> entries = control.Entries[sector.FDIndex];
                        foreach (FDEntry entry in entries)
                        {
                            if (entry is FDSlantEntry slantEntry)
                            {
                                // If the X slope is greater than zero, then its value is added to the floor heights of corners 00 and 01.
                                // If it is less than zero, then its value is subtracted from the floor heights of corners 10 and 11.
                                slantEntry.XSlant *= -1;
                            }
                            else if (entry is FDClimbEntry climbEntry)
                            {
                                // We only need to flip the direction if it's exclusively set in +/- X direction.
                                if (climbEntry.IsNegativeX ^ climbEntry.IsPositiveX)
                                {
                                    climbEntry.IsNegativeX = !(climbEntry.IsPositiveX ^= true);
                                }
                            }
                        }
                    }
                }
            }

            control.WriteToLevel(level);
        }

        private void MirrorRooms(TR2Level level)
        {
            foreach (TR2Room room in level.Rooms)
            {
                room.Info.X = FlipWorldX(room.Info.X);
                room.Info.X -= room.NumXSectors * SectorSize;
                Debug.Assert(room.Info.X >= 0);

                // Flip the face vertices
                int mid = room.NumXSectors / 2;
                foreach (TR2RoomVertex vert in room.RoomData.Vertices)
                {
                    int sectorX = vert.Vertex.X / SectorSize;
                    int newSectorX = room.NumXSectors - sectorX;
                    vert.Vertex.X = (short)(newSectorX * SectorSize);
                    Debug.Assert(vert.Vertex.X >= 0);
                }

                // Change visibility portal vertices and flip the normal for X
                foreach (TRRoomPortal portal in room.Portals)
                {
                    foreach (TRVertex vert in portal.Vertices)
                    {
                        int sectorX = (int)Math.Round((double)vert.X / SectorSize);
                        int newSectorX = room.NumXSectors - sectorX;
                        vert.X = (short)(newSectorX * SectorSize);
                        Debug.Assert(vert.X >= 0);
                    }
                    portal.Normal.X *= -1;
                }

                // Move the lights to their new spots
                foreach (TR2RoomLight light in room.Lights)
                {
                    light.X = FlipWorldX(light.X);
                }

                // Move the static meshes
                foreach (TR2RoomStaticMesh mesh in room.StaticMeshes)
                {
                    mesh.X = (uint)FlipWorldX((int)mesh.X);

                    // Convert the angle to short units for consistency and then flip it if +/-X
                    int angle = mesh.Rotation + _south;
                    if (angle == _east || angle == _west)
                    {
                        angle *= -1;
                        angle -= _south;
                        mesh.Rotation = (ushort)angle;
                    }
                }
            }
        }

        private void MirrorBoxes(TR2Level level)
        {
            // Boxes do not necessarily cover only one sector and several sectors can point
            // to the same box. So we need to work out the smallest new X position for shared
            // boxes and update each one only once. The XMax value is simply the previous Max/Min 
            // difference added to the new XMin value.
            Dictionary<TR2Box, int> boxPositionMap = new Dictionary<TR2Box, int>();

            foreach (TR2Room room in level.Rooms)
            {
                int roomX = room.Info.X / SectorSize;
                for (int i = 0; i < room.SectorList.Length; i++)
                {
                    TRRoomSector sector = room.SectorList[i];
                    if (sector.BoxIndex != ushort.MaxValue)
                    {
                        TR2Box box = level.Boxes[sector.BoxIndex];

                        // Where is this sector in the world?
                        int sectorX = i / room.NumZSectors;
                        sectorX += roomX;

                        if (!boxPositionMap.ContainsKey(box))
                        {
                            boxPositionMap[box] = sectorX;
                        }
                        else
                        {
                            boxPositionMap[box] = Math.Min(boxPositionMap[box], sectorX);
                        }
                    }
                }
            }

            foreach (TR2Box box in boxPositionMap.Keys)
            {
                int boxXDiff = box.XMax - box.XMin;
                box.XMin = (byte)boxPositionMap[box];
                box.XMax = (byte)(box.XMin + boxXDiff);
            }
        }

        private void MirrorStaticMeshes(TR2Level level)
        {
            foreach (TRStaticMesh staticMesh in level.StaticMeshes)
            {
                // Get the actual mesh
                TRMesh mesh = TR2LevelUtilities.GetMesh(level, staticMesh.Mesh);

                // Move each vertex to the other side of the mesh. Negative values
                // are supported so we needn't worry about shifting.
                foreach (TRVertex vert in mesh.Vertices)
                {
                    vert.X *= -1;
                }

                // Flip the MinX and MaxX bounding box values
                FlipBoundingBox(staticMesh.CollisionBox);
                FlipBoundingBox(staticMesh.VisibilityBox);
            }
        }

        private void FlipBoundingBox(TRBoundingBox box)
        {
            short min = box.MinX;
            short max = box.MaxX;
            box.MinX = (short)(max * -1);
            box.MaxX = (short)(min * -1);
        }

        private void MirrorEntities(TR2Level level)
        {
            foreach (TR2Entity entity in level.Entities)
            {
                entity.X = FlipWorldX(entity.X);
                AdjustEntityPosition(entity);
            }

            AdjustDoors(level);
        }

        private void AdjustEntityPosition(TR2Entity entity)
        {
            // If it's facing +/-X direction, flip it
            if (entity.Angle == _east || entity.Angle == _west)
            {
                entity.Angle *= -1;
            }

            switch ((TR2Entities)entity.TypeID)
            {
                // These take up 2 tiles so need some fiddling
                case TR2Entities.Elevator:
                case TR2Entities.SpikyCeiling:
                case TR2Entities.SpikyWall:
                    switch (entity.Angle)
                    {
                        case _south:
                            entity.X += SectorSize;
                            break;
                        case _west:
                            entity.Z -= SectorSize;
                            break;
                        case _north:
                            entity.X -= SectorSize;
                            break;
                        case _east:
                            entity.Z += SectorSize;
                            break;
                    }
                    break;
                case TR2Entities.Gong: // case 0 applicable to IceCave
                    switch (entity.Angle)
                    {
                        case _south:
                            entity.X -= SectorSize;
                            break;
                        case _west:
                            entity.Z += SectorSize;
                            break;
                        case _north:
                            entity.X += SectorSize;
                            break;
                        case _east:
                            entity.Z -= SectorSize;
                            break;
                    }
                    break;

                // These look odd if flipped, so reset to standard
                case TR2Entities.WallMountedKnifeBlade:
                    if (entity.Angle == _north)
                    {
                        entity.Angle = _south;
                    }
                    break;
                case TR2Entities.StatueWithKnifeBlade:
                    if (entity.Angle == _east)
                    {
                        entity.Angle = _west;
                        entity.X += SectorSize;
                    }
                    break;

                // Bridge tilts need to be rotated
                case TR2Entities.BridgeTilt1:
                case TR2Entities.BridgeTilt2:
                    switch (entity.Angle)
                    {
                        case _south:
                            entity.Angle = _north;
                            break;
                        case _west:
                            entity.Angle = _east;
                            break;
                        case _north:
                            entity.Angle = _south;
                            break;
                        case _east:
                            entity.Angle = _west;
                            break;
                    }
                    break;

                case TR2Entities.AirplanePropeller:
                    if (entity.Angle == _west)
                    {
                        entity.Angle = _east;
                    }
                    break;

                case TR2Entities.OverheadPulleyHook:
                    if (entity.Angle == _south || entity.Angle == _north)
                    {
                        entity.Angle += _south;
                    }
                    break;

                case TR2Entities.PowerSaw:
                    if (entity.Angle == _north)
                    {
                        entity.X += SectorSize;
                    }
                    break;

                case TR2Entities.Helicopter:
                    if (entity.Angle == _west)
                    {
                        entity.Angle = _north;
                        entity.X += SectorSize;
                        entity.Z += SectorSize;
                    }
                    break;

                case TR2Entities.MarcoBartoli:
                    // InitialiseBartoli in Dragon.c always shifts Bartoli as follows,
                    // so we need to move him 512 in the +X to avoid him ending up either
                    // OOB or in mid-air.
                    // item->pos.x_pos -= STEP_L*2;
                    entity.X += SectorSize / 2;
                    break;
            }
        }

        private void AdjustDoors(TR2Level level)
        {
            // Double doors need to be swapped otherwise they open in the wrong direction
            List<TR2Entity> doors = level.Entities.ToList().FindAll(e => TR2EntityUtilities.IsDoorType((TR2Entities)e.TypeID));

            // Iterate backwards and try to find doors that are next to each other.
            // If found, swap their types.
            for (int i = doors.Count - 1; i >= 0; i--)
            {
                TR2Entity door1 = doors[i];
                for (int j = doors.Count - 1; j >= 0; j--)
                {
                    if (j == i)
                    {
                        continue;
                    }

                    TR2Entity door2 = doors[j];

                    if (AreDoubleDoors(door1, door2))
                    {
                        short tmp = door1.TypeID;
                        door1.TypeID = door2.TypeID;
                        door2.TypeID = tmp;

                        // Don't process these doors again, so just remove the first
                        doors.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        private bool AreDoubleDoors(TR2Entity door1, TR2Entity door2)
        {
            // If the difference between X or Z position is one sector size, they share the same Y val,
            // and they are facing the same diretion, then they're double doors.
            return door1.Room == door2.Room &&
                door1.TypeID != door2.TypeID &&
                door1.Y == door2.Y &&
                door1.Angle == door2.Angle &&
                (Math.Abs(door1.X - door2.X) == SectorSize || Math.Abs(door1.Z - door2.Z) == SectorSize);
        }

        private void MirrorNullMeshes(TR2Level level)
        {
            // The deals with actual cameras as well as sinks
            foreach (TRCamera camera in level.Cameras)
            {
                camera.X = FlipWorldX(camera.X);
            }

            foreach (TRSoundSource sound in level.SoundSources)
            {
                sound.X = FlipWorldX(sound.X);
            }

            // TODO: Handle TRCinematicFrames by working out how to mirror animation
            // frames e.g. the LaraMiscAnim that corresponds with the cinematics.
            // Currently, frames are left untouched so the Rig starting animation
            // and dragon dagger cutscene behave normally. Lara is a bit out of
            // place in the HSH cinematics for now.
        }

        private void MirrorTextures(TR2Level level)
        {
            // Collect unique texture references from each of the rooms
            ISet<ushort> textureReferences = new HashSet<ushort>();

            // Keep track of static meshes so they are only processed once,
            // and so we only target those actually in use in rooms.
            List<TRStaticMesh> staticMeshes = level.StaticMeshes.ToList();
            ISet<TRStaticMesh> processedMeshes = new HashSet<TRStaticMesh>();

            foreach (TR2Room room in level.Rooms)
            {
                // Invert the faces, otherwise they are inside out
                foreach (TRFace4 f in room.RoomData.Rectangles)
                {
                    Swap(f.Vertices, 0, 3);
                    Swap(f.Vertices, 1, 2);
                    textureReferences.Add(f.Texture);
                }

                foreach (TRFace3 f in room.RoomData.Triangles)
                {
                    Swap(f.Vertices, 0, 2);
                    textureReferences.Add(f.Texture);
                }

                foreach (TR2RoomStaticMesh roomStaticMesh in room.StaticMeshes)
                {
                    TRStaticMesh staticMesh = staticMeshes.Find(m => m.ID == roomStaticMesh.MeshID);
                    if (!processedMeshes.Add(staticMesh))
                    {
                        continue;
                    }

                    TRMesh mesh = TR2LevelUtilities.GetMesh(level, staticMesh.Mesh);

                    // Flip the faces and store texture references
                    foreach (TRFace4 f in mesh.TexturedRectangles)
                    {
                        Swap(f.Vertices, 0, 3);
                        Swap(f.Vertices, 1, 2);
                        textureReferences.Add(f.Texture);
                    }

                    foreach (TRFace4 f in mesh.ColouredRectangles)
                    {
                        Swap(f.Vertices, 0, 3);
                        Swap(f.Vertices, 1, 2);
                    }

                    foreach (TRFace3 f in mesh.TexturedTriangles)
                    {
                        Swap(f.Vertices, 0, 2);
                        textureReferences.Add(f.Texture);
                    }

                    foreach (TRFace3 f in mesh.ColouredTriangles)
                    {
                        Swap(f.Vertices, 0, 2);
                    }
                }
            }
                        
            // Include all animated texture references too
            foreach (TRAnimatedTexture anim in level.AnimatedTextures)
            {
                for (int i = 0; i < anim.Textures.Length; i++)
                {
                    textureReferences.Add(anim.Textures[i]);
                }
            }

            // Flip the object texture vertices in the same way as done for faces
            foreach (ushort textureRef in textureReferences)
            {
                IndexedTRObjectTexture texture = new IndexedTRObjectTexture
                {
                    Texture = level.ObjectTextures[textureRef]
                };

                if (texture.IsTriangle)
                {
                    Swap(texture.Texture.Vertices, 0, 2);
                }
                else
                {
                    Swap(texture.Texture.Vertices, 0, 3);
                    Swap(texture.Texture.Vertices, 1, 2);
                }
            }
        }
    }
}