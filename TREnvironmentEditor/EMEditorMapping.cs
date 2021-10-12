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
        public EMEditorSet NonPurist { get; set; }
        public List<EMEditorSet> Any { get; set; }
        public List<List<EMEditorSet>> AllWithin { get; set; }
        public List<EMConditionalEditorSet> ConditionalAllWithin { get; set; }
        public List<EMEditorGroupedSet> OneOf { get; set; }
        public EMEditorSet Mirrored { get; set; }
        public Dictionary<ushort, ushort> AlternativeTextures { get; set; }

        public EMEditorMapping()
        {
            All = new EMEditorSet();
            NonPurist = new EMEditorSet();
            Any = new List<EMEditorSet>();
            AllWithin = new List<List<EMEditorSet>>();
            ConditionalAllWithin = new List<EMConditionalEditorSet>();
            OneOf = new List<EMEditorGroupedSet>();
            Mirrored = new EMEditorSet();
        }

        public static EMEditorMapping Get(string packPath)
        {
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
            if (NonPurist != null)
            {
                NonPurist.RemapTextures(AlternativeTextures);
            }
            if (Any != null)
            {
                Any.ForEach(s => s.RemapTextures(AlternativeTextures));
            }
            if (AllWithin != null)
            {
                AllWithin.ForEach(a => a.ForEach(s => s.RemapTextures(AlternativeTextures)));
            }
            if (ConditionalAllWithin != null)
            {
                foreach (EMConditionalEditorSet condSet in ConditionalAllWithin)
                {
                    condSet.OnTrue.ForEach(s => s.RemapTextures(AlternativeTextures));
                    condSet.OnFalse.ForEach(s => s.RemapTextures(AlternativeTextures));
                }
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