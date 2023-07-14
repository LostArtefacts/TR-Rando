﻿using System.Drawing;
using TRLevelControl.Model;

namespace TRModelTransporter.Model.Textures
{
    public class IndexedTRSpriteTexture : AbstractIndexedTRTexture
    {
        private TRSpriteTexture _texture;

        public override int Atlas
        {
            get => _texture.Atlas;
            set => _texture.Atlas = (ushort)value;
        }

        public TRSpriteTexture Texture
        {
            get => _texture;
            set
            {
                _texture = value;
                GetBoundsFromTexture();
            }
        }

        protected override void GetBoundsFromTexture()
        {
            _bounds = new Rectangle(Texture.X, Texture.Y, (Texture.Width + 1) / 256, (Texture.Height + 1) / 256);
        }

        protected override void ApplyBoundDiffToTexture(int xDiff, int yDiff)
        {
            Texture.X += (byte)xDiff;
            Texture.Y += (byte)yDiff;
        }

        public override AbstractIndexedTRTexture Clone()
        {
            return new IndexedTRSpriteTexture
            {
                Index = Index,
                Classification = Classification,
                Texture = new TRSpriteTexture
                {
                    Atlas = Texture.Atlas,
                    BottomSide = Texture.BottomSide,
                    Height = Texture.Height,
                    Width = Texture.Width,
                    LeftSide = Texture.LeftSide,
                    RightSide = Texture.RightSide,
                    TopSide = Texture.TopSide,
                    X = Texture.X,
                    Y = Texture.Y
                }
            };
        }
    }
}