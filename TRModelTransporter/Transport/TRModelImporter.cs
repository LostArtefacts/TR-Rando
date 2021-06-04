using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TRLevelReader.Helpers;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Model;
using TRModelTransporter.Model.Textures;

namespace TRModelTransporter.Transport
{
    public class TRModelImporter : AbstractTRModelTransport
    {
        public IEnumerable<TR2Entities> EntitiesToImport { get; set; }
        public IEnumerable<TR2Entities> EntitiesToRemove { get; set; }
        public bool ClearUnusedSprites { get; set; }
        public string TextureRemapPath { get; set; }
        public ITexturePositionMonitor TexturePositionMonitor { get; set; }

        public TRModelImporter()
        {
            EntitiesToImport = new List<TR2Entities>();
            EntitiesToRemove = new List<TR2Entities>();
            ClearUnusedSprites = false;
        }

        public void Import()
        {
            List<TR2Entities> existingEntities = new List<TR2Entities>();
            Level.Models.ToList().ForEach(m => existingEntities.Add((TR2Entities)m.ID));

            if (EntitiesToRemove != null)
            {
                AdjustEntities();
            }

            CleanAliases();

            List<TRModelDefinition> standardModelDefinitions = new List<TRModelDefinition>();
            List<TRModelDefinition> soundModelDefinitions = new List<TRModelDefinition>();
            foreach (TR2Entities entity in EntitiesToImport)
            {
                BuildDefinitionList(standardModelDefinitions, soundModelDefinitions, existingEntities, entity, false);
            }

            // Check for alias duplication
            ValidateDefinitionList(existingEntities);

            if (standardModelDefinitions.Count + soundModelDefinitions.Count > 0)
            {
                Import(standardModelDefinitions, soundModelDefinitions);
            }
        }

        private void AdjustEntities()
        {
            // If an entity is marked to be removed but is also in the list
            // to import, don't remove it in the first place.
            List<TR2Entities> cleanedEntities = new List<TR2Entities>();
            foreach (TR2Entities entity in EntitiesToRemove)
            {
                if (_aliasMap.ContainsKey(entity))
                {
                    // Check if we have another alias in the import list different from any
                    // in the current level
                    TR2Entities alias = TR2EntityUtilities.GetAliasForLevel(LevelName, entity);
                    TR2Entities importAlias = TR2Entities.Lara;
                    foreach (TR2Entities a in _aliasMap[entity])
                    {
                        if (EntitiesToImport.Contains(a))
                        {
                            importAlias = a;
                            break;
                        }
                    }

                    if (alias != importAlias)
                    {
                        cleanedEntities.Add(entity);
                    }
                }
                else if (!EntitiesToImport.Contains(entity))
                {
                    cleanedEntities.Add(entity);
                }
            }
            EntitiesToRemove = cleanedEntities;
        }

        private void CleanAliases()
        {
            List<TR2Entities> cleanedEntities = new List<TR2Entities>();
            // Do we have any aliases?
            foreach (TR2Entities importEntity in EntitiesToImport)
            {
                if (_aliasMap.ContainsKey(importEntity))
                {
                    throw new TransportException(string.Format
                    (
                        "Cannot import ambiguous entity {0} - choose an alias from [{1}].",
                        importEntity.ToString(),
                        string.Join(", ", _aliasMap[importEntity])
                    ));
                }

                bool entityIsValid = true;
                if (_entityAliases.ContainsKey(importEntity))
                {
                    TR2Entities existingEntity = TR2EntityUtilities.GetAliasForLevel(LevelName, _entityAliases[importEntity]);
                    // This entity is only valid if the alias it's for is not already there
                    entityIsValid = importEntity != existingEntity;
                }

                if (entityIsValid)
                {
                    cleanedEntities.Add(importEntity);
                }
            }

            // #139 Ensure that aliases are added last so to avoid dependency issues
            cleanedEntities.Sort(delegate (TR2Entities e1, TR2Entities e2)
            {
                return ((short)TR2EntityUtilities.TranslateEntityAlias(e1)).CompareTo((short)TR2EntityUtilities.TranslateEntityAlias(e2));
            });

            // For some reason, if the Barracuda is added before the shark, there is a slight animation
            // corruption on the shark's mouth. I can't find the reason. The patch is to ensure the 
            // Barracuda is added last.
            // #137 Reason found - animated textures were not being reindexed following deduplication. See 
            // TRModelExtensions.ReindexTextures and #137.
            /*int barracudaIndex = cleanedEntities.FindIndex(e => _aliasMap[TR2Entities.Barracuda].Contains(e));
            if (barracudaIndex != -1)
            {
                TR2Entities barracuda = cleanedEntities[barracudaIndex];
                cleanedEntities.RemoveAt(barracudaIndex);
                cleanedEntities.Add(barracuda);
            }*/

            EntitiesToImport = cleanedEntities;
        }

        private void ValidateDefinitionList(List<TR2Entities> modelEntities)
        {
            Dictionary<TR2Entities, List<TR2Entities>> detectedAliases = new Dictionary<TR2Entities, List<TR2Entities>>();
            foreach (TR2Entities entity in modelEntities)
            {
                if (_entityAliases.ContainsKey(entity))
                {
                    TR2Entities masterEntity = _entityAliases[entity];
                    if (!detectedAliases.ContainsKey(masterEntity))
                    {
                        detectedAliases[masterEntity] = new List<TR2Entities>();
                    }
                    detectedAliases[_entityAliases[entity]].Add(entity);
                }
            }

            foreach (TR2Entities masterEntity in detectedAliases.Keys)
            {
                if (detectedAliases[masterEntity].Count > 1 && !_permittedAliasDuplicates.Contains(masterEntity))
                {
                    throw new TransportException(string.Format
                    (
                        "Only one alias per entity can exist in the same level. [{0}] were found as aliases for {1}.",
                        string.Join(", ", detectedAliases[masterEntity]),
                        masterEntity.ToString()
                    ));
                }
            }
        }

        private void BuildDefinitionList(List<TRModelDefinition> standardModelDefinitions, List<TRModelDefinition> soundModelDefinitions, List<TR2Entities> modelEntities, TR2Entities nextEntity, bool isDependency)
        {
            if (modelEntities.Contains(nextEntity))
            {
                // If the model already in the list is a dependency only, but the new one to add isn't, switch it
                TRModelDefinition definition = standardModelDefinitions.Find(m => m.Alias == nextEntity);
                if (definition != null && definition.IsDependencyOnly && !isDependency)
                {
                    definition.IsDependencyOnly = false;
                }
                return;
            }

            TRModelDefinition nextDefinition = LoadDefinition(nextEntity);
            nextDefinition.IsDependencyOnly = isDependency;
            modelEntities.Add(nextEntity);

            // Add dependencies first
            foreach (TR2Entities dependency in nextDefinition.Dependencies)
            {
                // If it's a non-graphics dependency, but we are importing another alias
                // for it, or the level already contains the dependency, we don't need it.
                bool nonGraphics = _noGraphicsEntityDependencies.Contains(dependency);
                TR2Entities aliasFor = TR2EntityUtilities.TranslateEntityAlias(dependency);
                if (aliasFor != dependency && nonGraphics)
                {
                    bool required = true;
                    // #139 check entire model list for instances where alias and dependencies cause clashes
                    foreach (TR2Entities entity in modelEntities)
                    {
                        // If this entity and the dependency are in the same family
                        if (aliasFor == TR2EntityUtilities.TranslateEntityAlias(entity))
                        {
                            // Skip it
                            required = false;
                            break;
                        }
                    }

                    if (!required)
                    {
                        // We don't need the graphics, but do we need hardcoded sound?
                        if (_soundOnlyDependencies.Contains(dependency) && standardModelDefinitions.Find(m => m.Alias == dependency) == null)
                        {
                            soundModelDefinitions.Add(LoadDefinition(dependency));
                        }

                        continue;
                    }
                }

                BuildDefinitionList(standardModelDefinitions, soundModelDefinitions, modelEntities, dependency, nonGraphics);
            }

            standardModelDefinitions.Add(nextDefinition);
        }

        private void Import(IEnumerable<TRModelDefinition> standardDefinitions, IEnumerable<TRModelDefinition> soundOnlyDefinitions)
        {
            // Textures first, which will remap Mesh rectangles/triangles to the new texture indices.
            // This is called using the entire entity list to import so that RectanglePacker packer has
            // the best chance to organise the tiles.
            _textureHandler.Definitions = standardDefinitions;
            _textureHandler.EntitiesToRemove = EntitiesToRemove;
            _textureHandler.ClearUnusedSprites = ClearUnusedSprites;
            _textureHandler.TextureRemap = JsonConvert.DeserializeObject<TextureRemapGroup>(File.ReadAllText(TextureRemapPath));
            _textureHandler.PositionMonitor = TexturePositionMonitor;
            _textureHandler.Import();

            // Hardcoded sounds are also imported en-masse to avoid to ensure the correct SoundMap indices are assigned
            // before any animation sounds are dealt with.
            _soundHandler.Definitions = standardDefinitions.Concat(soundOnlyDefinitions);
            _soundHandler.Import();

            foreach (TRModelDefinition definition in standardDefinitions)
            {
                Definition = definition;
                // Colours next, again to remap Mesh rectangles/triangles to any new palette indices
                _colourHandler.Import();

                // Meshes and trees should now be remapped, so import into the level
                _meshHandler.Import();

                // Animations, AnimCommands, AnimDispatches, Sounds, StateChanges and Frames
                _animationHandler.Import();

                // Cinematic frames
                _cinematicHandler.Import();

                // Add the model, which will have the correct StartingMesh, MeshTree, Frame and Animation offset.
                _modelHandler.Import();
            }

            _textureHandler.ResetUnusedTextures();
        }
    }
}