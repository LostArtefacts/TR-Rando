﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelReader.Serialization
{
    public interface ISerializableCompact
    {
        byte[] Serialize();
    }
}
