using System.Collections.Generic;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Model;
using TRModelTransporter.Model.Textures;

namespace TRModelTransporter.Transport
{
    public class TRModelExporter : AbstractTRModelTransport
    {
        protected static readonly string _segmentsFolder = @"Resources\ModelSegments";

        public bool ExportIndividualSegments { get; set; }

        public ITextureClassifier TextureClassifier { get; set; }

        public TRModelExporter()
        {
            ExportIndividualSegments = false;
        }

        public TRModelDefinition CreateModelDefinition(TR2Entities modelEntity)
        {
            Definition = new TRModelDefinition
            {
                Alias = modelEntity
            };
            
            if (_entityAliases.ContainsKey(modelEntity))
            {
                modelEntity = _entityAliases[modelEntity];
            }
            _modelHandler.ModelEntity = modelEntity;

            _textureHandler.ExportIndividualSegments = ExportIndividualSegments;
            _textureHandler.SegmentsFolder = _segmentsFolder;
            _textureHandler.TextureClassifier = TextureClassifier;

            _modelHandler.Export();
            _meshHandler.Export();
            _colourHandler.Export();
            _textureHandler.Export();
            _animationHandler.Export();
            _cinematicHandler.Export();
            _soundHandler.Export();

            ExportDependencies();

            return _definition;
        }

        private void ExportDependencies()
        {
            List<TR2Entities> dependencies = new List<TR2Entities>();
            if (_entityDependencies.ContainsKey(_definition.Entity))
            {
                dependencies.AddRange(_entityDependencies[_definition.Entity]);
            }
            _definition.Dependencies = dependencies.ToArray();
        }

        public void Export(TR2Entities entity)
        {
            CreateModelDefinition(entity);
            StoreDefinition(_definition);
        }
    }
}