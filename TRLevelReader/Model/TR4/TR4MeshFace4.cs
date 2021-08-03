using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelReader.Serialization;

namespace TRLevelReader.Model
{
    public class TR4MeshFace4 : ISerializableCompact
    {
        public ushort[] Vertices { get; set; }

        public ushort Texture { get; set; }

        public ushort Effects { get; set; }

        public byte[] Serialize()
        {
            throw new NotImplementedException();
        }
    }
}
