using System;
using System.Collections.Generic;
using System.IO;
using TRModelTransporter.Model;
using TRModelTransporter.Transport;

namespace TRModelTransporter.Utilities
{
    public abstract class AbstractMassTRModelExporter<E, L, D>
        where E : Enum
        where L : class
        where D : AbstractTRModelDefinition<E>
    {
        public abstract List<string> LevelNames { get; }
        public abstract Dictionary<string, List<E>> ExportTypes { get; }

        private AbstractTRModelExporter<E, L, D> _exporter;
        private List<E> _processedEntities;

        public void Export(string levelFileDirectory, string exportDirectory, string segmentsDirectory = null)
        {
            _exporter = CreateExporter();
            _exporter.ExportIndividualSegments = segmentsDirectory != null;
            _exporter.SegmentsDataFolder = segmentsDirectory;
            _exporter.DataFolder = exportDirectory;
            _processedEntities = new List<E>();

            foreach (string lvlName in LevelNames)
            {
                _exporter.LevelName = lvlName;
                if (ExportTypes.ContainsKey(lvlName))
                {
                    string levelPath = Path.Combine(levelFileDirectory, lvlName);
                    foreach (E entity in ExportTypes[lvlName])
                    {
                        Export(levelPath, entity);
                    }
                }
            }
        }

        private void Export(string levelPath, E entity)
        {
            if (!_processedEntities.Contains(entity))
            {
                // The level has to be re-read per entity because TextureTransportHandler can modify ObjectTextures
                // which when shared between entities is difficult to undo.
                _exporter.TextureClassifier = new TRTextureClassifier(levelPath);
                L level = ReadLevel(levelPath);
                D definition = _exporter.Export(level, entity);
                _processedEntities.Add(entity);

                foreach (E dependency in definition.Dependencies)
                {
                    Export(levelPath, dependency);
                }
            }
        }

        protected abstract AbstractTRModelExporter<E, L, D> CreateExporter();
        protected abstract L ReadLevel(string path);
    }
}