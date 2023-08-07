using System.Collections.Generic;
using System.Linq;
using TRLevelControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRLevelControl.Model.Enums;
using TRModelTransporter.Model.Definitions;
using TRModelTransporter.Transport;

namespace TRModelTransporter.Utilities
{
    public class MassTR2ModelExporter : AbstractMassTRModelExporter<TR2Entities, TR2Level, TR2ModelDefinition>
    {
        private static readonly List<string> _sourceLevels = TR2LevelNames.AsListWithAssault.Concat(new List<string>
        {
            // https://trcustoms.org/levels/3013 by Topixtor
            "TOPIORC.TR2",
            "TOPICAC.TR2",
        }).ToList();

        public override List<string> LevelNames => _sourceLevels;

        public override Dictionary<string, List<TR2Entities>> ExportTypes => _exportModelTypes;

        private readonly TR2LevelControl _reader;

        public MassTR2ModelExporter()
        {
            _reader = new TR2LevelControl();
        }

        protected override AbstractTRModelExporter<TR2Entities, TR2Level, TR2ModelDefinition> CreateExporter()
        {
            return new TR2ModelExporter();
        }

        protected override TR2Level ReadLevel(string path)
        {
            return _reader.Read(path);
        }

        private static readonly Dictionary<string, List<TR2Entities>> _exportModelTypes = new Dictionary<string, List<TR2Entities>>
        {
            [TR2LevelNames.GW] = new List<TR2Entities>
            {
                TR2Entities.Pistols_M_H, TR2Entities.Shotgun_M_H, TR2Entities.Uzi_M_H, TR2Entities.Autos_M_H, TR2Entities.Harpoon_M_H, TR2Entities.M16_M_H, TR2Entities.GrenadeLauncher_M_H,
                TR2Entities.LaraSun, TR2Entities.Crow, TR2Entities.Spider, TR2Entities.BengalTiger, TR2Entities.TRex
            },
            [TR2LevelNames.VENICE] = new List<TR2Entities>
            {
                TR2Entities.Boat, TR2Entities.Doberman, TR2Entities.MaskedGoon1, TR2Entities.MaskedGoon2, TR2Entities.MaskedGoon3, TR2Entities.Rat, TR2Entities.StickWieldingGoon1BodyWarmer
            },
            [TR2LevelNames.BARTOLI] = new List<TR2Entities>
            {
                TR2Entities.StickWieldingGoon1WhiteVest
            },
            [TR2LevelNames.OPERA] = new List<TR2Entities>
            {
                TR2Entities.ShotgunGoon
            },
            [TR2LevelNames.RIG] = new List<TR2Entities>
            {
                TR2Entities.Gunman1OG, TR2Entities.Gunman2, TR2Entities.ScubaDiver, TR2Entities.StickWieldingGoon1Bandana
            },
            [TR2LevelNames.DA] = new List<TR2Entities>
            {
                TR2Entities.FlamethrowerGoonOG
            },
            [TR2LevelNames.FATHOMS] = new List<TR2Entities>
            {
                TR2Entities.LaraUnwater, TR2Entities.BarracudaUnwater, TR2Entities.Shark
            },
            [TR2LevelNames.DORIA] = new List<TR2Entities>
            {
                TR2Entities.StickWieldingGoon1GreenVest, TR2Entities.YellowMorayEel
            },
            [TR2LevelNames.LQ] = new List<TR2Entities>
            {
                TR2Entities.BlackMorayEel, TR2Entities.StickWieldingGoon2
            },
            [TR2LevelNames.TIBET] = new List<TR2Entities>
            {
                TR2Entities.LaraSnow, TR2Entities.Eagle, TR2Entities.Mercenary2, TR2Entities.Mercenary3, TR2Entities.MercSnowmobDriver, TR2Entities.SnowLeopard
            },
            [TR2LevelNames.MONASTERY] = new List<TR2Entities>
            {
                TR2Entities.Mercenary1, TR2Entities.MonkWithKnifeStick, TR2Entities.MonkWithLongStick
            },
            [TR2LevelNames.COT] = new List<TR2Entities>
            {
                TR2Entities.BarracudaIce, TR2Entities.Yeti
            },
            [TR2LevelNames.CHICKEN] = new List<TR2Entities>
            {
                TR2Entities.BirdMonster, TR2Entities.WhiteTiger
            },
            [TR2LevelNames.XIAN] = new List<TR2Entities>
            {
                TR2Entities.BarracudaXian, TR2Entities.GiantSpider
            },
            [TR2LevelNames.FLOATER] = new List<TR2Entities>
            {
                TR2Entities.Knifethrower, TR2Entities.XianGuardSword, TR2Entities.XianGuardSpear
            },
            [TR2LevelNames.LAIR] = new List<TR2Entities>
            {
                TR2Entities.MarcoBartoli
            },
            [TR2LevelNames.HOME] = new List<TR2Entities>
            {
                TR2Entities.LaraHome, TR2Entities.StickWieldingGoon1BlackJacket
            },
            [TR2LevelNames.ASSAULT] = new List<TR2Entities>
            {
                TR2Entities.Winston
            },
            ["TOPIORC.TR2"] = new List<TR2Entities>
            {
                TR2Entities.FlamethrowerGoonTopixtor, TR2Entities.Gunman1TopixtorORC
            },
            ["TOPICAC.TR2"] = new List<TR2Entities>
            {
                TR2Entities.Gunman1TopixtorCAC
            }
        };
    }
}