using Newtonsoft.Json;
using System.Drawing.Imaging;
using TRModelTransporter.Data;
using TRModelTransporter.Handlers;
using TRModelTransporter.Model;

namespace TRModelTransporter.Transport;

public abstract class TRDataTransport<E, L, D> 
    where E : Enum
    where L : class
    where D : TRBlobBase<E>
{
    protected static readonly string _defaultDataFolder = @"Resources\Models";
    protected static readonly string _dataFileName = "Data.json";
    protected static readonly string _imageFileName = "Segments.png";

    public IDataProvider<E> Data { get; set; }
    public L Level { get; set; }

    public string LevelName { get; set; }

    public string DataFolder { get; set; }

    protected readonly CinematicTransportHandler _cinematicHandler;
    protected readonly ModelTransportHandler _modelHandler;
    protected readonly ColourTransportHandler _colourHandler;
    protected readonly SoundTransportHandler _soundHandler;

    public TRDataTransport()
    {
        DataFolder = _defaultDataFolder;

        _cinematicHandler = new CinematicTransportHandler();
        _modelHandler = new ModelTransportHandler();
        _colourHandler = new ColourTransportHandler();
        _soundHandler = new SoundTransportHandler();
    }

    protected void StoreDefinition(D definition)
    {
        string directory = Path.Combine(DataFolder, definition.Alias.ToString());
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (definition.HasGraphics)
        {
            definition.Image.Save(Path.Combine(directory, _imageFileName), ImageFormat.Png);
        }
        File.WriteAllText(Path.Combine(directory, _dataFileName), JsonConvert.SerializeObject(definition, Formatting.None));
    }

    public D LoadDefinition(E modelEntity)
    {
        string directory = Path.Combine(DataFolder, modelEntity.ToString());
        string dataFilePath = Path.Combine(directory, _dataFileName);
        string imageFilePath = Path.Combine(directory, _imageFileName);

        if (!File.Exists(dataFilePath))
        {
            throw new IOException(string.Format("Missing model data JSON file ({0})", dataFilePath));
        }

        D definition = JsonConvert.DeserializeObject<D>(File.ReadAllText(dataFilePath));
        definition.Alias = modelEntity;

        if (definition.HasGraphics)
        {
            if (!File.Exists(imageFilePath))
            {
                throw new IOException(string.Format("Missing model data image file ({0})", imageFilePath));
            }
            definition.Image = new(imageFilePath);
        }
        return definition;
    }

    protected bool Equals(E e1, E e2)
    {
        return EqualityComparer<E>.Default.Equals(e1, e2);
    }
}
