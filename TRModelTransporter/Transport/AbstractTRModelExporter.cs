using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using TRModelTransporter.Events;
using TRModelTransporter.Handlers;
using TRModelTransporter.Model;
using TRModelTransporter.Model.Textures;

namespace TRModelTransporter.Transport
{
    public abstract class AbstractTRModelExporter<E, L, D> : AbstractTRModelTransport<E, L, D>
        where E : Enum
        where L : class
        where D : AbstractTRModelDefinition<E>
    {
        protected static readonly string _defaultSegmentsFolder = @"Resources\ModelSegments";

        public bool ExportIndividualSegments { get; set; }
        public string SegmentsDataFolder { get; set; }

        public ITextureClassifier TextureClassifier { get; set; }

        protected AbstractTextureExportHandler<E, L, D> _textureHandler;

        public AbstractTRModelExporter()
        {
            SegmentsDataFolder = _defaultSegmentsFolder;
            _textureHandler = CreateTextureHandler();
        }

        protected abstract AbstractTextureExportHandler<E, L, D> CreateTextureHandler();

        public D Export(L level, E entity)
        {
            EventHandler<SegmentEventArgs> segmentDelegate = null;
            if (ExportIndividualSegments)
            {
                string dir = Path.Combine(SegmentsDataFolder, entity.ToString());
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, true);
                }
                Directory.CreateDirectory(dir);

                _textureHandler.SegmentExported += segmentDelegate = delegate (object sender, SegmentEventArgs e)
                {
                    e.Bitmap.Save(Path.Combine(dir, e.SegmentIndex + ".png"), ImageFormat.Png);
                };
            }

            D definition = CreateModelDefinition(level, entity);
            ExportDependencies(definition);
            StoreDefinition(definition);

            if (ExportIndividualSegments)
            {
                _textureHandler.SegmentExported -= segmentDelegate;
            }

            return definition;
        }

        protected abstract D CreateModelDefinition(L level, E modelEntity);

        private void ExportDependencies(D definition)
        {
            List<E> dependencies = new List<E>(Data.GetModelDependencies(definition.Alias));
            definition.Dependencies = dependencies.ToArray();
        }
    }
}