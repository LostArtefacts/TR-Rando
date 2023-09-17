using TRLevelControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRModelTransporter.Data;
using TRModelTransporter.Handlers;
using TRModelTransporter.Handlers.Textures;
using TRModelTransporter.Model.Definitions;

namespace TRModelTransporter.Transport;

public class TR1ModelExporter : AbstractTRModelExporter<TR1Type, TR1Level, TR1ModelDefinition>
{
    public TR1ModelExporter()
    {
        Data = new TR1DefaultDataProvider();
    }

    protected override AbstractTextureExportHandler<TR1Type, TR1Level, TR1ModelDefinition> CreateTextureHandler()
    {
        return new TR1TextureExportHandler();
    }

    protected override TR1ModelDefinition CreateModelDefinition(TR1Level level, TR1Type modelEntity)
    {
        TR1ModelDefinition definition = new()
        {
            Alias = modelEntity
        };

        if (Data.IsAlias(modelEntity))
        {
            modelEntity = Data.TranslateAlias(modelEntity);
        }

        ModelTransportHandler.Export(level, definition, modelEntity);
        MeshTransportHandler.Export(level, definition);
        ColourTransportHandler.Export(level, definition);
        _textureHandler.Export(level, definition, TextureClassifier, Data.GetSpriteDependencies(modelEntity), Data.GetIgnorableTextureIndices(modelEntity, LevelName));
        AnimationTransportHandler.Export(level, definition);
        CinematicTransportHandler.Export(level, definition, Data.GetCinematicEntities());
        SoundTransportHandler.Export(level, definition, Data.GetHardcodedSounds(definition.Alias));

        return definition;
    }

    protected override void PreDefinitionCreation(TR1Level level, TR1Type modelEntity)
    {
        switch (modelEntity)
        {
            case TR1Type.Pierre:
                AmendPierreGunshot(level);
                AmendPierreDeath(level);
                break;
            case TR1Type.Larson:
                AmendLarsonDeath(level);
                break;
            case TR1Type.SkateboardKid:
                AmendSkaterBoyDeath(level);
                break;
            case TR1Type.Natla:
                AmendNatlaDeath(level);
                break;
            case TR1Type.MovingBlock:
                AddMovingBlockSFX(level);
                break;
        }
    }

    protected override void ModelExportReady(TR1ModelDefinition definition)
    {
        switch (definition.Alias)
        {
            case TR1Type.Kold:
                if (definition.Colours.ContainsKey(123))
                {
                    // Incorrect orange colouring on head and hands
                    definition.Colours[123].Red = 28;
                    definition.Colours[123].Green = 18;
                    definition.Colours[123].Blue = 4;
                }
                break;
            case TR1Type.SkateboardKid:
                if (definition.Colours.ContainsKey(182))
                {
                    // Incorrect yellow colouring on his arm
                    definition.Colours[182].Red = 51;
                    definition.Colours[182].Green = 33;
                    definition.Colours[182].Blue = 22;
                }
                break;
            case TR1Type.CowboyHeadless:
                AmendDXtre3DTextures(definition);
                break;
            default:
                break;
        }
    }

    public static void AmendPierreGunshot(TR1Level level)
    {
        TRModel model = Array.Find(level.Models, m => m.ID == (uint)TR1Type.Pierre);
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

    public static void AmendPierreDeath(TR1Level level)
    {
        TRModel model = Array.Find(level.Models, m => m.ID == (uint)TR1Type.Pierre);
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

    public static void AmendLarsonDeath(TR1Level level)
    {
        TRModel model = Array.Find(level.Models, m => m.ID == (uint)TR1Type.Larson);
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

    public static void AmendSkaterBoyDeath(TR1Level level)
    {
        TRModel model = Array.Find(level.Models, m => m.ID == (uint)TR1Type.SkateboardKid);
        // Get his death animation
        TRAnimation anim = level.Animations[model.Animation + 13];
        // Play the death sound on the 2nd frame (doesn't work on the 1st, which is OG).
        level.AnimCommands[anim.AnimCommand + 2].Value++;
    }

    public static void AmendNatlaDeath(TR1Level level)
    {
        TRModel model = Array.Find(level.Models, m => m.ID == (uint)TR1Type.Natla);
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

    public static void AddMovingBlockSFX(TR1Level level)
    {
        // ToQ moving blocks are silent but we want them to scrape along the floor when they move.
        // Import the trapdoor closing SFX from Vilcabamba and adjust the animations accordingly.

        if (level.SoundMap[162] == -1)
        {
            TR1Level vilcabamba = new TR1LevelControl().Read(TR1LevelNames.VILCABAMBA);
            SoundUtilities.ImportLevelSound(level, vilcabamba, new short[] { 162 });
        }

        TRModel model = Array.Find(level.Models, m => m.ID == (uint)TR1Type.MovingBlock);
        List<TRAnimCommand> cmds = level.AnimCommands.ToList();
        for (int i = 2; i < 4; i++)
        {
            TRAnimation anim = level.Animations[model.Animation + i];
            anim.NumAnimCommands++;

            anim.AnimCommand = (ushort)cmds.Count;
            cmds.Add(new TRAnimCommand { Value = 4 }); // KillItem

            // On the 1st frame, play SFX 162
            cmds.Add(new TRAnimCommand { Value = 5 });
            cmds.Add(new TRAnimCommand { Value = (short)(anim.FrameStart) });
            cmds.Add(new TRAnimCommand { Value = 162 });
        }            

        level.AnimCommands = cmds.ToArray();
        level.NumAnimCommands = (uint)cmds.Count;
    }
}
