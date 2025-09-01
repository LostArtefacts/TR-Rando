using System.Diagnostics;
using TRLevelControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRDataControl.Environment;

public class EMMirrorFunction : BaseEMFunction
{
    private const int _east = (short.MaxValue + 1) / 2;
    private const int _north = 0;
    private const int _west = _east * -1;
    private const int _south = short.MinValue;

    private int _worldWidth, _xAdjustment;

    public override void ApplyToLevel(TR1Level level)
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

    public override void ApplyToLevel(TR3Level level)
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

    private void CalculateWorldWidth(TR1Level level)
    {
        _worldWidth = 0;
        _xAdjustment = 0;
        foreach (TR1Room room in level.Rooms)
        {
            _worldWidth = Math.Max(_worldWidth, room.Info.X + TRConsts.Step4 * room.NumXSectors);
        }
    }

    private void CalculateWorldWidth(TR2Level level)
    {
        _worldWidth = 0;
        _xAdjustment = 0;
        foreach (TR2Room room in level.Rooms)
        {
            _worldWidth = Math.Max(_worldWidth, room.Info.X + TRConsts.Step4 * room.NumXSectors);
        }
    }

    private void CalculateWorldWidth(TR3Level level)
    {
        _worldWidth = 0;
        _xAdjustment = 0;
        foreach (TR3Room room in level.Rooms)
        {
            _worldWidth = Math.Max(_worldWidth, room.Info.X + TRConsts.Step4 * room.NumXSectors);
        }

        TR3Entity puna = level.Entities.Find(e => e.TypeID == TR3Type.Puna);
        if (puna != null)
        {
            // Rebuild the world around Puna's Lizard
            TR3Entity lizardMan = level.Entities.Find(e => e.Room == puna.Room && e.TypeID == TR3Type.LizardMan);
            _xAdjustment = lizardMan.X - FlipWorldX(lizardMan.X);
        }
    }

    private int FlipWorldX(int x)
    {
        // Shift the point 100% to the left, then flip it back to +. If we have a level such as Puna
        // that's been built around particular coords, adjust X.
        x -= _worldWidth;
        x *= -1;
        x += _xAdjustment;
        Debug.Assert(x >= 0);
        return x;
    }

    private static void MirrorFloorData(TR1Level level)
    {
        foreach (TR1Room room in level.Rooms)
        {
            MirrorSectors(room.Sectors, room.NumXSectors, room.NumZSectors, level.FloorData);
        }
    }

    private static void MirrorFloorData(TR2Level level)
    {
        foreach (TR2Room room in level.Rooms)
        {
            MirrorSectors(room.Sectors, room.NumXSectors, room.NumZSectors, level.FloorData);
        }
    }

    private static void MirrorFloorData(TR3Level level)
    {
        foreach (TR3Room room in level.Rooms)
        {
            MirrorSectors(room.Sectors, room.NumXSectors, room.NumZSectors, level.FloorData);
        }
    }

    private static void MirrorSectors(List<TRRoomSector> sectors, ushort numXSectors, ushort numZSectors, FDControl floorData)
    {
        // Convert the flattened sector list to 2D            
        List<List<TRRoomSector>> sectorMap = new();
        for (int x = 0; x < numXSectors; x++)
        {
            sectorMap.Add(new List<TRRoomSector>());
            for (int z = 0; z < numZSectors; z++)
            {
                sectorMap[x].Add(sectors[z + x * numZSectors]);
            }
        }

        // We are flipping X, so we just reverse the list of sector lists
        sectorMap.Reverse();
        sectors.Clear();
        foreach (List<TRRoomSector> sectorList in sectorMap)
        {
            sectors.AddRange(sectorList);
        }

        // Change slants and climbable entries
        foreach (TRRoomSector sector in sectors.DistinctBy(s => s.FDIndex))
        {
            if (sector.FDIndex != 0)
            {
                List<FDEntry> entries = floorData[sector.FDIndex];
                for (int i = 0; i < entries.Count; i++)
                {
                    FDEntry entry = entries[i];
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
                    else if (entry is FDTriangulationEntry triangulation)
                    {
                        // Flip the corners
                        byte c00 = triangulation.C00;
                        byte c10 = triangulation.C10;
                        byte c01 = triangulation.C01;
                        byte c11 = triangulation.C11;
                        triangulation.C00 = c10;
                        triangulation.C10 = c00;
                        triangulation.C01 = c11;
                        triangulation.C11 = c01;

                        // And the heights
                        sbyte h1 = triangulation.H1;
                        sbyte h2 = triangulation.H2;
                        triangulation.H1 = h2;
                        triangulation.H2 = h1;

                        // And the triangulation
                        switch (triangulation.Type)
                        {
                            // Non-portals
                            case FDTriangulationType.FloorNWSE_Solid:
                                triangulation.Type = FDTriangulationType.FloorNESW_Solid;
                                break;
                            case FDTriangulationType.FloorNESW_Solid:
                                triangulation.Type = FDTriangulationType.FloorNWSE_Solid;
                                break;

                            case FDTriangulationType.CeilingNWSE_Solid:
                                triangulation.Type = FDTriangulationType.CeilingNESW_Solid;
                                break;
                            case FDTriangulationType.CeilingNESW_Solid:
                                triangulation.Type = FDTriangulationType.CeilingNWSE_Solid;
                                break;

                            // Portals: _SW, _NE etc indicate triangles whose right-angles point towards the portal
                            case FDTriangulationType.FloorNWSE_SW:
                                triangulation.Type = FDTriangulationType.FloorNESW_NW;
                                break;
                            case FDTriangulationType.FloorNWSE_NE:
                                triangulation.Type = FDTriangulationType.FloorNESW_SE;
                                break;
                            case FDTriangulationType.FloorNESW_SE:
                                triangulation.Type = FDTriangulationType.FloorNWSE_NE;
                                break;
                            case FDTriangulationType.FloorNESW_NW:
                                triangulation.Type = FDTriangulationType.FloorNWSE_SW;
                                break;

                            case FDTriangulationType.CeilingNWSE_SW:
                                triangulation.Type = FDTriangulationType.CeilingNESW_SE;
                                break;
                            case FDTriangulationType.CeilingNWSE_NE:
                                triangulation.Type = FDTriangulationType.CeilingNESW_NW;
                                break;
                            case FDTriangulationType.CeilingNESW_NW:
                                triangulation.Type = FDTriangulationType.CeilingNWSE_NE;
                                break;
                            case FDTriangulationType.CeilingNESW_SE:
                                triangulation.Type = FDTriangulationType.CeilingNWSE_SW;
                                break;
                        }
                    }
                    else if (entry is FDMinecartEntry minecart)
                    {
                        // If left is followed by right, it means stop the minecart and they appear to
                        // need to remain in this order. Only switch the entry if there is no other.
                        if (minecart.Type == FDMinecartType.Left)
                        {
                            if (!(i < entries.Count - 1 && entries[i + 1] is FDMinecartEntry mincartRight && mincartRight.Type == FDMinecartType.Right))
                            {
                                entries[i] = new FDMinecartEntry
                                {
                                    Type = FDMinecartType.Right
                                };
                            }
                        }
                        else if (!(i > 0 && entries[i - 1] is FDMinecartEntry minecartLeft && minecartLeft.Type == FDMinecartType.Left))
                        {
                            entries[i] = new FDMinecartEntry
                            {
                                Type= FDMinecartType.Left
                            };
                        }
                    }
                }
            }
        }
    }

    private void MirrorRooms(TR1Level level)
    {
        foreach (TR1Room room in level.Rooms)
        {
            int oldRoomX = room.Info.X;
            room.Info.X = FlipWorldX(oldRoomX);
            room.Info.X -= room.NumXSectors * TRConsts.Step4;
            Debug.Assert(room.Info.X >= 0);

            MirrorRoomMesh(room.Mesh, oldRoomX, room.Info.X, room.NumXSectors);

            // Change visibility portal vertices and flip the normal for X
            foreach (TRRoomPortal portal in room.Portals)
            {
                foreach (TRVertex vert in portal.Vertices)
                {
                    int sectorX = (int)Math.Round((double)vert.X / TRConsts.Step4);
                    int newSectorX = room.NumXSectors - sectorX;
                    vert.X = (short)(newSectorX * TRConsts.Step4);
                    Debug.Assert(vert.X >= 0);
                }
                portal.Normal.X *= -1;
            }

            // Move the lights to their new spots
            foreach (TR1RoomLight light in room.Lights)
            {
                light.X = FlipWorldX(light.X);
            }

            MirrorRoomStaticMeshes(room.StaticMeshes);
        }
    }

    private void MirrorRooms(TR2Level level)
    {
        foreach (TR2Room room in level.Rooms)
        {
            int oldRoomX = room.Info.X;
            room.Info.X = FlipWorldX(oldRoomX);
            room.Info.X -= room.NumXSectors * TRConsts.Step4;
            Debug.Assert(room.Info.X >= 0);

            MirrorRoomMesh(room.Mesh, oldRoomX, room.Info.X, room.NumXSectors);

            // Change visibility portal vertices and flip the normal for X
            foreach (TRRoomPortal portal in room.Portals)
            {
                foreach (TRVertex vert in portal.Vertices)
                {
                    int sectorX = (int)Math.Round((double)vert.X / TRConsts.Step4);
                    int newSectorX = room.NumXSectors - sectorX;
                    vert.X = (short)(newSectorX * TRConsts.Step4);
                    Debug.Assert(vert.X >= 0);
                }
                portal.Normal.X *= -1;
            }

            // Move the lights to their new spots
            foreach (TR2RoomLight light in room.Lights)
            {
                light.X = FlipWorldX(light.X);
            }

            MirrorRoomStaticMeshes(room.StaticMeshes);
        }
    }

    private void MirrorRooms(TR3Level level)
    {
        foreach (TR3Room room in level.Rooms)
        {
            int oldRoomX = room.Info.X;
            room.Info.X = FlipWorldX(oldRoomX);
            room.Info.X -= room.NumXSectors * TRConsts.Step4;
            Debug.Assert(room.Info.X >= 0);

            MirrorRoomMesh(room.Mesh, oldRoomX, room.Info.X, room.NumXSectors);

            // Change visibility portal vertices and flip the normal for X
            foreach (TRRoomPortal portal in room.Portals)
            {
                foreach (TRVertex vert in portal.Vertices)
                {
                    int sectorX = (int)Math.Round((double)vert.X / TRConsts.Step4);
                    int newSectorX = room.NumXSectors - sectorX;
                    vert.X = (short)(newSectorX * TRConsts.Step4);
                    Debug.Assert(vert.X >= 0);
                }
                portal.Normal.X *= -1;
            }

            // Move the lights to their new spots
            foreach (TR3RoomLight light in room.Lights)
            {
                light.X = FlipWorldX(light.X);
            }

            MirrorRoomStaticMeshes(room.StaticMeshes);
        }
    }

    private void MirrorRoomMesh<T, V>(TRRoomMesh<T, V> mesh, int oldRoomX, int newRoomX, ushort roomWidth)
        where T : Enum
        where V : TRRoomVertex
    {
        // Flip room sprites separately as they don't sit on tile edges
        List<V> processedVerts = new();
        foreach (TRRoomSprite<T> sprite in mesh.Sprites)
        {
            V roomVertex = mesh.Vertices[sprite.Vertex];

            // Flip the old world coordinate, then subtract the new room position
            int x = oldRoomX + roomVertex.Vertex.X;
            x = FlipWorldX(x);
            x -= newRoomX;
            roomVertex.Vertex.X = (short)x;

            Debug.Assert(roomVertex.Vertex.X >= 0);
            processedVerts.Add(roomVertex);
        }

        // Flip the face vertices
        foreach (V vert in mesh.Vertices)
        {
            if (processedVerts.Contains(vert))
            {
                continue;
            }

            int sectorX = vert.Vertex.X / TRConsts.Step4;
            int newSectorX = roomWidth - sectorX;
            vert.Vertex.X = (short)(newSectorX * TRConsts.Step4);
            Debug.Assert(vert.Vertex.X >= 0);
        }
    }

    private void MirrorRoomStaticMeshes<T>(IEnumerable<TRRoomStaticMesh<T>> meshes)
        where T : Enum
    {
        foreach (TRRoomStaticMesh<T> mesh in meshes)
        {
            mesh.X = FlipWorldX(mesh.X);
            if (mesh.Angle == _east || mesh.Angle == _west)
            {
                mesh.Angle *= -1;
            }
        }
    }

    private void MirrorBoxes(TR1Level level)
    {
        MirrorBoxes(level.Boxes);
    }

    private void MirrorBoxes(TR2Level level)
    {
        MirrorBoxes(level.Boxes);
    }

    private void MirrorBoxes(TR3Level level)
    {
        MirrorBoxes(level.Boxes);
    }

    private void MirrorBoxes(List<TRBox> boxes)
    {
        // Boxes do not necessarily cover only one sector and several sectors can point
        // to the same box. So we need to work out the smallest new X position for shared
        // boxes and update each one only once. This is done by converting the xmin and xmax
        // to world coordinates, flipping them over X and then swapping them.
        foreach (TRBox box in boxes)
        {
            uint newMaxX = (uint)FlipWorldX((int)box.XMin);
            uint newMinX = (uint)FlipWorldX((int)box.XMax);
            Debug.Assert(newMaxX >= newMinX);
            box.XMin = newMinX;
            box.XMax = newMaxX;
        }
    }

    private static void MirrorStaticMeshes(TR1Level level)
    {
        MirrorStaticMeshes(level.StaticMeshes.Values);
    }

    private static void MirrorStaticMeshes(TR2Level level)
    {
        MirrorStaticMeshes(level.StaticMeshes.Values);
    }

    private static void MirrorStaticMeshes(TR3Level level)
    {
        MirrorStaticMeshes(level.StaticMeshes.Values);
    }

    private static void MirrorStaticMeshes(IEnumerable<TRStaticMesh> staticMeshes)
    {
        foreach (TRStaticMesh staticMesh in staticMeshes)
        {
            foreach (TRVertex vert in staticMesh.Mesh.Vertices)
            {
                vert.X *= -1;
            }

            FlipBoundingBox(staticMesh.CollisionBox);
            FlipBoundingBox(staticMesh.VisibilityBox);
        }
    }

    private static void FlipBoundingBox(TRBoundingBox box)
    {
        short min = box.MinX;
        short max = box.MaxX;
        box.MinX = (short)(max * -1);
        box.MaxX = (short)(min * -1);
    }

    private void MirrorEntities(TR1Level level)
    {
        foreach (TR1Entity entity in level.Entities)
        {
            entity.X = FlipWorldX(entity.X);
            AdjustTR1EntityPosition(entity);
        }

        AdjustDoors(level.Entities.FindAll(e => TR1TypeUtilities.IsDoorType(e.TypeID)));
    }

    private void MirrorEntities(TR2Level level)
    {
        foreach (TR2Entity entity in level.Entities)
        {
            entity.X = FlipWorldX(entity.X);
            AdjustTR2EntityPosition(entity);
        }

        AdjustDoors(level.Entities.FindAll(e => TR2TypeUtilities.IsDoorType(e.TypeID)));
    }

    private void MirrorEntities(TR3Level level)
    {
        foreach (TR3Entity entity in level.Entities)
        {
            entity.X = FlipWorldX(entity.X);
            AdjustTR3EntityPosition(entity);
        }

        AdjustDoors(level.Entities.FindAll(e => TR3TypeUtilities.IsDoorType(e.TypeID)));
    }

    private static void AdjustTR1EntityPosition(TR1Entity entity)
    {
        entity.Angle *= -1;

        switch (entity.TypeID)
        {
            case TR1Type.Animating1:
            case TR1Type.Animating2:
            case TR1Type.Animating3:
            case TR1Type.AtlanteanEgg:
                switch (entity.Angle)
                {
                    case _east:
                        entity.Z -= TRConsts.Step4;
                        break;
                    case _west:
                        entity.Z += TRConsts.Step4;
                        break;
                    case _north:
                        entity.X += TRConsts.Step4;
                        break;
                    case _south:
                        entity.X -= TRConsts.Step4;
                        break;
                }
                break;
            case TR1Type.AdamEgg:
                switch (entity.Angle)
                {
                    case _east:
                        entity.Z -= TRConsts.Step4 * 2;
                        break;
                    case _west:
                        entity.Z += TRConsts.Step4 * 2;
                        break;
                    case _north:
                        entity.X += TRConsts.Step4 * 2;
                        break;
                    case _south:
                        entity.X -= TRConsts.Step4 * 2;
                        break;
                }
                break;
            case TR1Type.BridgeTilt1:
            case TR1Type.BridgeTilt2:
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

    private static void AdjustTR2EntityPosition(TR2Entity entity)
    {
        // If it's facing +/-X direction, flip it
        if (entity.Angle == _east || entity.Angle == _west)
        {
            entity.Angle *= -1;
        }

        switch (entity.TypeID)
        {
            // These take up 2 tiles so need some fiddling
            case TR2Type.Elevator:
            case TR2Type.SpikyCeiling:
            case TR2Type.SpikyWall:
                switch (entity.Angle)
                {
                    case _south:
                        entity.X += TRConsts.Step4;
                        break;
                    case _west:
                        entity.Z -= TRConsts.Step4;
                        break;
                    case _north:
                        entity.X -= TRConsts.Step4;
                        break;
                    case _east:
                        entity.Z += TRConsts.Step4;
                        break;
                }
                break;
            case TR2Type.Gong: // case 0 applicable to IceCave
                switch (entity.Angle)
                {
                    case _south:
                        entity.X -= TRConsts.Step4;
                        break;
                    case _west:
                        entity.Z += TRConsts.Step4;
                        break;
                    case _north:
                        entity.X += TRConsts.Step4;
                        break;
                    case _east:
                        entity.Z -= TRConsts.Step4;
                        break;
                }
                break;

            case TR2Type.StatueWithKnifeBlade:
                if (entity.Angle == _east)
                {
                    entity.Angle = _west;
                    entity.X += TRConsts.Step4;
                }
                else if (entity.Angle == _west)
                {
                    entity.Angle = _east;
                    entity.X -= TRConsts.Step4;
                }
                break;

            // Bridge tilts need to be rotated
            case TR2Type.BridgeTilt1:
            case TR2Type.BridgeTilt2:
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

            case TR2Type.AirplanePropeller:
                if (entity.Angle == _west)
                {
                    entity.Angle = _east;
                }
                break;

            case TR2Type.OverheadPulleyHook:
                if (entity.Angle == _south || entity.Angle == _north)
                {
                    entity.Angle += _south;
                }
                break;

            case TR2Type.PowerSaw:
                if (entity.Angle == _north)
                {
                    entity.X += TRConsts.Step4;
                }
                break;

            case TR2Type.Helicopter:
                if (entity.Angle == _west)
                {
                    entity.Angle = _north;
                    entity.X += TRConsts.Step4;
                    entity.Z += TRConsts.Step4;
                }
                break;
        }
    }

    private static void AdjustTR3EntityPosition(TR3Entity entity)
    {
        // Flip the angle - north and south remain, everything else moves appropriately
        entity.Angle *= -1;

        switch (entity.TypeID)
        {
            // These take up several tiles so need some fiddling
            case TR3Type.SpikyVertWallOrTunnelBorer:
            case TR3Type.SpikyWall:
            case TR3Type.SubwayTrain:
                switch (entity.Angle)
                {
                    case _north:
                        entity.X -= TRConsts.Step4;
                        break;
                    case _east:
                        entity.Z += TRConsts.Step4;
                        break;
                    case _south:
                        entity.X += TRConsts.Step4;
                        break;
                    case _west:
                        entity.Z -= TRConsts.Step4;
                        break;
                }
                break;

            case TR3Type.Area51Swinger:
                switch (entity.Angle)
                {
                    case _north:
                        entity.X += TRConsts.Step4;
                        break;
                    case _west:
                        entity.Z += TRConsts.Step4;
                        break;
                }
                break;
            case TR3Type.BigMissile:
            case TR3Type.MovableBoom:
                switch (entity.Angle)
                {
                    case _east:
                        entity.Z -= TRConsts.Step4;
                        break;
                }
                break;

            // Bridge tilts need to be rotated
            case TR3Type.BridgeTilt1:
            case TR3Type.BridgeTilt2:
            case TR3Type.FireBreathingDragonStatue:
                switch (entity.Angle)
                {
                    case _north:
                        entity.Angle = _south;
                        break;
                    case _east:
                        entity.Angle = _west;
                        break;
                    case _south:
                        entity.Angle = _north;
                        break;
                    case _west:
                        entity.Angle = _east;
                        break;
                }
                break;

            // The Crash Site walls
            case TR3Type.DestroyableBoardedUpWall:
                switch (entity.Angle)
                {
                    case _east:
                        entity.Z -= 3 * TRConsts.Step4;
                        break;
                    case _south:
                        entity.X -= 3 * TRConsts.Step4;
                        break;
                }
                break;
        }
    }

    private static void AdjustDoors<T>(IEnumerable<TREntity<T>> allDoors)
        where T : Enum
    {
        // Double doors need to be swapped otherwise they open in the wrong direction.
        // Iterate backwards and try to find doors that are next to each other.
        // If found, swap their types.
        List<TREntity<T>> doors = new(allDoors);
        for (int i = doors.Count - 1; i >= 0; i--)
        {
            TREntity<T> door1 = doors[i];
            for (int j = doors.Count - 1; j >= 0; j--)
            {
                if (j == i)
                {
                    continue;
                }

                TREntity<T> door2 = doors[j];

                if (AreDoubleDoors(door1, door2))
                {
                    (door2.TypeID, door1.TypeID) = (door1.TypeID, door2.TypeID);

                    // Don't process these doors again, so just remove the first
                    doors.RemoveAt(i);
                    break;
                }
            }
        }
    }

    private static bool AreDoubleDoors<T>(TREntity<T> door1, TREntity<T> door2)
        where T : Enum
    {
        // If the difference between X or Z position is one sector size, they share the same Y val,
        // and they are facing the same direction, then they're double doors.
        if (EqualityComparer<T>.Default.Equals(door1.TypeID, door2.TypeID)
            || door1.Room != door2.Room
            || door1.Y != door2.Y
            || door1.Angle != door2.Angle)
        {
            return false;
        }

        // Be careful not to shift doors that are in front of each other, like at the end of Vilcabamba.
        return (door1.Angle == _north || door1.Angle == _south)
            ? Math.Abs(door1.X - door2.X) == TRConsts.Step4
            : Math.Abs(door1.Z - door2.Z) == TRConsts.Step4;
    }

    private void MirrorNullMeshes(TR1Level level)
    {
        // The deals with actual cameras as well as sinks
        foreach (TRCamera camera in level.Cameras)
        {
            camera.X = FlipWorldX(camera.X);
        }

        foreach (TRSoundSource<TR1SFX> sound in level.SoundSources)
        {
            sound.X = FlipWorldX(sound.X);
        }

        // TODO: Handle TRCinematicFrames by working out how to mirror animation
        // frames e.g. the LaraMiscAnim that corresponds with the cinematics.
        // Currently, frames are left untouched so the Rig starting animation
        // and dragon dagger cutscene behave normally. Lara is a bit out of
        // place in the HSH cinematics for now.
    }

    private void MirrorNullMeshes(TR2Level level)
    {
        // The deals with actual cameras as well as sinks
        foreach (TRCamera camera in level.Cameras)
        {
            camera.X = FlipWorldX(camera.X);
        }

        foreach (TRSoundSource<TR2SFX> sound in level.SoundSources)
        {
            sound.X = FlipWorldX(sound.X);
        }

        // TODO: Handle TRCinematicFrames by working out how to mirror animation
        // frames e.g. the LaraMiscAnim that corresponds with the cinematics.
        // Currently, frames are left untouched so the Rig starting animation
        // and dragon dagger cutscene behave normally. Lara is a bit out of
        // place in the HSH cinematics for now.
    }

    private void MirrorNullMeshes(TR3Level level)
    {
        // The deals with actual cameras as well as sinks
        foreach (TRCamera camera in level.Cameras)
        {
            camera.X = FlipWorldX(camera.X);
        }

        foreach (TRSoundSource<TR3SFX> sound in level.SoundSources)
        {
            sound.X = FlipWorldX(sound.X);
        }
    }

    private static void MirrorTextures(TR1Level level)
    {
        HashSet<ushort> textureReferences = new();
        HashSet<TRStaticMesh> processedMeshes = new();

        foreach (TR1Room room in level.Rooms)
        {
            MirrorRoomMeshTextures(room.Mesh, textureReferences);
            foreach (TR1RoomStaticMesh roomStaticMesh in room.StaticMeshes)
            {
                TRStaticMesh staticMesh = level.StaticMeshes[roomStaticMesh.ID];
                if (processedMeshes.Add(staticMesh))
                {
                    MirrorMeshTextures(staticMesh.Mesh, textureReferences);
                }
            }
        }

        // Include all animated texture references too
        foreach (TRAnimatedTexture anim in level.AnimatedTextures)
        {
            for (int i = 0; i < anim.Textures.Count; i++)
            {
                textureReferences.Add(anim.Textures[i]);
            }
        }

        MirrorObjectTextures(textureReferences, level.ObjectTextures);

        // Models such as doors may use textures also used on walls, but
        // these models aren't mirrored so the texture will end up being
        // upside down. Rotate the relevant mesh faces.
        MirrorDependentFaces(level.Models.Values, textureReferences);
    }

    private static void MirrorTextures(TR2Level level)
    {
        HashSet<ushort> textureReferences = new();
        HashSet<TRStaticMesh> processedMeshes = new();

        foreach (TR2Room room in level.Rooms)
        {
            MirrorRoomMeshTextures(room.Mesh, textureReferences);
            foreach (TR2RoomStaticMesh roomStaticMesh in room.StaticMeshes)
            {
                TRStaticMesh staticMesh = level.StaticMeshes[roomStaticMesh.ID];
                if (processedMeshes.Add(staticMesh))
                {
                    MirrorMeshTextures(staticMesh.Mesh, textureReferences);
                }
            }
        }

        // Include all animated texture references too
        foreach (TRAnimatedTexture anim in level.AnimatedTextures)
        {
            for (int i = 0; i < anim.Textures.Count; i++)
            {
                textureReferences.Add(anim.Textures[i]);
            }
        }

        MirrorObjectTextures(textureReferences, level.ObjectTextures);
    }

    private static void MirrorTextures(TR3Level level)
    {
        HashSet<ushort> textureReferences = new();
        HashSet<TRStaticMesh> processedMeshes = new();

        foreach (TR3Room room in level.Rooms)
        {
            MirrorRoomMeshTextures(room.Mesh, textureReferences);
            foreach (TR3RoomStaticMesh roomStaticMesh in room.StaticMeshes)
            {
                TRStaticMesh staticMesh = level.StaticMeshes[roomStaticMesh.ID];
                if (processedMeshes.Add(staticMesh))
                {
                    MirrorMeshTextures(staticMesh.Mesh, textureReferences);
                }
            }
        }

        foreach (TRAnimatedTexture anim in level.AnimatedTextures)
        {
            for (int i = 0; i < anim.Textures.Count; i++)
            {
                textureReferences.Add(anim.Textures[i]);
            }
        }

        MirrorObjectTextures(textureReferences, level.ObjectTextures);
    }

    private static void MirrorRoomMeshTextures<T, V>(TRRoomMesh<T, V> mesh, ISet<ushort> textureReferences)
        where T : Enum
        where V : TRRoomVertex
    {
        foreach (TRFace face in mesh.Rectangles)
        {
            face.SwapVertices(0, 3);
            face.SwapVertices(1, 2);
            textureReferences.Add(face.Texture);
        }

        foreach (TRFace face in mesh.Triangles)
        {
            face.SwapVertices(0, 2);
            textureReferences.Add(face.Texture);
        }
    }

    private static void MirrorMeshTextures(TRMesh mesh, ISet<ushort> textureReferences)
    {
        foreach (TRMeshFace face in mesh.TexturedRectangles)
        {
            face.SwapVertices(0, 3);
            face.SwapVertices(1, 2);
            textureReferences.Add(face.Texture);
        }

        foreach (TRMeshFace face in mesh.ColouredRectangles)
        {
            face.SwapVertices(0, 3);
            face.SwapVertices(1, 2);
        }

        foreach (TRMeshFace face in mesh.TexturedTriangles)
        {
            face.SwapVertices(0, 2);
            textureReferences.Add(face.Texture);
        }

        foreach (TRMeshFace face in mesh.ColouredTriangles)
        {
            face.SwapVertices(0, 2);
        }
    }

    private static void MirrorObjectTextures(ISet<ushort> textureReferences, List<TRObjectTexture> objectTextures)
    {
        foreach (ushort textureRef in textureReferences)
        {
            objectTextures[textureRef].FlipVertical();
        }
    }

    private static void MirrorDependentFaces(IEnumerable<TRModel> models, ISet<ushort> textureReferences)
    {
        IEnumerable<TRMeshFace> faces = models.SelectMany(m => m.Meshes)
            .SelectMany(m => m.TexturedRectangles)
            .Where(f => textureReferences.Contains(f.Texture))
            .Distinct();
        foreach (TRMeshFace face in faces)
        {
            face.SwapVertices(0, 2);
            face.SwapVertices(1, 3);
        }
    }
}
