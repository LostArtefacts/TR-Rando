using TRLevelControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRModelTransporter.Model.Definitions;
using TRModelTransporter.Transport;

namespace TRModelTransporter.Utilities;

public class TR3MassExporter : TRMassExporter<TR3Type, TR3Level, TR3Blob>
{
    public override List<string> LevelNames => TR3LevelNames.AsListWithAssault;

    public override Dictionary<string, List<TR3Type>> ExportTypes => _exportModelTypes;

    private readonly TR3LevelControl _reader;

    public TR3MassExporter()
    {
        _reader = new();
    }

    protected override TRDataExporter<TR3Type, TR3Level, TR3Blob> CreateExporter()
    {
        return new TR3DataExporter();
    }

    protected override TR3Level ReadLevel(string path)
    {
        return _reader.Read(path);
    }

    private static readonly Dictionary<string, List<TR3Type>> _exportModelTypes = new()
    {
        [TR3LevelNames.JUNGLE] = new List<TR3Type>
        {
            TR3Type.LaraIndia, TR3Type.Monkey, TR3Type.Tiger, TR3Type.Door1
        },
        [TR3LevelNames.RUINS] = new List<TR3Type>
        {
            TR3Type.Shiva, TR3Type.CobraIndia
        },
        [TR3LevelNames.GANGES] = new List<TR3Type>
        {
            TR3Type.Quad, TR3Type.Vulture
        },
        [TR3LevelNames.CAVES] = new List<TR3Type>
        {
            TR3Type.TonyFirehands, TR3Type.Infada_P
        },
        [TR3LevelNames.COASTAL] = new List<TR3Type>
        {
            TR3Type.LaraCoastal, TR3Type.Croc, TR3Type.TribesmanAxe, TR3Type.TribesmanDart
        },
        [TR3LevelNames.CRASH] = new List<TR3Type>
        {
            TR3Type.Compsognathus, TR3Type.Mercenary, TR3Type.Raptor, TR3Type.Tyrannosaur
        },
        [TR3LevelNames.MADUBU] = new List<TR3Type>
        {
            TR3Type.Kayak, TR3Type.LizardMan
        },
        [TR3LevelNames.PUNA] = new List<TR3Type>
        {
            TR3Type.Puna, TR3Type.OraDagger_P
        },
        [TR3LevelNames.THAMES] = new List<TR3Type>
        {
            TR3Type.LaraLondon, TR3Type.Crow, TR3Type.LondonGuard, TR3Type.LondonMerc, TR3Type.Rat
        },
        [TR3LevelNames.ALDWYCH] = new List<TR3Type>
        {
            TR3Type.Punk, TR3Type.DogLondon
        },
        [TR3LevelNames.LUDS] = new List<TR3Type>
        {
            TR3Type.ScubaSteve, TR3Type.UPV
        },
        [TR3LevelNames.CITY] = new List<TR3Type>
        {
            TR3Type.SophiaLee, TR3Type.EyeOfIsis_P
        },
        [TR3LevelNames.NEVADA] = new List<TR3Type>
        {
            TR3Type.LaraNevada, TR3Type.DamGuard, TR3Type.CobraNevada
        },
        [TR3LevelNames.HSC] = new List<TR3Type>
        {
            TR3Type.MPWithStick, TR3Type.MPWithGun, TR3Type.Prisoner, TR3Type.DogNevada
        },
        [TR3LevelNames.AREA51] = new List<TR3Type>
        {
            TR3Type.KillerWhale, TR3Type.MPWithMP5, TR3Type.Element115_P
        },
        [TR3LevelNames.ANTARC] = new List<TR3Type>
        {
            TR3Type.LaraAntarc, TR3Type.CrawlerMutantInCloset, TR3Type.Boat, TR3Type.RXRedBoi, TR3Type.DogAntarc
        }
        ,
        [TR3LevelNames.RXTECH] = new List<TR3Type>
        {
            TR3Type.Crawler, TR3Type.RXTechFlameLad, TR3Type.BruteMutant
        },
        [TR3LevelNames.TINNOS] = new List<TR3Type>
        {
            TR3Type.TinnosMonster, TR3Type.TinnosWasp, TR3Type.Door4
        },
        [TR3LevelNames.WILLIE] = new List<TR3Type>
        {
            TR3Type.Willie, TR3Type.RXGunLad
        },
        [TR3LevelNames.HALLOWS] = new List<TR3Type>
        {
            
        },
        [TR3LevelNames.ASSAULT] = new List<TR3Type>
        {
            TR3Type.LaraHome, TR3Type.Winston, TR3Type.WinstonInCamoSuit
        }
    };
}
