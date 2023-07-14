using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelControl.Serialization
{
    public interface ISerializableCompact
    {
        byte[] Serialize();
    }
}
