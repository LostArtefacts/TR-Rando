using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelReader.Model;

namespace TRLevelReader
{
    public static class TR2LevelReader
    {
        private static readonly uint TR2VersionHeader = 0x0000002D;

        public static TR2Level ReadLevel(string Filename)
        {
            if (!Filename.ToUpper().Contains("TR2"))
            {
                throw new NotImplementedException("File reader only supports TR2 levels");
            }

            TR2Level level = new TR2Level();
            int bytesRead = 0;

            using (BinaryReader reader = new BinaryReader(File.Open(Filename, FileMode.Open)))
            {
                Log.LogF("File opened");

                level.Version = reader.ReadUInt32();
                bytesRead += sizeof(uint);

                if (level.Version != TR2VersionHeader)
                {
                    throw new NotImplementedException("File reader only suppors TR2 levels");
                }
            }

            return level;
        }
    }
}
