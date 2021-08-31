using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TR2RandomizerCore.Helpers;
using TR2RandomizerCore.Utilities;
using TREnvironmentEditor;
using TREnvironmentEditor.Model;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRGE.Core;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Model.Textures;

namespace TR2RandomizerCore.Randomizers
{
    public class EnvironmentRandomizer : RandomizerBase
    {
        private const int _sectorSize = 1024;
        private const int _east = (short.MaxValue + 1) / 2;
        private const int _north = 0;
        private const int _west = _east * -1;
        private const int _south = short.MinValue;

        public bool EnforcedModeOnly { get; set; }
        public uint NumMirrorLevels { get; set; }
        public bool RandomizeWater { get; set; }
        public bool RandomizeSlots { get; set; }

        internal TexturePositionMonitorBroker TextureMonitor { get; set; }

        private List<EMType> _disallowedTypes;
        private List<TR23ScriptedLevel> _levelsToMirror;

        public override void Randomize(int seed)
        {
            _generator = new Random(seed);

            _disallowedTypes = new List<EMType>();
            if (!RandomizeWater)
            {
                _disallowedTypes.Add(EMType.Flood);
                _disallowedTypes.Add(EMType.Drain);
            }
            if (!RandomizeSlots)
            {
                _disallowedTypes.Add(EMType.MoveSlot);
            }

            _levelsToMirror = Levels.RandomSelection(_generator, (int)NumMirrorLevels);

            foreach (TR23ScriptedLevel lvl in Levels)
            {
                LoadLevelInstance(lvl);

                RandomizeEnvironment(_levelInstance);

                SaveLevelInstance();

                if (!TriggerProgress())
                {
                    break;
                }
            }
        }

        private void RandomizeEnvironment(TR2CombinedLevel level)
        {
            EMEditorMapping mapping = EMEditorMapping.Get(level.Name);
            if (mapping != null)
            {
                ApplyMappingToLevel(level, mapping);
            }

            if (!EnforcedModeOnly && _levelsToMirror.Contains(level.Script))
            {
                MirrorLevel(level);
            }
        }

        private void ApplyMappingToLevel(TR2CombinedLevel level, EMEditorMapping mapping)
        {
            // Process enforced packs first. We do not pass disallowed types here.
            mapping.All.ApplyToLevel(level.Data, new EMType[] { });

            if (EnforcedModeOnly)
            {
                return;
            }

            if (mapping.Any.Count > 0)
            {
                // Pick a random number of packs to apply, but at least 1
                int packCount = _generator.Next(1, mapping.Any.Count + 1);
                List<EMEditorSet> randomSet = mapping.Any.RandomSelection(_generator, packCount);
                foreach (EMEditorSet mod in randomSet)
                {
                    mod.ApplyToLevel(level.Data, _disallowedTypes);
                }
            }

            // AllWithin means one from each set will be applied. Used for the likes of choosing a new
            // keyhole position from a set.
            if (mapping.AllWithin.Count > 0)
            {
                foreach (List<EMEditorSet> modList in mapping.AllWithin)
                {
                    EMEditorSet mod = modList[_generator.Next(0, modList.Count)];
                    mod.ApplyToLevel(level.Data, _disallowedTypes);
                }
            }

            // OneOf is used for a leader-follower situation, but where only one follower from
            // a group is wanted. An example is removing a ladder (the leader) and putting it in 
            // a different position, so the followers are the different positions from which we pick one.
            if (mapping.OneOf.Count > 0)
            {
                foreach (EMEditorGroupedSet mod in mapping.OneOf)
                {
                    EMEditorSet follower = mod.Followers[_generator.Next(0, mod.Followers.Length)];
                    mod.ApplyToLevel(level.Data, follower, _disallowedTypes);
                }
            }
        }

        private void MirrorLevel(TR2CombinedLevel level)
        {
            MirrorFloorData(level);

            // Work out the width of the world
            int globalMaxX = 0;
            foreach (TR2Room room in level.Data.Rooms)
            {
                globalMaxX = Math.Max(globalMaxX, room.Info.X + _sectorSize * room.NumXSectors);
            }

            // Shift the world 50% left and take entities with it
            int globalMidX = globalMaxX / 2;

            List<TR2Entity> entities = level.Data.Entities.ToList();
            List<TRCamera> cameras = level.Data.Cameras.ToList();
            // Sinks aren't handled yet, but are included here so we only change actual cameras.
            // For sinks, the Room is actually the strength of the current, so we need to work
            // out world coordinate repositioning for these.
            List<TRCamera> sinks = GetSinks(level.Data);

            // Collect unique texture references from each of the rooms
            ISet<ushort> textureReferences = new HashSet<ushort>();

            for (int rm = 0; rm < level.Data.NumRooms; rm++)
            {
                TR2Room room = level.Data.Rooms[rm];

                // Convert X pos to room space
                List<TR2Entity> roomEntities = entities.FindAll(e => e.Room == rm);
                List<TRCamera> roomCameras = cameras.FindAll(c => c.Room == rm && !sinks.Contains(c));
                List<TR2RoomLight> roomLights = room.Lights.ToList();

                roomEntities.ForEach(e => e.X -= room.Info.X);
                roomCameras.ForEach(c => c.X -= room.Info.X);
                roomLights.ForEach(l => l.X -= room.Info.X);

                // Map needed for static meshes to temporarily store negative values (X is uint)
                Dictionary<TR2RoomStaticMesh, int> roomStaticMeshPositions = new Dictionary<TR2RoomStaticMesh, int>();
                foreach (TR2RoomStaticMesh mesh in room.StaticMeshes)
                {
                    roomStaticMeshPositions[mesh] = (int)(mesh.X - room.Info.X);
                }

                // Flip the room
                room.Info.X -= globalMidX;
                room.Info.X *= -1;
                room.Info.X += globalMidX;
                room.Info.X -= room.NumXSectors * _sectorSize;
                Debug.Assert(room.Info.X >= 0);

                int roomOffset = room.Info.X + room.NumXSectors * _sectorSize;

                // Move the entities to their new spots
                RelocateEntities(roomEntities, roomOffset);

                // Move the cameras to their new spots
                foreach (TRCamera camera in roomCameras)
                {
                    // Move it to the other side of the room
                    camera.X *= -1;
                    // Move it back to world coords
                    camera.X += roomOffset;

                    Debug.Assert(camera.X >= 0);
                }

                // Move the lights to their new spots
                foreach (TR2RoomLight light in roomLights)
                {
                    // Move it to the other side of the room
                    light.X *= -1;
                    // Move it back to world coords
                    light.X += roomOffset;

                    Debug.Assert(light.X >= 0);
                }

                // Move the static meshes
                foreach (TR2RoomStaticMesh mesh in roomStaticMeshPositions.Keys)
                {
                    int x = roomStaticMeshPositions[mesh] * -1;
                    x += roomOffset;
                    Debug.Assert(x >= 0);
                    mesh.X = (uint)x;
                    // Rotation, flip +/-X
                    int degrees = (int)(mesh.Rotation / 16384.0f * -90);
                    if (degrees == -90)
                    {
                        degrees = -270;
                    }
                    else if (degrees == -270)
                    {
                        degrees = -90;
                    }
                    mesh.Rotation = (ushort)(degrees * 16384 / -90);
                }

                // Flip the face vertices
                int mid = room.NumXSectors / 2;
                foreach (TR2RoomVertex vert in room.RoomData.Vertices)
                {
                    int sectorX = vert.Vertex.X / _sectorSize;
                    int newSectorX = room.NumXSectors - sectorX;
                    vert.Vertex.X = (short)(newSectorX * _sectorSize);
                    Debug.Assert(vert.Vertex.X >= 0);
                }

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

                // Change visibility portal vertices and flip the normal for X
                foreach (TRRoomPortal portal in room.Portals)
                {
                    foreach (TRVertex vert in portal.Vertices)
                    {
                        int sectorX = (int)Math.Round((double)vert.X / _sectorSize);
                        int newSectorX = room.NumXSectors - sectorX;
                        vert.X = (short)(newSectorX * _sectorSize);
                        Debug.Assert(vert.X >= 0);
                    }
                    portal.Normal.X *= -1;
                }

                UpdateBoxes(level.Data, room);
            }

            MirrorTextures(level, textureReferences);
        }

        private void UpdateBoxes(TR2Level level, TR2Room room)
        {
            // The order still needs investigation as AI pathfinding isn't quite right.
            // The following ensures the boxes line up properly with the sector positioning.
            // TODO: read into overlapping, but initial thoughts are that these will need to 
            // be updated by mapping the "old" sector box indices to new.

            int roomX = room.Info.X / _sectorSize;
            int roomZ = room.Info.Z / _sectorSize;
            for (int i = 0; i < room.SectorList.Length; i++)
            {
                TRRoomSector sector = room.SectorList[i];
                if (sector.BoxIndex != ushort.MaxValue)
                {
                    TR2Box box = level.Boxes[sector.BoxIndex];
                    int boxXDiff = box.XMax - box.XMin;
                    int boxZDiff = box.ZMax - box.ZMin;

                    int sectorX = i / room.NumZSectors;
                    int sectorZ = i % room.NumZSectors;
                    sectorX += roomX;
                    sectorZ += roomZ;

                    box.XMin = (byte)sectorX;
                    box.XMax = (byte)(box.XMin + boxXDiff);
                    box.ZMin = (byte)sectorZ;
                    box.ZMax = (byte)(box.ZMin + boxZDiff);
                }
            }
        }

        private List<TRCamera> GetSinks(TR2Level level)
        {
            List<TRCamera> sinks = new List<TRCamera>();

            FDControl control = new FDControl();
            control.ParseFromLevel(level);
            List<FDActionListItem> currentActions = FDUtilities.GetActionListItems(control, FDTrigAction.UnderwaterCurrent);

            foreach (FDActionListItem action in currentActions)
            {
                TRCamera sink = level.Cameras[action.Parameter];
                if (!sinks.Contains(sink))
                {
                    sinks.Add(sink);
                }
            }

            return sinks;
        }

        private void RelocateEntities(List<TR2Entity> roomEntities, int roomOffset)
        {
            foreach (TR2Entity entity in roomEntities)
            {
                // Move it to the other side of the room
                entity.X *= -1;
                // Move it back to world coords
                entity.X += roomOffset;

                AdjustEntityPosition(entity);

                Debug.Assert(entity.X >= 0);
            }

            // Double doors need to be swapped otherwise they open in the wrong direction
            List<TR2Entities> doorTypes = new List<TR2Entities>
            {
                TR2Entities.Door1, TR2Entities.Door2, TR2Entities.Door3,
                TR2Entities.Door4, TR2Entities.Door5, TR2Entities.LiftingDoor1,
                TR2Entities.LiftingDoor2, TR2Entities.LiftingDoor3
            };
            List<TR2Entity> doors = roomEntities.FindAll(e => doorTypes.Contains((TR2Entities)e.TypeID));

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

                    // if the difference between X or Z position is one sector size and they share the same Y val, they're double doors
                    if (door1.TypeID != door2.TypeID && door1.Y == door2.Y && (Math.Abs(door1.X - door2.X) == _sectorSize || Math.Abs(door1.Z - door2.Z) == _sectorSize))
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
                case TR2Entities.SpikyWall:
                    switch (entity.Angle)
                    {
                        case _south:
                            entity.X += _sectorSize;
                            break;
                        case _west:
                            entity.Z -= _sectorSize;
                            break;
                        case _north:
                            entity.X -= _sectorSize;
                            break;
                        case _east:
                            entity.Z += _sectorSize;
                            break;
                    }
                    break;
                case TR2Entities.Gong: // case 0 applicable to IceCave
                    switch (entity.Angle)
                    {
                        case _south:
                            entity.X -= _sectorSize;
                            break;
                        case _west:
                            entity.Z += _sectorSize;
                            break;
                        case _north:
                            entity.X += _sectorSize;
                            break;
                        case _east:
                            entity.Z -= _sectorSize;
                            break;
                    }
                    break;

                // This looks odd if flipped, so reset to standard
                case TR2Entities.WallMountedKnifeBlade:
                    if (entity.Angle == _north)
                    {
                        entity.Angle = _south;
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
            }
        }

        private void MirrorFloorData(TR2CombinedLevel level)
        {
            FDControl control = new FDControl();
            control.ParseFromLevel(level.Data);

            foreach (TR2Room room in level.Data.Rooms)
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

                // We are flipping over the X axis, so we just reverse the sector list
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
                                //If the X slope is greater than zero, then its value is added to the floor heights of corners 00 and 01.
                                //If it is less than zero, then its value is subtracted from the floor heights of corners 10 and 11.
                                if (slantEntry.XSlant != 0)
                                {
                                    slantEntry.XSlant *= -1;
                                }
                                if (slantEntry.XSlant != 0 && slantEntry.ZSlant != 0)
                                {
                                    // TODO
                                }
                            }
                            else if (entry is FDClimbEntry climbEntry)
                            {
                                if (climbEntry.IsNegativeX)
                                {
                                    climbEntry.IsNegativeX = false;
                                    climbEntry.IsPositiveX = true;
                                }
                                else if (climbEntry.IsPositiveX)
                                {
                                    climbEntry.IsPositiveX = false;
                                    climbEntry.IsNegativeX = true;
                                }
                            }
                        }
                    }
                }
            }

            control.WriteToLevel(level.Data);
        }

        private void MirrorTextures(TR2CombinedLevel level, ISet<ushort> textureReferences)
        {
            // Basically flip the vertices in the same way as done for faces
            foreach (ushort textureRef in textureReferences)
            {
                IndexedTRObjectTexture texture = new IndexedTRObjectTexture
                {
                    Texture = level.Data.ObjectTextures[textureRef]
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

            // Notify the texture monitor that this level has been flipped
            TexturePositionMonitor monitor = TextureMonitor.CreateMonitor(level.Name);
            monitor.UseMirroring = true;
        }

        private static void Swap<T>(T[] arr, int pos1, int pos2)
        {
            T temp = arr[pos1];
            arr[pos1] = arr[pos2];
            arr[pos2] = temp;
        }
    }
}