using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelReader.Serialization;

namespace TRLevelReader.Model
{
    public class TRMesh : ISerializableCompact
    {
        //Held for convenience here but is not included in serialisation
        //Value matches that in containing pointer array
        public uint Pointer { get; set; }

        //6 Bytes
        public TRVertex Centre { get; set; }

        //4 Bytes
        public int CollRadius { get; set; }

        //2 Bytes
        public short NumVertices { get; set; }

        //NumVertices * 6 Bytes
        public TRVertex[] Vertices { get; set; }

        //2 bytes - if this is negative, lights is populated. Otherwise Normals populated
        public short NumNormals { get; set; }

        //NumNormals * 6 bytes OR
        public TRVertex[] Normals { get; set; }

        //NumNormals * 2 bytes - It's either Normals (+) or Lights (-) not both
        public short[] Lights { get; set; }

        //2 bytes
        public short NumTexturedRectangles { get; set; }

        //NumTexturedRectangles * 10 bytes
        public TRFace4[] TexturedRectangles { get; set; }

        //2 bytes
        public short NumTexturedTriangles { get; set; }

        //NumTexturedTriangles * 8 bytes
        public TRFace3[] TexturedTriangles { get; set; }

        //2 bytes
        public short NumColouredRectangles { get; set; }

        //NumColouredRectangles * 10 bytes
        public TRFace4[] ColouredRectangles { get; set; }

        //2 bytes
        public short NumColouredTriangles { get; set; }

        //NumColouredTriangles * 8 bytes
        public TRFace3[] ColouredTriangles { get; set; }

        //Some meshes appear to have trailing data
        internal byte[] TrailingData { get; set; }

        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(Centre.Serialize());
                    writer.Write(CollRadius);
                    writer.Write(NumVertices);
                    
                    foreach (TRVertex vert in Vertices)
                    {
                        writer.Write(vert.Serialize());
                    }

                    writer.Write(NumNormals);

                    if (NumNormals > 0)
                    {
                        foreach (TRVertex normal in Normals)
                        {
                            writer.Write(normal.Serialize());
                        }
                    }
                    else
                    {
                        foreach (ushort light in Lights)
                        {
                            writer.Write(light);
                        }
                    }

                    writer.Write(NumTexturedRectangles);
                    foreach (TRFace4 face in TexturedRectangles)
                    {
                        writer.Write(face.Serialize());
                    }

                    writer.Write(NumTexturedTriangles);
                    foreach (TRFace3 face in TexturedTriangles)
                    {
                        writer.Write(face.Serialize());
                    }

                    writer.Write(NumColouredRectangles);
                    foreach (TRFace4 face in ColouredRectangles)
                    {
                        writer.Write(face.Serialize());
                    }

                    writer.Write(NumColouredTriangles);
                    foreach (TRFace3 face in ColouredTriangles)
                    {
                        writer.Write(face.Serialize());
                    }

                    if (TrailingData != null)
                    {
                        writer.Write(TrailingData);
                    }
                }

                return stream.ToArray();
            }
        }
    }
}
