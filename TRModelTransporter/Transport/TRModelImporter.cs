using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Helpers;
using TRModelTransporter.Model;
using TRModelTransporter.Textures;
using TRTexture16Importer.Helpers;

namespace TRModelTransporter.Transport
{
    public class TRModelImporter
    {
        private static readonly string _defaultDataFolder = @"Resources\Models";
        private static readonly string _dataFileName = "Data.json";
        private static readonly string _imageFileName = "Segments.png";

        public string DataFolder { get; set; }
        public TR2Level Level { get; set; }

        public TRModelImporter()
        {
            DataFolder = _defaultDataFolder;
        }

        private TRModelDefinition GetDefinition(TR2Entities modelEntity)
        {
            string directory = Path.Combine(DataFolder, modelEntity.ToString());
            string dataFilePath = Path.Combine(directory, _dataFileName);
            string imageFilePath = Path.Combine(directory, _imageFileName);

            if (!File.Exists(dataFilePath))
            {
                throw new IOException(string.Format("Missing model data JSON file ({0})", dataFilePath));
            }

            if (!File.Exists(imageFilePath))
            {
                throw new IOException(string.Format("Missing model data image file ({0})", imageFilePath));
            }

            TRModelDefinition definition = JsonConvert.DeserializeObject<TRModelDefinition>(File.ReadAllText(dataFilePath));
            definition.Bitmap = new Bitmap(imageFilePath);
            return definition;
        }

        public void Import(TR2Entities modelEntity, IEnumerable<TR2Entities> entityTexturesToRemove = null)
        {
            Import(new TR2Entities[] { modelEntity }, entityTexturesToRemove);
        }

        public void Import(IEnumerable<TR2Entities> modelEntities, IEnumerable<TR2Entities> entityTexturesToRemove = null)
        {
            ISet<TR2Entities> uniqueEntities = new HashSet<TR2Entities>(modelEntities);
            List<TRModelDefinition> modelDefinitions = new List<TRModelDefinition>();
            foreach (TR2Entities entity in uniqueEntities)
            {
                bool isDependency = false;
                foreach (TRModelDefinition definition in modelDefinitions)
                {
                    if (definition.Dependencies.Contains(entity))
                    {
                        isDependency = true;
                        break;
                    }
                }

                if (!isDependency)
                {
                    modelDefinitions.Add(GetDefinition(entity));
                }
            }

            Import(modelDefinitions, entityTexturesToRemove);
        }

        private void Import(IEnumerable<TRModelDefinition> definitions, IEnumerable<TR2Entities> entityTexturesToRemove = null)
        {
            // Textures first, which will remap Mesh rectangles/triangles to the new texture indices.
            // This is called using the entire entity list to import so that RectanglePacker packer has
            // the best chance to organise the tiles.
            ImportTextures(definitions, entityTexturesToRemove);

            foreach (TRModelDefinition definition in definitions)
            {
                // Colours next, again to remap Mesh rectangles/triangles to any new palette indices
                ImportColours(definition);

                // Meshes and trees should now be remapped, so import into the level
                ImportMeshData(definition);

                // Animations, AnimCommands, AnimDispatches, Sounds, StateChanges
                ImportAnimations(definition);

                // Frames are imported in bulk rather than per animation
                ImportFrames(definition);

                // Add the model, which will have the correct StartingMesh, MeshTree, Frame and Animation offset.
                ImportModel(definition);

                // Check for any dependencies (prime example is MaskedGoon2 and MaskedGoon3 - the game crashes if MaskedGoon1 is not present
                ImportDependencies(definition);
            }
        }

        private void ImportTextures(IEnumerable<TRModelDefinition> definitions, IEnumerable<TR2Entities> entityTexturesToRemove = null)
        {
            // Rebuild the segment list. We assume the list of IndexedTRObjectTextures has been
            // ordered by area descending to preserve the "master" texture for each segment.
            Dictionary<TRModelDefinition, List<TileSegment>> segments = new Dictionary<TRModelDefinition, List<TileSegment>>();
            foreach (TRModelDefinition definition in definitions)
            {
                segments[definition] = new List<TileSegment>();
                using (BitmapGraphics bg = new BitmapGraphics(definition.Bitmap))
                {
                    foreach (int segmentIndex in definition.ObjectTextures.Keys)
                    {
                        Bitmap segmentClip = bg.Extract(definition.ObjectTextureSegments[segmentIndex]);
                        TileSegment segment = null;
                        foreach (IndexedTRObjectTexture texture in definition.ObjectTextures[segmentIndex])
                        {
                            if (segment == null)
                            {
                                segments[definition].Add(segment = new TileSegment(texture, segmentClip));
                            }
                            else
                            {
                                segment.AddTexture(texture);
                            }
                        }
                    }
                }
            }

            // Pack the segments into the level, if possible.
            using (TexturedTilePacker packer = new TexturedTilePacker(Level))
            {
                if (entityTexturesToRemove != null)
                {
                    packer.RemoveModelSegments(entityTexturesToRemove);
                }

                List<TileSegment> allSegments = new List<TileSegment>();
                foreach (List<TileSegment> segmentList in segments.Values)
                {
                    allSegments.AddRange(segmentList);
                }

                packer.AddRectangles(allSegments);
                packer.Pack(true);
                if (packer.OrphanedRectangles.Count > 0)
                {
                    List<string> entityNames = new List<string>();
                    foreach (TRModelDefinition def in definitions)
                    {
                        entityNames.Add(def.Entity.ToString());
                    }
                    throw new PackingException
                    (
                        string.Format("Failed to pack {0} rectangles for model types {1}.", packer.OrphanedRectangles, string.Join(", ", entityNames))
                    );
                }
            }

            // Add each ObjectTexture to the level and store a map of old index to new index.
            List<TRObjectTexture> levelTextures = Level.ObjectTextures.ToList();
            Dictionary<TRModelDefinition, Dictionary<int, int>> indexMap = new Dictionary<TRModelDefinition, Dictionary<int, int>>();
            foreach (TRModelDefinition definition in definitions)
            {
                indexMap[definition] = new Dictionary<int, int>();
                foreach (TileSegment segment in segments[definition])
                {
                    foreach (IndexedTRObjectTexture texture in segment.Textures)
                    {
                        levelTextures.Add(texture.Texture);
                        indexMap[definition][texture.Index] = levelTextures.Count - 1;
                    }
                }
            }

            // Save the new textures in the level
            Level.ObjectTextures = levelTextures.ToArray();
            Level.NumObjectTextures = (uint)levelTextures.Count;

            // Change the definition's meshes so that the textured rectangles and triangles point
            // to the correct object texture.
            foreach (TRModelDefinition definition in indexMap.Keys)
            {
                foreach (TRMesh mesh in definition.Meshes)
                {
                    foreach (TRFace4 rect in mesh.TexturedRectangles)
                    {
                        rect.Texture = (ushort)indexMap[definition][rect.Texture];
                    }
                    foreach (TRFace3 tri in mesh.TexturedTriangles)
                    {
                        tri.Texture = (ushort)indexMap[definition][tri.Texture];
                    }
                }
            }
        }

        private void ImportColours(TRModelDefinition definition)
        {
            Dictionary<int, int> indexMap = new Dictionary<int, int>();
            int newColourCount = 0;
            foreach (int paletteIndex in definition.Colours.Keys)
            {
                TRColour4 newColour = definition.Colours[paletteIndex];
                int existingIndex = Level.Palette16.ToList().FindIndex
                (
                    e => e.Red == newColour.Red && e.Green == newColour.Green && e.Blue == newColour.Blue && e.Unused == newColour.Unused
                );

                if (existingIndex != -1)
                {
                    indexMap[paletteIndex] = existingIndex;
                }
                else
                {
                    // TextureLevelMapping.RedrawStaticTargets makes use of (index - 1)
                    // so we'll go from (index - 2). I don't like this approach -
                    // needs improving!
                    int newColourIndex = Level.Palette16.Length - 2 - newColourCount;
                    Level.Palette16[newColourIndex] = newColour;
                    indexMap[paletteIndex] = newColourIndex;
                    newColourCount++;
                }
            }

            foreach (TRMesh mesh in definition.Meshes)
            {
                foreach (TRFace4 rect in mesh.ColouredRectangles)
                {
                    rect.Texture = ReindexTexture(rect.Texture, indexMap);
                }
                foreach (TRFace3 tri in mesh.ColouredTriangles)
                {
                    tri.Texture = ReindexTexture(tri.Texture, indexMap);
                }
            }
        }

        private ushort ReindexTexture(ushort value, Dictionary<int, int> indexMap)
        {
            byte[] arr = BitConverter.GetBytes(value);
            int highByte = Convert.ToInt32(arr[1]);
            if (indexMap.ContainsKey(highByte))
            {
                arr[1] = (byte)indexMap[highByte];
                return BitConverter.ToUInt16(arr, 0);
            }
            return value;
        }

        private void ImportMeshData(TRModelDefinition definition)
        {
            // Copy the MeshTreeNodes and Meshes into the level, making a note of the first
            // inserted index for each - this is used to update the Model to point to the
            // correct starting positions.
            for (int i = 0; i < definition.MeshTrees.Length; i++)
            {
                TRMeshTreeNode tree = definition.MeshTrees[i];
                int insertedIndex = TR2LevelUtilities.InsertMeshTreeNode(Level, tree);
                if (i == 0)
                {
                    definition.Model.MeshTree = 4 * (uint)insertedIndex;
                }
            }

            for (int i = 0; i < definition.Meshes.Length; i++)
            {
                TRMesh mesh = definition.Meshes[i];
                int insertedIndex = TR2LevelUtilities.InsertMesh(Level, mesh);
                if (i == 0)
                {
                    definition.Model.StartingMesh = (ushort)insertedIndex;
                }
            }
        }

        private void ImportAnimations(TRModelDefinition definition)
        {
            Dictionary<int, PackedAnimation> animations = definition.Animations;
            bool firstAnimationConfigured = false;
            Dictionary<int, int> indexMap = new Dictionary<int, int>();
            foreach (int oldAnimationIndex in animations.Keys)
            {
                PackedAnimation packedAnimation = animations[oldAnimationIndex];
                UnpackStateChanges(packedAnimation);
                UnpackAnimSounds(packedAnimation);
                UnpackAnimCommands(packedAnimation);

                int newAnimationIndex = UnpackAnimation(packedAnimation);
                indexMap[oldAnimationIndex] = newAnimationIndex;

                if (!firstAnimationConfigured)
                {
                    definition.Model.Animation = (ushort)newAnimationIndex;
                    firstAnimationConfigured = true;
                }
            }

            // Re-map the NextAnimations of each of the animation and dispatches
            // now we know the indices of each of the newly inserted animations.
            foreach (PackedAnimation packedAnimation in animations.Values)
            {
                packedAnimation.Animation.NextAnimation = (ushort)indexMap[packedAnimation.Animation.NextAnimation];
                foreach (TRAnimDispatch dispatch in packedAnimation.AnimationDispatches.Values)
                {
                    dispatch.NextAnimation = (short)indexMap[dispatch.NextAnimation];
                }
            }

            // Inserting SampleIndices will break the game unless they are sorted numerically
            // so handle this outwith the main animation insertion loop for ease.
            ResortSoundIndices();
        }

        private void UnpackStateChanges(PackedAnimation packedAnimation)
        {
            if (packedAnimation.Animation.NumStateChanges == 0)
            {
                if (packedAnimation.AnimationDispatches.Count != 0)
                {
                    throw new Exception();
                }
                return;
            }

            // Import the AnimDispatches first, noting their new indices
            List<TRAnimDispatch> animDispatches = Level.AnimDispatches.ToList();
            Dictionary<int, int> indexMap = new Dictionary<int, int>();
            foreach (int oldDispatchIndex in packedAnimation.AnimationDispatches.Keys)
            {
                TRAnimDispatch dispatch = packedAnimation.AnimationDispatches[oldDispatchIndex];
                indexMap[oldDispatchIndex] = animDispatches.Count;
                animDispatches.Add(dispatch);
                // The dispatch's NextAnimation will need to be remapped, but this is handled in ImportAnimations
            }

            // The animation's StateChangeOffset will be the current length of level StateChanges
            List<TRStateChange> stateChanges = Level.StateChanges.ToList();
            packedAnimation.Animation.StateChangeOffset = (ushort)stateChanges.Count;

            // Import Each state change, re-mapping AnimDispatch to new index
            foreach (TRStateChange stateChange in packedAnimation.StateChanges)
            {
                stateChange.AnimDispatch = (ushort)indexMap[stateChange.AnimDispatch];
                stateChanges.Add(stateChange);
            }

            // Save back to the level
            Level.AnimDispatches = animDispatches.ToArray();
            Level.NumAnimDispatches = (uint)animDispatches.Count;

            Level.StateChanges = stateChanges.ToArray();
            Level.NumStateChanges = (uint)stateChanges.Count;
        }

        /**
         * SampleIndices has to remain in numerical order, but rather than dealing with it after inserting
         * each new sample, ImportAnimations will handle reorganising the list and remapping SoundDetails
         * as necessary.
         */
        private void UnpackAnimSounds(PackedAnimation packedAnimation)
        {
            // First, insert each required SampleIndex value, which are the values
            // that point into MAIN.SFX. Note the first inserted index for updating
            // the relevant SoundDetails.Sample.
            List<uint> sampleIndices = Level.SampleIndices.ToList();
            Dictionary<uint, ushort> sampleMap = new Dictionary<uint, ushort>();

            foreach (ushort sampleIndex in packedAnimation.SampleIndices.Keys)
            {
                uint[] sampleValues = packedAnimation.SampleIndices[sampleIndex];
                if (sampleValues.Length > 0)
                {
                    // Store each value as long as it does not already exist
                    for (int i = 0; i < sampleValues.Length; i++)
                    {
                        if (!sampleIndices.Contains(sampleValues[i]))
                        {
                            sampleIndices.Add(sampleValues[i]);
                        }
                    }

                    // Store the "new" index of the first sample
                    sampleMap[sampleIndex] = (ushort)sampleIndices.IndexOf(sampleValues[0]);
                }
            }

            // Update each SoundDetails.Sample with the new SampleIndex values. Store
            // a map of old SoundsDetails indices to new.
            List<TRSoundDetails> soundDetails = Level.SoundDetails.ToList();
            Dictionary<int, int> soundDetailsMap = new Dictionary<int, int>();

            foreach (int soundDetailsIndex in packedAnimation.SoundDetails.Keys)
            {
                TRSoundDetails details = packedAnimation.SoundDetails[soundDetailsIndex];
                details.Sample = sampleMap[details.Sample];
                soundDetailsMap[soundDetailsIndex] = soundDetails.Count;
                soundDetails.Add(details);
            }

            // Update the SoundMap with new pointers to SoundDetails. We are limited to
            // 370 entries so make use of the -1 "null" values. Store the new map indices
            // for updating the AnimCommands.
            List<short> soundMap = Level.SoundMap.ToList();
            Dictionary<int, int> soundIndexMap = new Dictionary<int, int>();

            foreach (int soundMapIndex in packedAnimation.SoundMapIndices.Keys)
            {
                int firstAvailableSlot = soundMap.FindIndex(i => i == -1);
                if (firstAvailableSlot == -1)
                {
                    throw new Exception(string.Format("There is not space left in SoundMap for {0}", soundMapIndex));
                }

                short newSoundDetailsIndex = (short)soundDetailsMap[packedAnimation.SoundMapIndices[soundMapIndex]];
                Level.SoundMap[firstAvailableSlot] = soundMap[firstAvailableSlot] = newSoundDetailsIndex;
                soundIndexMap[soundMapIndex] = firstAvailableSlot;
            }

            // Change the Param[1] value of each PlaySound AnimCommand to point to the
            // new index in SoundMap.
            foreach (PackedAnimationCommand cmd in packedAnimation.Commands.Values)
            {
                if (cmd.Command == TR2AnimCommand.PlaySound)
                {
                    int oldSoundMapIndex = cmd.Params[1] & 0x3fff;
                    int newSoundMapIndex = soundIndexMap[oldSoundMapIndex];

                    int param = cmd.Params[1] & ~oldSoundMapIndex;
                    param |= newSoundMapIndex;
                    cmd.Params[1] = (short)param;

                    if ((cmd.Params[1] & 0x3fff) != newSoundMapIndex)
                    {
                        throw new Exception("Failed to convert new sound map index.");
                    }
                }
            }

            Level.SampleIndices = sampleIndices.ToArray();
            Level.NumSampleIndices = (uint)sampleIndices.Count;

            Level.SoundDetails = soundDetails.ToArray();
            Level.NumSoundDetails = (uint)soundDetails.Count;
        }

        private void UnpackAnimCommands(PackedAnimation packedAnimation)
        {
            if (packedAnimation.Commands.Count == 0)
            {
                return;
            }

            List<TRAnimCommand> levelAnimCommands = Level.AnimCommands.ToList();
            packedAnimation.Animation.AnimCommand = (ushort)levelAnimCommands.Count;
            foreach (PackedAnimationCommand cmd in packedAnimation.Commands.Values)
            {
                levelAnimCommands.Add(new TRAnimCommand { Value = (short)cmd.Command });
                foreach (short param in cmd.Params)
                {
                    levelAnimCommands.Add(new TRAnimCommand { Value = param });
                }
            }

            // Save back to the level
            Level.AnimCommands = levelAnimCommands.ToArray();
            Level.NumAnimCommands = (uint)levelAnimCommands.Count;
        }

        private void ResortSoundIndices()
        {
            // Store the values from SampleIndices against their current positions
            // in the list.
            List<uint> sampleIndices = Level.SampleIndices.ToList();
            Dictionary<int, uint> indexMap = new Dictionary<int, uint>();
            for (int i = 0; i < sampleIndices.Count; i++)
            {
                indexMap[i] = sampleIndices[i];
            }

            // Sort the indices to avoid the game crashing
            sampleIndices.Sort();

            // Remap each SoundDetail to use the new index of the sample it points to
            foreach (TRSoundDetails soundDetails in Level.SoundDetails)
            {
                soundDetails.Sample = (ushort)sampleIndices.IndexOf(indexMap[soundDetails.Sample]);
            }

            // Save the samples back to the level
            Level.SampleIndices = sampleIndices.ToArray();
        }

        private int UnpackAnimation(PackedAnimation animation)
        {
            List<TRAnimation> levelAnimations = Level.Animations.ToList();
            levelAnimations.Add(animation.Animation);
            Level.Animations = levelAnimations.ToArray();
            Level.NumAnimations++;

            return levelAnimations.Count - 1;
        }

        private void ImportFrames(TRModelDefinition definition)
        {
            List<ushort> levelFrames = Level.Frames.ToList();
            definition.Model.FrameOffset = (uint)levelFrames.Count * 2;

            levelFrames.AddRange(definition.Frames);
            Level.Frames = levelFrames.ToArray();
            Level.NumFrames = (uint)levelFrames.Count;

            foreach (PackedAnimation packedAnimation in definition.Animations.Values)
            {
                packedAnimation.Animation.FrameOffset += definition.Model.FrameOffset;
            }
        }

        private void ImportModel(TRModelDefinition definition)
        {
            List<TRModel> levelModels = Level.Models.ToList();
            levelModels.Add(definition.Model);
            Level.Models = levelModels.ToArray();
            Level.NumModels++;
        }

        private void ImportDependencies(TRModelDefinition definition)
        {
            List<TRModel> models = Level.Models.ToList();
            foreach (TR2Entities entity in definition.Dependencies)
            {
                if (models.FindIndex(m => m.ID == (short)entity) == -1)
                {
                    // TODO: don't import textures if we don't have to
                    Import(entity);
                }
            }
        }
    }
}