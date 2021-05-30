using Newtonsoft.Json;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Handlers;
using TRModelTransporter.Model;

namespace TRModelTransporter.Transport
{
    public abstract class AbstractTRModelTransport
    {
        protected static readonly Dictionary<TR2Entities, TR2Entities[]> _entityDependencies = new Dictionary<TR2Entities, TR2Entities[]>
        {
            [TR2Entities.TRex] =
                new TR2Entities[] { TR2Entities.LaraMiscAnim_H_Wall },
            [TR2Entities.MaskedGoon2] =
                new TR2Entities[] { TR2Entities.MaskedGoon1 },
            [TR2Entities.MaskedGoon3] =
                new TR2Entities[] { TR2Entities.MaskedGoon1 },
            [TR2Entities.ScubaDiver] =
                new TR2Entities[] { TR2Entities.ScubaHarpoonProjectile_H },
            [TR2Entities.Shark] =
                new TR2Entities[] { TR2Entities.LaraMiscAnim_H_Unwater },
            [TR2Entities.StickWieldingGoon2] =
                new TR2Entities[] { TR2Entities.StickWieldingGoon1GreenVest },
            [TR2Entities.MercSnowmobDriver] =
                new TR2Entities[] { TR2Entities.BlackSnowmob },
            [TR2Entities.BlackSnowmob] =
                new TR2Entities[] { TR2Entities.RedSnowmobile },
            [TR2Entities.RedSnowmobile] =
                new TR2Entities[] { TR2Entities.SnowmobileBelt, TR2Entities.LaraSnowmobAnim_H },
            [TR2Entities.Mercenary3] =
                new TR2Entities[] { TR2Entities.Mercenary2 },
            [TR2Entities.Yeti] =
                new TR2Entities[] { TR2Entities.LaraMiscAnim_H_Ice },
            [TR2Entities.XianGuardSpear] =
                new TR2Entities[] { TR2Entities.LaraMiscAnim_H_Xian, TR2Entities.XianGuardSpearStatue },
            [TR2Entities.XianGuardSword] =
                new TR2Entities[] { TR2Entities.XianGuardSwordStatue },
            [TR2Entities.Knifethrower] =
                new TR2Entities[] { TR2Entities.KnifeProjectile_H },
            [TR2Entities.MarcoBartoli] =
                new TR2Entities[]
                {
                    TR2Entities.DragonExplosionEmitter_N, TR2Entities.DragonExplosion1_H, TR2Entities.DragonExplosion2_H, TR2Entities.DragonExplosion3_H,
                    TR2Entities.DragonFront_H, TR2Entities.DragonBack_H, TR2Entities.DragonBonesFront_H, TR2Entities.DragonBonesBack_H, TR2Entities.LaraMiscAnim_H_Xian
                }
        };

        // If these are imported as dependencies only, their textures will be skipped
        protected static readonly List<TR2Entities> _noGraphicsEntityDependencies = new List<TR2Entities>
        {
            TR2Entities.StickWieldingGoon1GreenVest, TR2Entities.MaskedGoon1, TR2Entities.Mercenary2
        };

        protected static readonly Dictionary<TR2Entities, TR2Entities> _entityAliases = new Dictionary<TR2Entities, TR2Entities>
        {
            [TR2Entities.BengalTiger] = TR2Entities.TigerOrSnowLeopard,
            [TR2Entities.SnowLeopard] = TR2Entities.TigerOrSnowLeopard,
            [TR2Entities.WhiteTiger] = TR2Entities.TigerOrSnowLeopard,
            [TR2Entities.StickWieldingGoon1Bandana] = TR2Entities.StickWieldingGoon1,
            [TR2Entities.StickWieldingGoon1BlackJacket] = TR2Entities.StickWieldingGoon1,
            [TR2Entities.StickWieldingGoon1BodyWarmer] = TR2Entities.StickWieldingGoon1,
            [TR2Entities.StickWieldingGoon1GreenVest] = TR2Entities.StickWieldingGoon1,
            [TR2Entities.StickWieldingGoon1WhiteVest] = TR2Entities.StickWieldingGoon1,
            [TR2Entities.LaraMiscAnim_H_Ice] = TR2Entities.LaraMiscAnim_H,
            [TR2Entities.LaraMiscAnim_H_Unwater] = TR2Entities.LaraMiscAnim_H,
            [TR2Entities.LaraMiscAnim_H_Xian] = TR2Entities.LaraMiscAnim_H,
            [TR2Entities.LaraMiscAnim_H_Wall] = TR2Entities.LaraMiscAnim_H,
            [TR2Entities.BarracudaIce] = TR2Entities.Barracuda,
            [TR2Entities.BarracudaUnwater] = TR2Entities.Barracuda,
            [TR2Entities.BarracudaXian] = TR2Entities.Barracuda
        };

        protected static readonly Dictionary<TR2Entities, List<TR2Entities>> _aliasMap = new Dictionary<TR2Entities, List<TR2Entities>>
        {
            [TR2Entities.TigerOrSnowLeopard] = new List<TR2Entities>
            {
                TR2Entities.BengalTiger, TR2Entities.SnowLeopard, TR2Entities.WhiteTiger
            },
            [TR2Entities.StickWieldingGoon1] = new List<TR2Entities>
            {
                TR2Entities.StickWieldingGoon1Bandana, TR2Entities.StickWieldingGoon1BlackJacket,
                TR2Entities.StickWieldingGoon1BodyWarmer, TR2Entities.StickWieldingGoon1GreenVest,
                TR2Entities.StickWieldingGoon1WhiteVest
            },
            [TR2Entities.Barracuda] = new List<TR2Entities>
            {
                TR2Entities.BarracudaIce, TR2Entities.BarracudaUnwater, TR2Entities.BarracudaXian
            },
            [TR2Entities.LaraMiscAnim_H] = new List<TR2Entities>
            {
                TR2Entities.LaraMiscAnim_H_Ice, TR2Entities.LaraMiscAnim_H_Unwater, TR2Entities.LaraMiscAnim_H_Xian, TR2Entities.LaraMiscAnim_H_Wall
            }
        };

        protected static readonly List<TR2Entities> _permittedAliasDuplicates = new List<TR2Entities>
        {
            TR2Entities.LaraMiscAnim_H
        };

        protected static readonly string _defaultDataFolder = @"Resources\Models";
        protected static readonly string _dataFileName = "Data.json";
        protected static readonly string _imageFileName = "Segments.png";

        protected readonly AnimationTransportHandler _animationHandler;
        protected readonly CinematicTransportHandler _cinematicHandler;
        protected readonly ColourTransportHandler _colourHandler;
        protected readonly MeshTransportHandler _meshHandler;
        protected readonly ModelTransportHandler _modelHandler;
        protected readonly SoundTransportHandler _soundHandler;
        protected readonly TextureTransportHandler _textureHandler;

        protected TR2Level _level;
        protected TRModelDefinition _definition;

        public TR2Level Level
        {
            get => _level;
            set
            {
                _level = value;
                _animationHandler.Level = _level;
                _cinematicHandler.Level = _level;
                _colourHandler.Level = _level;
                _meshHandler.Level = _level;
                _modelHandler.Level = _level;
                _soundHandler.Level = _level;
                _textureHandler.Level = _level;
            }
        }

        public string LevelName { get; set; }

        public TRModelDefinition Definition
        {
            get => _definition;
            protected set
            {
                _definition = value;
                _animationHandler.Definition = _definition;
                _cinematicHandler.Definition = _definition;
                _colourHandler.Definition = _definition;
                _meshHandler.Definition = _definition;
                _modelHandler.Definition = _definition;
                _soundHandler.Definition = _definition;
                _textureHandler.Definition = _definition;
            }
        }

        public string DataFolder { get; set; }

        public AbstractTRModelTransport()
        {
            _animationHandler = new AnimationTransportHandler();
            _cinematicHandler = new CinematicTransportHandler();
            _colourHandler = new ColourTransportHandler();
            _meshHandler = new MeshTransportHandler();
            _modelHandler = new ModelTransportHandler();
            _soundHandler = new SoundTransportHandler();
            _textureHandler = new TextureTransportHandler();

            DataFolder = _defaultDataFolder;
        }

        protected void StoreDefinition(TRModelDefinition definition)
        {
            string directory = Path.Combine(DataFolder, definition.Alias.ToString());
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (definition.HasGraphics)
            {
                definition.Bitmap.Save(Path.Combine(directory, "Segments.png"), ImageFormat.Png);
            }
            File.WriteAllText(Path.Combine(directory, "Data.json"), JsonConvert.SerializeObject(definition, Formatting.None));
        }

        public TRModelDefinition LoadDefinition(TR2Entities modelEntity)
        {
            string directory = Path.Combine(DataFolder, modelEntity.ToString());
            string dataFilePath = Path.Combine(directory, _dataFileName);
            string imageFilePath = Path.Combine(directory, _imageFileName);

            if (!File.Exists(dataFilePath))
            {
                throw new IOException(string.Format("Missing model data JSON file ({0})", dataFilePath));
            }

            TRModelDefinition definition = JsonConvert.DeserializeObject<TRModelDefinition>(File.ReadAllText(dataFilePath));
            definition.Alias = modelEntity;

            if (definition.HasGraphics)
            {
                if (!File.Exists(imageFilePath))
                {
                    throw new IOException(string.Format("Missing model data image file ({0})", imageFilePath));
                }
                definition.Bitmap = new Bitmap(imageFilePath);
            }
            return definition;
        }
    }
}