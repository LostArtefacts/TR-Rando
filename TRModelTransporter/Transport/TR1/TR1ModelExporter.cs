using System;
using System.Collections.Generic;
using System.Linq;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Data;
using TRModelTransporter.Handlers;
using TRModelTransporter.Handlers.Textures;
using TRModelTransporter.Model.Definitions;

namespace TRModelTransporter.Transport
{
    public class TR1ModelExporter : AbstractTRModelExporter<TREntities, TRLevel, TR1ModelDefinition>
    {
        public TR1ModelExporter()
        {
            Data = new TR1DefaultDataProvider();
        }

        protected override AbstractTextureExportHandler<TREntities, TRLevel, TR1ModelDefinition> CreateTextureHandler()
        {
            return new TR1TextureExportHandler();
        }

        protected override TR1ModelDefinition CreateModelDefinition(TRLevel level, TREntities modelEntity)
        {
            TR1ModelDefinition definition = new TR1ModelDefinition
            {
                Alias = modelEntity
            };

            if (Data.IsAlias(modelEntity))
            {
                modelEntity = Data.TranslateAlias(modelEntity);
            }

            _modelHandler.Export(level, definition, modelEntity);
            _meshHandler.Export(level, definition);
            _colourHandler.Export(level, definition);
            _textureHandler.Export(level, definition, TextureClassifier, Data.GetSpriteDependencies(modelEntity), Data.GetIgnorableTextureIndices(modelEntity, LevelName));
            _animationHandler.Export(level, definition);
            _cinematicHandler.Export(level, definition, Data.GetCinematicEntities());
            _soundHandler.Export(level, definition, Data.GetHardcodedSounds(definition.Alias));

            return definition;
        }

        protected override void PreDefinitionCreation(TRLevel level, TREntities modelEntity)
        {
            switch (modelEntity)
            {
                case TREntities.Pierre:
                    AmendPierreGunshot(level);
                    AmendPierreDeath(level);
                    break;
                case TREntities.Larson:
                    AmendLarsonDeath(level);
                    break;
                case TREntities.SkateboardKid:
                    AmendSkaterBoyDeath(level);
                    break;
                case TREntities.Natla:
                    AmendNatlaDeath(level);
                    break;
            }
        }

        protected override void ModelExportReady(TR1ModelDefinition definition)
        {
            switch (definition.Entity)
            {
                case TREntities.Kold:
                    if (definition.Colours.ContainsKey(123))
                    {
                        // Incorrect orange colouring on head and hands
                        definition.Colours[123].Red = 28;
                        definition.Colours[123].Green = 18;
                        definition.Colours[123].Blue = 4;
                    }
                    break;
                case TREntities.SkateboardKid:
                    if (definition.Colours.ContainsKey(182))
                    {
                        // Incorrect yellow colouring on his arm
                        definition.Colours[182].Red = 51;
                        definition.Colours[182].Green = 33;
                        definition.Colours[182].Blue = 22;
                    }
                    break;
                default:
                    break;
            }
        }

        public static void AmendPierreGunshot(TRLevel level)
        {
            TRModel model = Array.Find(level.Models, m => m.ID == (uint)TREntities.Pierre);
            // Get his shooting animation
            TRAnimation anim = level.Animations[model.Animation + 10];
            List<TRAnimCommand> cmds = level.AnimCommands.ToList();
            anim.AnimCommand = (ushort)cmds.Count;
            anim.NumAnimCommands = 1;

            // On the 2nd frame, play SFX 44 (magnums)
            cmds.Add(new TRAnimCommand { Value = 5 });
            cmds.Add(new TRAnimCommand { Value = (short)(anim.FrameStart + 1) });
            cmds.Add(new TRAnimCommand { Value = 44 });

            level.AnimCommands = cmds.ToArray();
            level.NumAnimCommands = (uint)cmds.Count;
        }

        public static void AmendPierreDeath(TRLevel level)
        {
            TRModel model = Array.Find(level.Models, m => m.ID == (uint)TREntities.Pierre);
            // Get his death animation
            TRAnimation anim = level.Animations[model.Animation + 12];
            anim.NumAnimCommands++;

            List<TRAnimCommand> cmds = level.AnimCommands.ToList();
            anim.AnimCommand = (ushort)cmds.Count;
            cmds.Add(new TRAnimCommand { Value = 4 }); // Death

            // On the 61st frame, play SFX 159 (death)
            cmds.Add(new TRAnimCommand { Value = 5 });
            cmds.Add(new TRAnimCommand { Value = (short)(anim.FrameStart + 60) });
            cmds.Add(new TRAnimCommand { Value = 159 });

            level.AnimCommands = cmds.ToArray();
            level.NumAnimCommands = (uint)cmds.Count;
        }

        public static void AmendLarsonDeath(TRLevel level)
        {
            TRModel model = Array.Find(level.Models, m => m.ID == (uint)TREntities.Larson);
            // Get his death animation
            TRAnimation anim = level.Animations[model.Animation + 15];
            anim.NumAnimCommands++;

            List<TRAnimCommand> cmds = level.AnimCommands.ToList();
            anim.AnimCommand = (ushort)cmds.Count;
            cmds.Add(new TRAnimCommand { Value = 4 }); // Death

            // On the 2nd frame, play SFX 158 (death)
            cmds.Add(new TRAnimCommand { Value = 5 });
            cmds.Add(new TRAnimCommand { Value = (short)(anim.FrameStart + 1) });
            cmds.Add(new TRAnimCommand { Value = 158 });

            level.AnimCommands = cmds.ToArray();
            level.NumAnimCommands = (uint)cmds.Count;
        }

        public static void AmendSkaterBoyDeath(TRLevel level)
        {
            TRModel model = Array.Find(level.Models, m => m.ID == (uint)TREntities.SkateboardKid);
            // Get his death animation
            TRAnimation anim = level.Animations[model.Animation + 13];
            // Play the death sound on the 2nd frame (doesn't work on the 1st, which is OG).
            level.AnimCommands[anim.AnimCommand + 2].Value++;
        }

        public static void AmendNatlaDeath(TRLevel level)
        {
            TRModel model = Array.Find(level.Models, m => m.ID == (uint)TREntities.Natla);
            // Get her death animation
            TRAnimation anim = level.Animations[model.Animation + 13];
            anim.NumAnimCommands++;

            List<TRAnimCommand> cmds = level.AnimCommands.ToList();
            anim.AnimCommand = (ushort)cmds.Count;
            cmds.Add(new TRAnimCommand { Value = 4 }); // Death

            // On the 5th frame, play SFX 160 (death)
            cmds.Add(new TRAnimCommand { Value = 5 });
            cmds.Add(new TRAnimCommand { Value = (short)(anim.FrameStart + 4) });
            cmds.Add(new TRAnimCommand { Value = 160 });

            level.AnimCommands = cmds.ToArray();
            level.NumAnimCommands = (uint)cmds.Count;
        }
    }
}