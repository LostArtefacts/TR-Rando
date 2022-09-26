using System;
using System.Collections.Generic;
using System.Drawing;

namespace TRRandomizerCore.Textures
{
    public class WireframeData
    {
        /// <summary>
        /// Textures that will be retained, such as water surfaces
        /// </summary>
        public List<ushort> ExcludedTextures { get; set; }

        /// <summary>
        /// Textures that may share space with others that are being retained, but we want to still remove.
        /// e.g. Mist in Crash Site is shared with plane floor.
        /// </summary>
        public List<ushort> ForcedOverrides { get; set; }

        /// <summary>
        /// The colour of the wire itself
        /// </summary>
        public Color HighlightColour { get; set; }

        /// <summary>
        /// Lara will become a solid version of the HighlightColour, otherwise she will be a frame
        /// </summary>
        public bool SolidLara { get; set; }

        /// <summary>
        /// Enemies will become a solid version of the HighlightColour, otherwise they will be framed
        /// </summary>
        public bool SolidEnemies { get; set; }

        /// <summary>
        /// Models that should also use solid textures if SolidEnemies is enabled (e.g. CutsceneActors)
        /// </summary>
        public List<uint> SolidModels { get; set; }

        /// <summary>
        /// Allows different solid colours to be allocated per model.
        /// </summary>
        public Dictionary<uint, Color> ModelColours { get; set; }

        /// <summary>
        /// Where textures are shared within segments, and we want to exclude only parts, we "clip" out the rest.
        /// </summary>
        public List<WireframeClip> ManualClips { get; set; }

        /// <summary>
        /// Whether or not to generate special textures for ladders.
        /// </summary>
        public bool HighlightLadders { get; set; }

        /// <summary>
        /// Whether or not 3D pickups are in use, similar to TR3.
        /// </summary>
        public bool Has3DPickups { get; set; }

        public WireframeData()
        {
            ExcludedTextures = new List<ushort>();
            ForcedOverrides = new List<ushort>();
            HighlightColour = Color.White;
            SolidLara = false;
            SolidEnemies = false;
            SolidModels = new List<uint>();
            ModelColours = new Dictionary<uint, Color>();
            ManualClips = new List<WireframeClip>();
            HighlightLadders = false;
            Has3DPickups = false;
        }
    }
}