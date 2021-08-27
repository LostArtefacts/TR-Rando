using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TRLevelReader.Serialization;

namespace TRLevelReader.Model
{
    public class TR5Room : ISerializableCompact
    {
        public byte[] XELALandmark { get; set; }

        public uint RoomDataSize { get; set; }

        public uint Seperator { get; set; }

        public uint EndSDOffset { get; set; }

        public uint StartSDOffset { get; set; }

        public uint Seperator2 { get; set; }

        public uint EndPortalOffset { get; set; }

        public TR5RoomInfo Info { get; set; }

        public ushort NumZSectors { get; set; }

        public ushort NumXSectors { get; set; }

        public uint RoomColourARGB { get; set; }

        public ushort NumLights { get; set; }

        public ushort NumStaticMeshes { get; set; }

        public byte Reverb { get; set; }

        public byte AlternateGroup { get; set; }

        public ushort WaterScheme { get; set; }

        public uint[] Filler { get; set; }

        public uint[] Seperator3 { get; set; }

        public uint Filler2 { get; set; }

        public ushort AlternateRoom { get; set; }

        public ushort Flags { get; set; }

        public uint Unknown1 { get; set; }

        public uint Unknown2 { get; set; }

        public uint Unknown3 { get; set; }

        public uint Seperator4 { get; set; }

        public ushort Unknown4 { get; set; }

        public ushort Unknown5 { get; set; }

        public float RoomX { get; set; }

        public float RoomY { get; set; }

        public float RoomZ { get; set; }

        public uint[] Seperator5 { get; set; }

        public uint Seperator6 { get; set; }

        public uint Seperator7 { get; set; }

        public uint NumRoomTriangles { get; set; }

        public uint NumRoomRectangles { get; set; }

        public uint RoomLightsPtr { get; set; }

        public uint RoomFogBulbsPtr { get; set; }

        public uint NumLights2 { get; set; }

        public uint NumFogBulbs { get; set; }

        public float RoomYTop { get; set; }

        public float RoomYBottom { get; set; }

        public uint NumLayers { get; set; }

        public uint LayersPtr { get; set; }

        public uint VerticesDataSize { get; set; }

        public uint PolyOffset { get; set; }

        public uint PolyOffset2 { get; set; }

        public uint NumVertices { get; set; }

        public uint[] Seperator8 { get; set; }

        //Not explicitly part of TR5Room in TRosettaStone - but since it immediately follows a room block
        //it may as well belong to a room. Will be easier to understand if it belongs to the room.
        public TR5RoomData RoomData { get; set; }

        public bool IsNullRoom
        {
            get
            {
                return (Seperator6 == 0xCDCDCDCD);
            }
        }

        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(XELALandmark);
                    writer.Write(RoomDataSize);
                    writer.Write(Seperator);
                    writer.Write(EndSDOffset);
                    writer.Write(StartSDOffset);
                    writer.Write(Seperator2);
                    writer.Write(EndPortalOffset);
                    writer.Write(Info.Serialize());
                    writer.Write(NumZSectors);
                    writer.Write(NumXSectors);
                    writer.Write(RoomColourARGB);
                    writer.Write(NumLights);
                    writer.Write(NumStaticMeshes);
                    writer.Write(Reverb);
                    writer.Write(AlternateGroup);
                    writer.Write(WaterScheme);
                    foreach (uint data in Filler) { writer.Write(data); }
                    foreach (uint data in Seperator3) { writer.Write(data); }
                    writer.Write(Filler2);
                    writer.Write(AlternateRoom);
                    writer.Write(Flags);
                    writer.Write(Unknown1);
                    writer.Write(Unknown2);
                    writer.Write(Unknown3);
                    writer.Write(Seperator4);
                    writer.Write(Unknown4);
                    writer.Write(Unknown5);
                    writer.Write(RoomX);
                    writer.Write(RoomY);
                    writer.Write(RoomZ);
                    foreach (uint data in Seperator5) { writer.Write(data); }
                    writer.Write(Seperator6);
                    writer.Write(Seperator7);
                    writer.Write(NumRoomTriangles);
                    writer.Write(NumRoomRectangles);
                    writer.Write(RoomLightsPtr);
                    writer.Write(RoomFogBulbsPtr);
                    writer.Write(NumLights2);
                    writer.Write(NumFogBulbs);
                    writer.Write(RoomYTop);
                    writer.Write(RoomYBottom);
                    writer.Write(NumLayers);
                    writer.Write(LayersPtr);
                    writer.Write(VerticesDataSize);
                    writer.Write(PolyOffset);
                    writer.Write(PolyOffset2);
                    writer.Write(NumVertices);
                    foreach (uint data in Seperator8) { writer.Write(data); }
                    writer.Write(RoomData.SerializeRaw());
                }

                return stream.ToArray();
            }
        }
    }
}
