using System;
using System.Collections.Generic;
using System.Linq;
using TR2RandomizerCore.Helpers;
using TREnvironmentEditor;
using TREnvironmentEditor.Model;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRGE.Core;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;

namespace TR2RandomizerCore.Randomizers
{
    public class EnvironmentRandomizer : RandomizerBase
    {
        public bool EnforcedModeOnly { get; set; }
        public uint NumMirrorLevels { get; set; }
        public bool RandomizeWater { get; set; }
        public bool RandomizeSlots { get; set; }

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

            if (_levelsToMirror.Contains(level.Script))
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

            int sectorSize = 1024;

            // Work out the width of the world
            int globalMaxX = 0;
            foreach (TR2Room room in level.Data.Rooms)
            {
                globalMaxX = Math.Max(globalMaxX, room.Info.X + sectorSize * room.NumXSectors);
            }

            // Shift the world 50% left and take entities with it
            List<TR2Entity> entities = level.Data.Entities.ToList();
            List<TRCamera> cameras = level.Data.Cameras.ToList();
            int globalMidX = globalMaxX / 2;
            for (int rm = 0; rm < level.Data.NumRooms; rm++)
            {
                TR2Room room = level.Data.Rooms[rm];

                // Convert X pos to room space
                List<TR2Entity> roomEntities = entities.FindAll(e => e.Room == rm);
                List<TRCamera> roomCameras = cameras.FindAll(e => e.Room == rm);
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
                room.Info.X -= room.NumXSectors * sectorSize;

                // Move the entities to their new spots
                for (int i = 0; i < roomEntities.Count; i++)
                {
                    TR2Entity ent = roomEntities[i];
                    // Move it to the other side of the room
                    ent.X *= -1;
                    // Move it back to world coords
                    ent.X += room.Info.X + room.NumXSectors * sectorSize;
                    // If it's facing +/-X direction, flip it
                    if (Math.Abs((int)ent.Angle) == 16384)
                    {
                        ent.Angle *= -1;
                    }

                    // These take up 2 tiles so need some fiddling
                    if (ent.TypeID == (short)TR2Entities.SpikyWall)
                    {
                        switch (ent.Angle)
                        {
                            case short.MinValue:
                                ent.X += sectorSize;
                                break;
                            case -16384:
                                ent.Z -= sectorSize;
                                break;
                            case 0:
                                ent.X -= sectorSize;
                                break;
                            case 16384:
                                ent.Z += sectorSize;
                                break;
                        }
                    }
                    else if (ent.TypeID == (short)TR2Entities.WallMountedKnifeBlade)
                    {
                        // Doesn't look right otherwise
                        switch (ent.Angle)
                        {
                            case 0:
                                ent.Angle = short.MinValue;
                                break;
                        }
                    }
                }

                // Move the cameras to their new spots
                foreach (TRCamera camera in roomCameras)
                {
                    // Move it to the other side of the room
                    camera.X *= -1;
                    // Move it back to world coords
                    camera.X += room.Info.X + room.NumXSectors * sectorSize;
                }

                // Move the lights to their new spots
                foreach (TR2RoomLight light in roomLights)
                {
                    // Move it to the other side of the room
                    light.X *= -1;
                    // Move it back to world coords
                    light.X += room.Info.X + room.NumXSectors * sectorSize;
                }

                // Move the static meshes
                foreach (TR2RoomStaticMesh mesh in roomStaticMeshPositions.Keys)
                {
                    int x = roomStaticMeshPositions[mesh] * -1;
                    x += room.Info.X + room.NumXSectors * 1024;
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
                    int sectorX = vert.Vertex.X / sectorSize;
                    int newSectorX = room.NumXSectors - (sectorX + 1);
                    vert.Vertex.X = (short)(newSectorX * sectorSize);
                    vert.Vertex.X += (short)sectorSize; // Not sure why at the moment, but this makes floor data line up
                }

                // Dirty hack to invert the faces - not final!
                foreach (TRFace4 f in room.RoomData.Rectangles)
                {
                    Swap(f.Vertices, 0, 3);
                    Swap(f.Vertices, 1, 2);
                }

                foreach (TRFace3 f in room.RoomData.Triangles)
                {
                    Swap(f.Vertices, 0, 2);
                }

                foreach (TRRoomPortal portal in room.Portals)
                {
                    foreach (TRVertex vert in portal.Vertices)
                    {
                        int sectorX = (int)Math.Round((double)vert.X / sectorSize);
                        int newSectorX = room.NumXSectors - (sectorX + 1);
                        vert.X = (short)(newSectorX * sectorSize);
                        vert.X += (short)sectorSize;
                    }
                    portal.Normal.X *= -1;
                }
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
                                // Setting these values needs investigating as it
                                // seems to break some triggers. See RM81 in GW.
                                //bool isNegX = climbEntry.IsNegativeX;
                                //bool isPosX = climbEntry.IsPositiveX;
                                //if (isNegX)
                                //{
                                //    climbEntry.IsNegativeX = !climbEntry.IsNegativeX;
                                //    climbEntry.IsPositiveX = true;
                                //}
                                //if (isPosX)
                                //{
                                //    climbEntry.IsPositiveX = !climbEntry.IsPositiveX;
                                //    climbEntry.IsNegativeX = true;
                                //}
                            }
                        }
                    }
                }
            }

            control.WriteToLevel(level.Data);
        }

        private static void Swap<T>(T[] arr, int pos1, int pos2)
        {
            T temp = arr[pos1];
            arr[pos1] = arr[pos2];
            arr[pos2] = temp;
        }
    }
}