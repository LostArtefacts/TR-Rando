using System;
using System.Collections.Generic;
using System.Linq;
using TRModelTransporter.Handlers;
using TRModelTransporter.Model;
using TRModelTransporter.Model.Textures;

namespace TRModelTransporter.Transport
{
    public abstract class AbstractTRModelImporter<E, L, D> : AbstractTRModelTransport<E, L, D> 
        where E : Enum
        where L : class
        where D : AbstractTRModelDefinition<E>
    {
        public IEnumerable<E> EntitiesToImport { get; set; }
        public IEnumerable<E> EntitiesToRemove { get; set; }
        public bool ClearUnusedSprites { get; set; }
        public string TextureRemapPath { get; set; }
        public ITexturePositionMonitor<E> TexturePositionMonitor { get; set; }
        public bool SortModels { get; set; }

        protected AbstractTextureImportHandler<E, L, D> _textureHandler;

        public AbstractTRModelImporter()
        {
            EntitiesToImport = new List<E>();
            EntitiesToRemove = new List<E>();
            ClearUnusedSprites = false;
        }

        protected abstract AbstractTextureImportHandler<E, L, D> CreateTextureHandler();

        public void Import()
        {
            if (_textureHandler == null)
            {
                _textureHandler = CreateTextureHandler();
                _textureHandler.Data = Data;
            }

            List<E> existingEntities = GetExistingModelTypes();

            if (EntitiesToRemove != null)
            {
                AdjustEntities();
            }

            CleanAliases();

            List<D> standardModelDefinitions = new List<D>();
            List<D> soundModelDefinitions = new List<D>();
            foreach (E entity in EntitiesToImport)
            {
                BuildDefinitionList(standardModelDefinitions, soundModelDefinitions, existingEntities, entity, false);
            }

            // Check for alias duplication
            ValidateDefinitionList(existingEntities, standardModelDefinitions);

            if (SortModels)
            {
                standardModelDefinitions.Sort(delegate (D d1, D d2)
                {
                    return d1.Entity.CompareTo(d2.Entity);
                });
                soundModelDefinitions.Sort(delegate (D d1, D d2)
                {
                    return d1.Entity.CompareTo(d2.Entity);
                });
            }

            try
            {
                if (standardModelDefinitions.Count + soundModelDefinitions.Count > 0)
                {
                    Import(standardModelDefinitions, soundModelDefinitions);
                }
            }
            finally
            {
                // Bitmap cleanup
                standardModelDefinitions.ForEach(d => d.Dispose());
                soundModelDefinitions.ForEach(d => d.Dispose());
            }
        }

        private void AdjustEntities()
        {
            // If an entity is marked to be removed but is also in the list
            // to import, don't remove it in the first place.
            List<E> cleanedEntities = new List<E>();
            foreach (E entity in EntitiesToRemove)
            {
                bool entityClean = false;
                if (Data.HasAliases(entity))
                {
                    // Check if we have another alias in the import list different from any
                    // in the current level
                    E alias = Data.GetLevelAlias(LevelName, entity);
                    E importAlias = default;
                    foreach (E a in Data.GetAliases(entity))
                    {
                        if (EntitiesToImport.Contains(a))
                        {
                            importAlias = a;
                            break;
                        }
                    }

                    if (!Equals(alias, importAlias))
                    {
                        entityClean = true;
                    }
                }
                else if (!EntitiesToImport.Contains(entity))
                {
                    entityClean = true;
                }

                if (entityClean)
                {
                    // There may be null meshes dependent on this removal, so we can only remove it if they're
                    // being removed as well.
                    IEnumerable<E> exclusions = Data.GetRemovalExclusions(entity);
                    if (exclusions.Count() > 0 && exclusions.All(EntitiesToRemove.Contains))
                    {
                        entityClean = false;
                    }
                }

                if (entityClean)
                {
                    cleanedEntities.Add(entity);
                }
            }
            EntitiesToRemove = cleanedEntities;
        }

        private void CleanAliases()
        {
            List<E> cleanedEntities = new List<E>();
            // Do we have any aliases?
            foreach (E importEntity in EntitiesToImport)
            {
                if (Data.HasAliases(importEntity))
                {
                    throw new TransportException(string.Format
                    (
                        "Cannot import ambiguous entity {0} - choose an alias from [{1}].",
                        importEntity.ToString(),
                        string.Join(", ", Data.GetAliases(importEntity))
                    ));
                }

                bool entityIsValid = true;
                if (Data.IsAlias(importEntity))
                {
                    E existingEntity = Data.GetLevelAlias(LevelName, Data.TranslateAlias(importEntity));
                    // This entity is only valid if the alias it's for is not already there
                    entityIsValid = !Equals(importEntity, existingEntity);
                }

                if (entityIsValid)
                {
                    cleanedEntities.Add(importEntity);
                }
            }

            // #139 Ensure that aliases are added last so to avoid dependency issues
            cleanedEntities.Sort(delegate (E e1, E e2)
            {
                return Data.TranslateAlias(e1).CompareTo(Data.TranslateAlias(e2));
            });

            EntitiesToImport = cleanedEntities;
        }

        private void ValidateDefinitionList(List<E> modelEntities, List<D> importDefinitions)
        {
            Dictionary<E, List<E>> detectedAliases = new Dictionary<E, List<E>>();
            foreach (E entity in modelEntities)
            {
                if (Data.IsAlias(entity))
                {
                    E masterEntity = Data.TranslateAlias(entity);
                    if (!detectedAliases.ContainsKey(masterEntity))
                    {
                        detectedAliases[masterEntity] = new List<E>();
                    }
                    detectedAliases[masterEntity].Add(entity);
                }
            }

            foreach (E masterEntity in detectedAliases.Keys)
            {
                if (detectedAliases[masterEntity].Count > 1)
                {
                    if (!Data.IsAliasDuplicatePermitted(masterEntity))
                    {
                        throw new TransportException(string.Format
                        (
                            "Only one alias per entity can exist in the same level. [{0}] were found as aliases for {1}.",
                            string.Join(", ", detectedAliases[masterEntity]),
                            masterEntity.ToString()
                        ));
                    }
                    else if (Data.AliasPriority.ContainsKey(masterEntity))
                    {
                        // If we are importing two aliases such as LaraMiscAnim_Unwater and LaraMiscAnim_Xian,
                        // allow the priority list to define exactly what imports. Otherwise while the prioritised
                        // model will be imported, other aspects such as texture import will try to import both.
                        E prioritisedType = Data.AliasPriority[masterEntity];
                        importDefinitions.RemoveAll(d => detectedAliases[masterEntity].Contains(d.Alias) && !Equals(d.Alias, prioritisedType));
                    }
                }
            }
        }

        private void BuildDefinitionList(List<D> standardModelDefinitions, List<D> soundModelDefinitions, List<E> modelEntities, E nextEntity, bool isDependency)
        {
            if (modelEntities.Contains(nextEntity))
            {
                // Are we allowed to replace it?
                if (!Data.IsOverridePermitted(nextEntity))
                {
                    // If the model already in the list is a dependency only, but the new one to add isn't, switch it
                    D definition = standardModelDefinitions.Find(m => Equals(m.Alias, nextEntity));
                    if (definition != null && definition.IsDependencyOnly && !isDependency)
                    {
                        definition.IsDependencyOnly = false;
                    }
                    else if (EntitiesToRemove.Contains(nextEntity))
                    {
                        EntitiesToRemove = new List<E>(EntitiesToRemove).Except(new List<E> { nextEntity });
                    }

                    // Avoid issues with cyclic dependencies by adding separately. The caveat here is
                    // cyclic dependencies can't have further sub-dependencies.
                    IEnumerable<E> cyclicDependencies = Data.GetCyclicDependencies(nextEntity);
                    foreach (E cyclicDependency in cyclicDependencies)
                    {
                        if (!modelEntities.Contains(cyclicDependency) || Data.IsOverridePermitted(cyclicDependency))
                        {
                            modelEntities.Add(cyclicDependency);
                            standardModelDefinitions.Add(LoadDefinition(cyclicDependency));
                        }
                    }

                    return;
                }
            }

            D nextDefinition = LoadDefinition(nextEntity);
            nextDefinition.IsDependencyOnly = isDependency;
            modelEntities.Add(nextEntity);

            // Add dependencies first
            foreach (E dependency in nextDefinition.Dependencies)
            {
                // If it's a non-graphics dependency, but we are importing another alias
                // for it, or the level already contains the dependency, we don't need it.
                bool nonGraphics = Data.IsNonGraphicsDependency(dependency);
                E aliasFor = Data.TranslateAlias(dependency);
                if (!Equals(aliasFor, dependency) && nonGraphics)
                {
                    bool required = true;
                    // #139 check entire model list for instances where alias and dependencies cause clashes
                    foreach (E entity in modelEntities)
                    {
                        // If this entity and the dependency are in the same family
                        if (Equals(aliasFor, Data.TranslateAlias(entity)))
                        {
                            // Skip it
                            required = false;
                            break;
                        }
                    }

                    if (!required)
                    {
                        // We don't need the graphics, but do we need hardcoded sound?
                        if (Data.IsSoundOnlyDependency(dependency) && standardModelDefinitions.Find(m => Equals(m.Alias, dependency)) == null)
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

        protected abstract List<E> GetExistingModelTypes();
        protected abstract void Import(IEnumerable<D> standardDefinitions, IEnumerable<D> soundOnlyDefinitions);
    }
}