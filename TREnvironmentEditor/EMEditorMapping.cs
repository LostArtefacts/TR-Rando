using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using TREnvironmentEditor.Model;
using TREnvironmentEditor.Parsing;

namespace TREnvironmentEditor
{
    public class EMEditorMapping
    {
        private static readonly EMConverter _converter = new EMConverter();

        public EMEditorSet All { get; set; }
        public List<EMEditorSet> Any { get; set; }
        public List<List<EMEditorSet>> AllWithin { get; set; }
        public List<EMEditorGroupedSet> OneOf { get; set; }
        public EMEditorSet Mirrored { get; set; }
        public Dictionary<ushort, ushort> AlternativeTextures { get; set; }

        public EMEditorMapping()
        {
            All = new EMEditorSet();
            Any = new List<EMEditorSet>();
            OneOf = new List<EMEditorGroupedSet>();
            Mirrored = new EMEditorSet();
        }

        public static EMEditorMapping Get(string lvlName)
        {
            string packPath = string.Format(@"Resources\Environment\{0}-Environment.json", lvlName);
            if (File.Exists(packPath))
            {
                return JsonConvert.DeserializeObject<EMEditorMapping>(File.ReadAllText(packPath), _converter);
            }

            return null;
        }

        public void AlternateTextures()
        {
            if (AlternativeTextures == null)
            {
                return;
            }

            if (All != null)
            {
                All.RemapTextures(AlternativeTextures);
            }
            if (Any != null)
            {
                Any.ForEach(s => s.RemapTextures(AlternativeTextures));
            }
            if (AllWithin != null)
            {
                AllWithin.ForEach(a => a.ForEach(s => s.RemapTextures(AlternativeTextures)));
            }
            if (OneOf != null)
            {
                foreach (EMEditorGroupedSet group in OneOf)
                {
                    group.Leader.RemapTextures(AlternativeTextures);
                    group.Followers.ForEach(s => s.RemapTextures(AlternativeTextures));
                }
            }
            if (Mirrored != null)
            {
                Mirrored.RemapTextures(AlternativeTextures);
            }
        }
    }
}