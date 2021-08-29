using System;
using System.Collections.Generic;

namespace TR2RandomizerCore.Globalisation
{
    public class Language : IComparable<Language>
    {
        public static readonly string DefaultTag = "EN";
        public static readonly string HybridTag = "HY";

        public string Name { get; set; }
        public string Tag { get; set; }

        public bool IsHybrid => Tag.ToUpper().Equals(HybridTag);

        public int CompareTo(Language other)
        {
            return IsHybrid ? 1 : Name.ToUpper().CompareTo(other.Name.ToUpper());
        }

        public override bool Equals(object obj)
        {
            return obj is Language language && Tag == language.Tag;
        }

        public override int GetHashCode()
        {
            return 1005349675 + EqualityComparer<string>.Default.GetHashCode(Tag);
        }
    }
}