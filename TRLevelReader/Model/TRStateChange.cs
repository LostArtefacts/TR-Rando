using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelReader.Serialization;

namespace TRLevelReader.Model
{
    public class TRStateChange : ISerializableCompact
    {
        public ushort StateID { get; set; }

        public ushort NumAnimDispatches { get; set; }

        public ushort AnimDispatch { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());

            sb.Append(" StateID: " + StateID);
            sb.Append(" NumAnimDispatches: " + NumAnimDispatches);
            sb.Append(" AnimDispatch: " + AnimDispatch);

            return sb.ToString();
        }

        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(StateID);
                    writer.Write(NumAnimDispatches);
                    writer.Write(AnimDispatch);
                }

                return stream.ToArray();
            }
        }
    }
}
