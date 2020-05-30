using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TRLevelReader.Model;

namespace TRLevelReader
{
    internal static class TR2FileReadUtilities
    {
        public static TRRoomPortal ReadRoomPortal(BinaryReader reader)
        {
            return new TRRoomPortal
            {
                AdjoiningRoom = reader.ReadUInt16(),

                Normal = new TRVertex
                {
                    X = reader.ReadInt16(),
                    Y = reader.ReadInt16(),
                    Z = reader.ReadInt16()
                },

                Vertices = new TRVertex[]
                {
                    new TRVertex { X = reader.ReadInt16(), Y = reader.ReadInt16(), Z = reader.ReadInt16() },
                    new TRVertex { X = reader.ReadInt16(), Y = reader.ReadInt16(), Z = reader.ReadInt16() },
                    new TRVertex { X = reader.ReadInt16(), Y = reader.ReadInt16(), Z = reader.ReadInt16() },
                    new TRVertex { X = reader.ReadInt16(), Y = reader.ReadInt16(), Z = reader.ReadInt16() },
                }
            };
        }

        public static TRRoomSector ReadRoomSector(BinaryReader reader)
        {
            return new TRRoomSector
            {
                FDIndex = reader.ReadUInt16(),
                BoxIndex = reader.ReadUInt16(),
                RoomBelow = reader.ReadByte(),
                Floor = reader.ReadSByte(),
                RoomAbove = reader.ReadByte(),
                Ceiling = reader.ReadSByte()
            };
        }

        public static TR2RoomLight ReadRoomLight(BinaryReader reader)
        {
            return new TR2RoomLight
            {
                X = reader.ReadInt32(),
                Y = reader.ReadInt32(),
                Z = reader.ReadInt32(),
                Intensity1 = reader.ReadUInt16(),
                Intensity2 = reader.ReadUInt16(),
                Fade1 = reader.ReadUInt32(),
                Fade2 = reader.ReadUInt32()
            };
        }

        public static TR2RoomStaticMesh ReadRoomStaticMesh(BinaryReader reader)
        {
            return new TR2RoomStaticMesh
            {
                X = reader.ReadUInt32(),
                Y = reader.ReadUInt32(),
                Z = reader.ReadUInt32(),
                Rotation = reader.ReadUInt16(),
                Intensity1 = reader.ReadUInt16(),
                Intensity2 = reader.ReadUInt16(),
                MeshID = reader.ReadUInt16()
            };
        }

        public static TRAnimation ReadAnimation(BinaryReader reader)
        {
            return new TRAnimation
            {
                FrameOffset = reader.ReadUInt32(),
                FrameRate = reader.ReadByte(),
                FrameSize = reader.ReadByte(),
                StateID = reader.ReadUInt16(),
                Speed = new FixedFloat<short, ushort>
                {
                    Whole = reader.ReadInt16(),
                    Fraction = reader.ReadUInt16()
                },
                Accel = new FixedFloat<short, ushort>
                {
                    Whole = reader.ReadInt16(),
                    Fraction = reader.ReadUInt16()
                },
                FrameStart = reader.ReadUInt16(),
                FrameEnd = reader.ReadUInt16(),
                NextAnimation = reader.ReadUInt16(),
                NextFrame = reader.ReadUInt16(),
                NumStateChanges = reader.ReadUInt16(),
                StateChangeOffset = reader.ReadUInt16(),
                NumAnimCommands = reader.ReadUInt16(),
                AnimCommand = reader.ReadUInt16()
            };
        }

        public static TRStateChange ReadStateChange(BinaryReader reader)
        {
            return new TRStateChange()
            {
                StateID = reader.ReadUInt16(),
                NumAnimDispatches = reader.ReadUInt16(),
                AnimDispatch = reader.ReadUInt16()
            };
        }

        public static TRAnimDispatch ReadAnimDispatch(BinaryReader reader)
        {
            return new TRAnimDispatch()
            {
                Low = reader.ReadInt16(),
                High = reader.ReadInt16(),
                NextAnimation = reader.ReadInt16(),
                NextFrame = reader.ReadInt16()
            };
        }

        public static TRAnimCommand ReadAnimCommand(BinaryReader reader)
        {
            return new TRAnimCommand()
            {
                Value = reader.ReadInt16()
            };
        }

        public static TRMeshTreeNode ReadMeshTreeNode(BinaryReader reader)
        {
            return new TRMeshTreeNode()
            {
                Flags = reader.ReadUInt32(),
                OffsetX = reader.ReadInt32(),
                OffsetY = reader.ReadInt32(),
                OffsetZ = reader.ReadInt32()
            };
        }
    }
}
