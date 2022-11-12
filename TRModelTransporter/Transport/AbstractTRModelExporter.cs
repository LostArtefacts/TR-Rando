using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using TRModelTransporter.Events;
using TRModelTransporter.Handlers;
using TRModelTransporter.Model;
using TRModelTransporter.Model.Textures;
using TRModelTransporter.Utilities;
using TRTexture16Importer.Textures;

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
            EventHandler<TRTextureRemapEventArgs> segmentRemapped = null;
            List<StaticTextureTarget> duplicateClips = null;
            string segmentDir = Path.Combine(SegmentsDataFolder, entity.ToString());
            if (ExportIndividualSegments)
            {
                if (Directory.Exists(segmentDir))
                {
                    Directory.Delete(segmentDir, true);
                }
                Directory.CreateDirectory(segmentDir);

                _textureHandler.SegmentExported += segmentDelegate = delegate (object sender, SegmentEventArgs e)
                {
                    e.Bitmap.Save(Path.Combine(segmentDir, e.SegmentIndex + ".png"), ImageFormat.Png);
                };

                duplicateClips = new List<StaticTextureTarget>();
                _textureHandler.SegmentRemapped += segmentRemapped = delegate (object sender, TRTextureRemapEventArgs e)
                {
                    duplicateClips.Add(new StaticTextureTarget
                    {
                        Segment = e.NewSegment.FirstTextureIndex,
                        Tile = e.OldTile.Index,
                        X = e.OldBounds.X,
                        Y = e.OldBounds.Y,
                        Clip = new Rectangle(e.AdjustmentPoint.X - e.NewBounds.X, e.AdjustmentPoint.Y - e.NewBounds.Y, e.OldBounds.Width, e.OldBounds.Height)
                    });
                };
            }

            PreDefinitionCreation(level, entity);
            D definition = CreateModelDefinition(level, entity);
            ExportDependencies(definition);
            ModelExportReady(definition);
            StoreDefinition(definition);

            if (ExportIndividualSegments)
            {
                _textureHandler.SegmentExported -= segmentDelegate;
                _textureHandler.SegmentRemapped -= segmentRemapped;

                File.WriteAllText(Path.Combine(segmentDir, "DuplicateClips.json"), JsonConvert.SerializeObject(duplicateClips, Formatting.Indented));
            }

            return definition;
        }

        protected virtual void PreDefinitionCreation(L level, E modelEntity) { }
        protected abstract D CreateModelDefinition(L level, E modelEntity);
        protected virtual void ModelExportReady(D definition) { }

        private void ExportDependencies(D definition)
        {
            List<E> dependencies = new List<E>(Data.GetModelDependencies(definition.Alias));
            definition.Dependencies = dependencies.ToArray();
        }
    }
}