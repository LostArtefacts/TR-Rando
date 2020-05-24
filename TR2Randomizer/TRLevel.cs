using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TR2Randomizer
{
    public class TRLevel
    {
        public TRItem GoldSecret { get; set; }

        public TRItem JadeSecret { get; set; }

        public TRItem StoneSecret { get; set; }

        public string Name { get; set; }

        public TRLevel()
        {
            GoldSecret = new TRItem
            {
                OID = (int)SecretType.Gold
            };

            JadeSecret = new TRItem
            {
                OID = (int)SecretType.Jade
            };

            StoneSecret = new TRItem
            {
                OID = (int)SecretType.Stone
            };
        }

        public bool DoesLevelHaveSecrets()
        {
            ProcessStartInfo trmodLaunch = new ProcessStartInfo
            {
                FileName = "trmod.exe",
                Arguments = this.Name + " LIST " + this.Name + ".trmlist",
                WindowStyle = ProcessWindowStyle.Hidden
            };

            var trmod = Process.Start(trmodLaunch);

            trmod.WaitForExit(2000);

            string FileOutput = File.ReadAllText(this.Name + ".trmlist");

            File.Delete(this.Name + ".trmlist");

            if (FileOutput.Contains("Item(190") && FileOutput.Contains("Item(191") && FileOutput.Contains("Item(192"))
            {
                return true;
            }

            return false;
        }
    }
}
