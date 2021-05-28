using System;
using System.Collections.Generic;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Helpers;
using TRModelTransporter.Model;

namespace TRModelTransporter.Handlers
{
    public class SoundTransportHandler : AbstractTransportHandler
    {
        private static readonly Dictionary<TR2Entities, int[]> _hardcodedSoundIndices = new Dictionary<TR2Entities, int[]>
        {
            [TR2Entities.DragonExplosionEmitter_N] = new int[]
            {
                341  // Explosion when dragon spawns
            },
            [TR2Entities.DragonFront_H] = new int[]
            {
                298, // Footstep
                299, // Growl 1
                300, // Growl 2
                301, // Body falling
                302, // Dying breath
                303, // Growl 3
                304, // Grunt
                305, // Fire-breathing
                306, // Leg lift
                307  // Leg hit
            },
            [TR2Entities.LaraSnowmobAnim_H] = new int[]
            {
                153, // Snowmobile idling
                155  // Snowmobile accelerating
            },
            [TR2Entities.StickWieldingGoon1Bandana] = new int[]
            {
                71,  // Thump 1
                72,  // Thump 2
            },
            [TR2Entities.StickWieldingGoon1BlackJacket] = new int[]
            {
                71,  // Thump 1
                72,  // Thump 2
            },
            [TR2Entities.StickWieldingGoon1BodyWarmer] = new int[]
            {
                69,  // Footstep
                70,  // Grunt
                71,  // Thump 1
                72,  // Thump 2
                121  // Thump 3
            },
            [TR2Entities.StickWieldingGoon1GreenVest] = new int[]
            {
                71,  // Thump 1
                72,  // Thump 2
            },
            [TR2Entities.StickWieldingGoon1WhiteVest] = new int[]
            {
                71,  // Thump 1
                72,  // Thump 2,
                121  // Thump 3
            },
            [TR2Entities.StickWieldingGoon2] = new int[]
            {
                69,  // Footstep
                70,  // Grunt
                71,  // Thump 1
                72,  // Thump 2
                180, // Chains
                181, // Chains
                182, // Footstep?
                183  // Another thump?
            },
            [TR2Entities.XianGuardSword] = new int[]
            {
                312  // Hovering
            }
        };

        // Importing is done en-masse to ensure hard-coded entries are allocated prior to anything else
        public IEnumerable<TRModelDefinition> Definitions { get; set; }

        private readonly SoundUnpacker _soundUnpacker;

        public SoundTransportHandler()
        {
            _soundUnpacker = new SoundUnpacker();
        }

        public override void Export()
        {
            if (_hardcodedSoundIndices.ContainsKey(Definition.Alias))
            {
                Definition.HardcodedSound = new PackedSound();
                ExportHardcodedSounds(_hardcodedSoundIndices[Definition.Alias]);
            }
        }

        private void ExportHardcodedSounds(IEnumerable<int> indices)
        {
            short id = -2;
            foreach (int index in indices)
            {
                Definition.HardcodedSound.SoundMapIndices[index] = id--;
            }

            int offset = 2;
            foreach (short index in Definition.HardcodedSound.SoundMapIndices.Keys)
            {
                short val = Definition.HardcodedSound.SoundMapIndices[index];
                TRSoundDetails details = Level.SoundDetails[Level.SoundMap[index]];
                Definition.HardcodedSound.SampleIndices[(ushort)(ushort.MaxValue - offset)] = new uint[] { Level.SampleIndices[details.Sample] };

                Definition.HardcodedSound.SoundDetails[val] = new TRSoundDetails
                {
                    Chance = details.Chance,
                    Characteristics = details.Characteristics,
                    Sample = (ushort)(ushort.MaxValue - offset),
                    Volume = details.Volume
                };
                offset++;
            }
        }

        public override void Import()
        {
            if (Definitions == null)
            {
                throw new Exception();
            }

            foreach (TRModelDefinition definition in Definitions)
            {
                if (definition.HardcodedSound != null)
                {
                    _soundUnpacker.Unpack(definition.HardcodedSound, Level, true);
                }
            }
        }
    }
}