using RectanglePacker.Organisation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Model;
using TRModelTransporter.Textures;

namespace TRModelTransporter.Transport
{
    public class TRModelBuilder
    {
        private static readonly Dictionary<TR2Entities, TR2Entities[]> _entityDependencies = new Dictionary<TR2Entities, TR2Entities[]>
        {
            [TR2Entities.MaskedGoon2] = new TR2Entities[] { TR2Entities.MaskedGoon1 },
            [TR2Entities.MaskedGoon3] = new TR2Entities[] { TR2Entities.MaskedGoon1 },
        };

        public TR2Level Level { get; set; }

        public TRModelDefinition CreateModelDefinition(TR2Entities modelEntity)
        {
            TRModelDefinition ed = new TRModelDefinition();
            ExportModel(ed, modelEntity);
            ExportMeshData(ed);
            ExportColours(ed);
            ExportTextures(ed);
            ExportAnimations(ed);
            ExportFrames(ed);
            ExportDependencies(ed);

            return ed;
        }

        private void ExportModel(TRModelDefinition definition, TR2Entities modelEntity)
        {
            definition.Model = Level.Models[Level.Models.ToList().FindIndex(e => e.ID == (short)modelEntity)];
        }

        private void ExportMeshData(TRModelDefinition definition)
        {
            definition.MeshTrees = TR2LevelUtilities.GetModelMeshTrees(Level, definition.Model);
            definition.Meshes = TR2LevelUtilities.GetModelMeshes(Level, definition.Model);
        }

        private void ExportColours(TRModelDefinition definition)
        {
            ISet<int> colourIndices = new SortedSet<int>();
            foreach (TRMesh mesh in definition.Meshes)
            {
                foreach (TRFace4 rect in mesh.ColouredRectangles)
                {
                    colourIndices.Add(BitConverter.GetBytes(rect.Texture)[1]);
                }
                foreach (TRFace3 tri in mesh.ColouredTriangles)
                {
                    colourIndices.Add(BitConverter.GetBytes(tri.Texture)[1]);
                }
            }

            definition.Colours = new Dictionary<int, TRColour4>(colourIndices.Count);
            foreach (int i in colourIndices)
            {
                definition.Colours[i] = Level.Palette16[i];
            }
        }

        private void ExportTextures(TRModelDefinition definition)
        {
            using (TexturedTilePacker levelPacker = new TexturedTilePacker(Level))
            {
                Dictionary<TexturedTile, List<TileSegment>> textureSegments = levelPacker.GetModelSegments(definition.Entity);
                definition.ObjectTextures = new Dictionary<int, List<IndexedTRObjectTexture>>();

                using (TexturedTilePacker segmentPacker = new TexturedTilePacker())
                {
                    int bitmapIndex = 0;
                    List<TileSegment> allSegments = new List<TileSegment>();
                    foreach (List<TileSegment> segments in textureSegments.Values)
                    {
                        allSegments.AddRange(segments);
                        for (int i = 0; i < segments.Count; i++)
                        {
                            TileSegment segment = segments[i];
                            definition.ObjectTextures[bitmapIndex++] = new List<IndexedTRObjectTexture>(segment.Textures.Cast<IndexedTRObjectTexture>().ToArray());
                        }
                    }

                    segmentPacker.AddRectangles(allSegments);

                    segmentPacker.FillMode = PackingFillMode.Horizontal;
                    segmentPacker.OrderMode = PackingOrderMode.Area;
                    segmentPacker.Order = PackingOrder.Descending;
                    segmentPacker.GroupMode = PackingGroupMode.Squares;
                    segmentPacker.TileWidth = 512;
                    segmentPacker.TileHeight = 512;
                    segmentPacker.MaximumTiles = 1;

                    segmentPacker.Pack();

                    TexturedTile tile = segmentPacker.Tiles[0];
                    List<Rectangle> rects = new List<Rectangle>();
                    foreach (TileSegment segment in allSegments)
                    {
                        rects.Add(segment.MappedBounds);
                    }
                    definition.ObjectTextureSegments = rects.ToArray();

                    Rectangle region = tile.GetOccupiedRegion(tile.BitmapGraphics.Graphics);
                    definition.Bitmap = tile.BitmapGraphics.Extract(region);
                }
            }
        }

        private void ExportAnimations(TRModelDefinition definition)
        {
            definition.Animations = PackAnimations(definition.Model);
        }

        private Dictionary<int, PackedAnimation> PackAnimations(TRModel model)
        {
            Dictionary<int, PackedAnimation> animations = new Dictionary<int, PackedAnimation>();

            int endAnimation = GetModelAnimationCount(Level, model) + model.Animation;
            for (int animationIndex = model.Animation; animationIndex < endAnimation; animationIndex++)
            {
                TRAnimation animation = Level.Animations[animationIndex];
                PackedAnimation packedAnimation = new PackedAnimation
                {
                    Animation = animation,
                };
                animations[animationIndex] = packedAnimation;

                PackStateChanges(packedAnimation, animation);
                PackAnimCommands(packedAnimation, animation);
                PackAnimSounds(packedAnimation);
            }

            return animations;
        }

        private void PackStateChanges(PackedAnimation packedAnimation, TRAnimation animation)
        {
            for (int stateChangeIndex = 0; stateChangeIndex < animation.NumStateChanges; stateChangeIndex++)
            {
                TRStateChange stateChange = Level.StateChanges[animation.StateChangeOffset + stateChangeIndex];
                packedAnimation.StateChanges.Add(stateChange);

                int dispatchOffset = stateChange.AnimDispatch;
                for (int i = 0; i < stateChange.NumAnimDispatches; i++, dispatchOffset++)
                {
                    if (!packedAnimation.AnimationDispatches.ContainsKey(dispatchOffset))
                    {
                        TRAnimDispatch dispatch = Level.AnimDispatches[dispatchOffset];
                        packedAnimation.AnimationDispatches[dispatchOffset] = dispatch;
                    }
                }
            }
        }

        private void PackAnimCommands(PackedAnimation packedAnimation, TRAnimation animation)
        {
            int cmdOffset = animation.AnimCommand;
            for (int i = 0; i < animation.NumAnimCommands; i++)
            {
                int cmdIndex = cmdOffset++;
                TRAnimCommand cmd = Level.AnimCommands[cmdIndex];

                int paramCount;
                switch ((TR2AnimCommand)cmd.Value)
                {
                    case TR2AnimCommand.SetPosition:
                        paramCount = 3;
                        break;
                    case TR2AnimCommand.JumpDistance:
                    case TR2AnimCommand.PlaySound:
                    case TR2AnimCommand.FlipEffect:
                        paramCount = 2;
                        break;
                    default:
                        paramCount = 0;
                        break;
                }

                short[] paramArr = new short[paramCount];
                for (int j = 0; j < paramCount; j++)
                {
                    paramArr[j] = Level.AnimCommands[cmdOffset++].Value;
                }

                packedAnimation.Commands[cmdIndex] = new PackedAnimationCommand
                {
                    Command = (TR2AnimCommand)cmd.Value,
                    Params = paramArr
                };
            }
        }

        private void PackAnimSounds(PackedAnimation packedAnimation)
        {
            foreach (PackedAnimationCommand cmd in packedAnimation.Commands.Values)
            {
                if (cmd.Command == TR2AnimCommand.PlaySound)
                {
                    int soundMapIndex = cmd.Params[1] & 0x3fff;
                    short soundDetailsIndex = Level.SoundMap[soundMapIndex];
                    if (soundDetailsIndex != -1)
                    {
                        packedAnimation.SoundMapIndices[soundMapIndex] = soundDetailsIndex;

                        TRSoundDetails soundDetails = Level.SoundDetails[soundDetailsIndex];
                        packedAnimation.SoundDetails[soundDetailsIndex] = soundDetails;

                        uint[] sampleIndices = new uint[soundDetails.NumSounds];
                        for (int i = 0; i < soundDetails.NumSounds; i++)
                        {
                            sampleIndices[i] = Level.SampleIndices[(ushort)(soundDetails.Sample + i)];
                        }

                        packedAnimation.SampleIndices[soundDetails.Sample] = sampleIndices;
                    }
                }
            }
        }

        private int GetModelAnimationCount(TR2Level level, TRModel model)
        {
            TRModel nextModel = model;
            int modelIndex = level.Models.ToList().IndexOf(model) + 1;
            while (modelIndex < level.NumModels)
            {
                nextModel = level.Models[modelIndex++];
                if (nextModel.Animation != ushort.MaxValue)
                {
                    break;
                }
            }

            ushort nextStartAnimation = nextModel.Animation;
            if (model == nextModel)
            {
                nextStartAnimation = (ushort)level.NumAnimations;
            }

            return model.Animation == ushort.MaxValue ? 0 : nextStartAnimation - model.Animation;
        }

        private void ExportFrames(TRModelDefinition definition)
        {
            int modelIndex = Level.Models.ToList().IndexOf(definition.Model);
            uint endFrame = 0;
            while (endFrame == 0 && modelIndex < Level.NumModels)
            {
                endFrame = Level.Models[++modelIndex].FrameOffset / 2;
            }

            List<ushort> frames = new List<ushort>();
            for (uint i = definition.Model.FrameOffset / 2; i < endFrame; i++)
            {
                frames.Add(Level.Frames[i]);
            }

            foreach (PackedAnimation anim in definition.Animations.Values)
            {
                anim.Animation.FrameOffset -= definition.Model.FrameOffset;
            }
            definition.Frames = frames.ToArray();
            //definition.Model.FrameOffset = 0;
        }

        private void ExportDependencies(TRModelDefinition definition)
        {
            List<TR2Entities> dependencies = new List<TR2Entities>();
            if (_entityDependencies.ContainsKey(definition.Entity))
            {
                dependencies.AddRange(_entityDependencies[definition.Entity]);
            }
            definition.Dependencies = dependencies.ToArray();
        }
    }
}