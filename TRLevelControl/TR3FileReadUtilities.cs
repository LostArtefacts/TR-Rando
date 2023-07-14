using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TRLevelControl.Model;

namespace TRLevelControl
{
    internal static class TR3FileReadUtilities
    {
        public static TR3RoomLight ReadRoomLight(BinaryReader reader)
        {
            return new TR3RoomLight
            {
                X = reader.ReadInt32(),
                Y = reader.ReadInt32(),
                Z = reader.ReadInt32(),
                Colour = new TRColour
                {
                    Red = reader.ReadByte(),
                    Green = reader.ReadByte(),
                    Blue = reader.ReadByte()
                },
                LightType = reader.ReadByte(),
                LightProperties = new short[4]
                {
                    reader.ReadInt16(),
                    reader.ReadInt16(),
                    reader.ReadInt16(),
                    reader.ReadInt16()
                }
            };
        }

        public static TR3RoomStaticMesh ReadRoomStaticMesh(BinaryReader reader)
        {
            return new TR3RoomStaticMesh
            {
                X = reader.ReadUInt32(),
                Y = reader.ReadUInt32(),
                Z = reader.ReadUInt32(),
                Rotation = reader.ReadUInt16(),
                Colour = reader.ReadUInt16(),
                Unused = reader.ReadUInt16(),
                MeshID = reader.ReadUInt16()
            };
        }

        public static TR3SoundDetails ReadSoundDetails(BinaryReader reader)
        {
            return new TR3SoundDetails()
            {
                Sample = reader.ReadUInt16(),
                Volume = reader.ReadByte(),
                Range = reader.ReadByte(),
                Chance = reader.ReadByte(),
                Pitch = reader.ReadByte(),
                Characteristics = reader.ReadInt16()
            };
        }
    }
}
