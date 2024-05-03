using Newtonsoft.Json;
using System.Drawing;
using System.Drawing.Imaging;
using TRModelTransporter.Events;
using TRModelTransporter.Handlers;
using TRModelTransporter.Model;
using TRModelTransporter.Model.Textures;
using TRModelTransporter.Utilities;
using TRTexture16Importer.Textures;

namespace TRModelTransporter.Transport;

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
        List<E> dependencies = new(Data.GetModelDependencies(definition.Alias));
        definition.Dependencies = dependencies.ToArray();
    }

    protected void AmendDXtre3DTextures(D _)
    {
        // Dxtre3D can produce faulty UV mapping which can cause casting issues
        // when used in model IO, so fix coordinates at this stage.
        // This may no longer be the case...
        /*foreach (List<IndexedTRObjectTexture> textureList in definition.ObjectTextures.Values)
        {
            foreach (IndexedTRObjectTexture texture in textureList)
            {
                Dictionary<TRObjectTextureVert, Point> points = new();
                foreach (TRObjectTextureVert vertex in texture.Texture.Vertices)
                {
                    int x = vertex.XCoordinate.Fraction;
                    if (vertex.XCoordinate.Whole == byte.MaxValue)
                    {
                        x++;
                    }

                    int y = vertex.YCoordinate.Fraction;
                    if (vertex.YCoordinate.Whole == byte.MaxValue)
                    {
                        y++;
                    }
                    points[vertex] = new Point(x, y);
                }

                int maxX = points.Values.Max(p => p.X);
                int maxY = points.Values.Max(p => p.Y);
                foreach (TRObjectTextureVert vertex in texture.Texture.Vertices)
                {
                    Point p = points[vertex];
                    if (p.X == maxX && maxX != byte.MaxValue)
                    {
                        vertex.XCoordinate.Fraction--;
                        vertex.XCoordinate.Whole = byte.MaxValue;
                    }
                    if (p.Y == maxY && maxY != byte.MaxValue)
                    {
                        vertex.YCoordinate.Fraction--;
                        vertex.YCoordinate.Whole = byte.MaxValue;
                    }
                }
            }
        }*/
    }
}
