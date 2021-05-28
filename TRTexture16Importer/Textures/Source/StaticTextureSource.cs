using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TRLevelReader.Model.Enums;

namespace TRTexture16Importer.Textures.Source
{
    public class StaticTextureSource : AbstractTextureSource, IDisposable
    {
        public string PNGPath { get; set; }
        public Dictionary<TR2Entities, Dictionary<Color, int>> EntityColourMap { get; set; }
        public Dictionary<TR2Entities, Dictionary<int, int>> EntityTextureMap { get; set; }
        public IEnumerable<TR2Entities> ColourEntities => EntityColourMap?.Keys;
        public IEnumerable<TR2Entities> TextureEntities => EntityTextureMap?.Keys;
        public Dictionary<string, List<Rectangle>> VariantMap { get; set; }
        public override string[] Variants => VariantMap.Keys.ToArray();
        public bool HasVariants => VariantMap.Count > 0;

        private Bitmap _bitmap;
        public Bitmap Bitmap
        {
            get
            {
                if (_bitmap == null)
                {
                    _bitmap = new Bitmap(PNGPath);
                }
                return _bitmap;
            }
        }

        public void Dispose()
        {
            if (_bitmap != null)
            {
                _bitmap.Dispose();
            }
        }

        public override bool Equals(object obj)
        {
            return obj is StaticTextureSource source && PNGPath == source.PNGPath;
        }

        public override int GetHashCode()
        {
            return -193438157 + EqualityComparer<string>.Default.GetHashCode(PNGPath);
        }
    }
}