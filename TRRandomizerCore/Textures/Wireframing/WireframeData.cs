using System;
using System.Collections.Generic;
using System.Drawing;

namespace TRRandomizerCore.Textures
{
    public class WireframeData
    {
        /// <summary>
        /// Textures that will be retained, such as water surfaces.
        /// </summary>
        public List<ushort> ExcludedTextures { get; set; }

        /// <summary>
        /// Textures that require specific processing.
        /// </summary>
        public List<SpecialTextureHandling> SpecialTextures { get; set; }

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
        /// The colour of the wire used to indicate triggers
        /// </summary>
        public Color TriggerColour { get; set; }

        /// <summary>
        /// The colour of the wire used to indicate death tiles
        /// </summary>
        public Color DeathColour { get; set; }

        /// <summary>
        /// Lara will become a solid version of the HighlightColour, otherwise she will be a frame
        /// </summary>
        public bool SolidLara { get; set; }

        /// <summary>
        /// Enemies will become a solid version of the HighlightColour, otherwise they will be framed
        /// </summary>
        public bool SolidEnemies { get; set; }
        
        /// <summary>
        /// Keyholes, switches etc will become a solid version of the HighlightColour, otherwise they will be framed
        /// </summary>
        public bool SolidInteractables { get; set; }

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
        /// Whether or not to generate special textures for heavy triggers and pads.
        /// </summary>
        public bool HighlightTriggers { get; set; }

        /// <summary>
        /// Whether or not to include death tiles in trigger highlights.
        /// </summary>
        public bool HighlightDeathTiles { get; set; }

        /// <summary>
        /// Whether or not 3D pickups are in use, similar to TR3.
        /// </summary>
        public bool Has3DPickups { get; set; }

        public WireframeData()
        {
            ExcludedTextures = new List<ushort>();
            SpecialTextures = new List<SpecialTextureHandling>();
            ForcedOverrides = new List<ushort>();
            HighlightColour = Color.White;
            TriggerColour = Color.White;
            DeathColour = Color.White;
            SolidLara = false;
            SolidEnemies = false;
            SolidModels = new List<uint>();
            ModelColours = new Dictionary<uint, Color>();
            ManualClips = new List<WireframeClip>();
            HighlightLadders = false;
            Has3DPickups = false;
        }

        public static List<SpecialTextureMode> GetDrawModes(SpecialTextureType type)
        {
            List<SpecialTextureMode> modes = new List<SpecialTextureMode>();
            switch (type)
            {
                case SpecialTextureType.MidasDoors:
                    modes.Add(SpecialTextureMode.MidasDoorBars);
                    modes.Add(SpecialTextureMode.MidasDoorLines);
                    modes.Add(SpecialTextureMode.MidasDoorFill);
                    modes.Add(SpecialTextureMode.MidasDoorDiagonals);
                    break;
            }
            return modes;
        }
    }

    public enum SpecialTextureType
    {
        MidasDoors
    }

    public enum SpecialTextureMode
    {
        MidasDoorBars,
        MidasDoorLines,
        MidasDoorFill,
        MidasDoorDiagonals
    }

    public class SpecialTextureHandling
    {
        public SpecialTextureType Type { get; set; }
        public SpecialTextureMode Mode { get; set; }
        public List<ushort> Textures { get; set; }
    }
}