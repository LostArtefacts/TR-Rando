using System.Collections.Generic;
using System.Linq;
using TRTexture16Importer.Helpers;

namespace TRTexture16Importer.Textures
{
    public class DynamicTextureSource : AbstractTextureSource
    {
        public Dictionary<string, HSBOperation> OperationMap { get; set; }
        public override string[] Variants => OperationMap.Keys.ToArray();
    }
}