using System;
using System.Collections.Generic;
using System.Drawing;

namespace TRRandomizerCore.Textures
{
    public class WireframeData<E>
        where E : Enum
    {
        /// <summary>
        /// Models that can't use transparent wireframe textures. If included here, solid backgrounds will be used e.g. Door1 in Venice because of room 96
        /// </summary>
        public List<E> OpaqueModels { get; set; }

        /// <summary>
        /// Texture references that will have transparent backgrounds e.g. grates
        /// </summary>
        public List<ushort> TransparentTextures { get; set; }

        /// <summary>
        /// Textures that will be retained, such as water surfaces
        /// </summary>
        public List<ushort> ExcludedTextures { get; set; }

        /// <summary>
        /// The solid background colour to use for opaque frames
        /// </summary>
        public Color BackgroundColour { get; set; }

        /// <summary>
        /// The colour of the wire itself
        /// </summary>
        public Color HighlightColour { get; set; }

        /// <summary>
        /// Lara will become a solid version of the HighlightColour, otherwise she will be a frame
        /// </summary>
        public bool SolidLara { get; set; }

        public WireframeData()
        {
            BackgroundColour = Color.Black;
            HighlightColour = Color.White;
            SolidLara = false;
        }
    }
}