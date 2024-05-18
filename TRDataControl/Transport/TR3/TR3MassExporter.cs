using TRLevelControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRDataControl;

public class TR3MassExporter : TRMassExporter<TR3Level, TR3Type, TR3SFX, TR3Blob>
{
    public override Dictionary<string, List<TR3Type>> Data => _data;

    protected override TRDataExporter<TR3Level, TR3Type, TR3SFX, TR3Blob> CreateExporter()
        => new TR3DataExporter();

    protected override TR3Level ReadLevel(string path)
        => new TR3LevelControl().Read(path);

    private static readonly Dictionary<string, List<TR3Type>> _data = new()
    {
        [TR3LevelNames.JUNGLE] = new()
        {
            TR3Type.LaraIndia, TR3Type.Monkey, TR3Type.Tiger, TR3Type.Door1
        },
        [TR3LevelNames.RUINS] = new()
        {
            TR3Type.Shiva, TR3Type.CobraIndia
        },
        [TR3LevelNames.GANGES] = new()
        {
            TR3Type.Quad, TR3Type.Vulture
        },
        [TR3LevelNames.CAVES] = new()
        {
            TR3Type.TonyFirehands, TR3Type.Infada_P
        },
        [TR3LevelNames.COASTAL] = new()
        {
            TR3Type.LaraCoastal, TR3Type.Croc, TR3Type.TribesmanAxe, TR3Type.TribesmanDart, TR3Type.Quest1_P
        },
        [TR3LevelNames.CRASH] = new()
        {
            TR3Type.Compsognathus, TR3Type.Mercenary, TR3Type.Raptor, TR3Type.Tyrannosaur
        },
        [TR3LevelNames.MADUBU] = new()
        {
            TR3Type.Kayak, TR3Type.LizardMan
        },
        [TR3LevelNames.PUNA] = new()
        {
            TR3Type.Puna, TR3Type.OraDagger_P
        },
        [TR3LevelNames.THAMES] = new()
        {
            TR3Type.LaraLondon, TR3Type.Crow, TR3Type.LondonGuard, TR3Type.LondonMerc, TR3Type.Rat
        },
        [TR3LevelNames.ALDWYCH] = new()
        {
            TR3Type.Punk, TR3Type.DogLondon
        },
        [TR3LevelNames.LUDS] = new()
        {
            TR3Type.ScubaSteve, TR3Type.UPV
        },
        [TR3LevelNames.CITY] = new()
        {
            TR3Type.SophiaLee, TR3Type.EyeOfIsis_P
        },
        [TR3LevelNames.NEVADA] = new()
        {
            TR3Type.LaraNevada, TR3Type.DamGuard, TR3Type.CobraNevada
        },
        [TR3LevelNames.HSC] = new()
        {
            TR3Type.MPWithStick, TR3Type.MPWithGun, TR3Type.Prisoner, TR3Type.DogNevada
        },
        [TR3LevelNames.AREA51] = new()
        {
            TR3Type.KillerWhale, TR3Type.MPWithMP5, TR3Type.Element115_P
        },
        [TR3LevelNames.ANTARC] = new()
        {
            TR3Type.LaraAntarc, TR3Type.CrawlerMutantInCloset, TR3Type.Boat, TR3Type.RXRedBoi, TR3Type.DogAntarc
        }
        ,
        [TR3LevelNames.RXTECH] = new()
        {
            TR3Type.Crawler, TR3Type.RXTechFlameLad, TR3Type.BruteMutant
        },
        [TR3LevelNames.TINNOS] = new()
        {
            TR3Type.TinnosMonster, TR3Type.TinnosWasp, TR3Type.Door4
        },
        [TR3LevelNames.WILLIE] = new()
        {
            TR3Type.Willie, TR3Type.RXGunLad
        },
        [TR3LevelNames.MADHOUSE] = new()
        {
            TR3Type.Quest2_P
        },
        [TR3LevelNames.ASSAULT] = new()
        {
            TR3Type.LaraHome, TR3Type.Winston, TR3Type.WinstonInCamoSuit
        }
    };
}
