using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelControl.Model;

namespace TRLevelControl
{
    public class TR1LevelWriter
    {
        public TR1LevelWriter()
        {

        }

        public void WriteLevelToFile(TR1Level lvl, string filepath)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Create(filepath)))
            {
                writer.Write(lvl.Serialize());
            }
        }
    }
}
