using System;
using System.Collections.Generic;

namespace TR2RandomizerView.Updates
{
    public class Update
    {
        public string CurrentVersion { get; set; }
        public string NewVersion { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string UpdateBody { get; set; }
        public string UpdateURL { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Update update &&
                   NewVersion == update.NewVersion;
        }

        public override int GetHashCode()
        {
            return 1055771379 + EqualityComparer<string>.Default.GetHashCode(NewVersion);
        }
    }
}