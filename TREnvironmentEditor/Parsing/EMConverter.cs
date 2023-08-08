using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TREnvironmentEditor.Model;
using TREnvironmentEditor.Model.Conditions;
using TREnvironmentEditor.Model.Types;

namespace TREnvironmentEditor.Parsing;

public class EMConverter : JsonConverter
{
    private static readonly string _emTypeName = "EMType";
    private static readonly string _conditionTypeName = "ConditionType";

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(BaseEMFunction) || objectType == typeof(BaseEMCondition);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);
        if (jo[_emTypeName] != null)
        {
            return ReadEMType(jo);
        }
        else if (jo[_conditionTypeName] != null)
        {
            return ReadConditionType(jo);
        }

        return null;
    }

    private object ReadEMType(JObject jo)
    {
        EMType type = (EMType)jo[_emTypeName].Value<int>();
        return type switch
        {
            // Surface types
            EMType.Ladder => JsonConvert.DeserializeObject<EMLadderFunction>(jo.ToString(), this),
            EMType.Floor => JsonConvert.DeserializeObject<EMFloorFunction>(jo.ToString(), this),
            EMType.Flood => JsonConvert.DeserializeObject<EMFloodFunction>(jo.ToString(), this),
            EMType.Drain => JsonConvert.DeserializeObject<EMDrainFunction>(jo.ToString(), this),
            EMType.Ceiling => JsonConvert.DeserializeObject<EMCeilingFunction>(jo.ToString(), this),
            EMType.Click => JsonConvert.DeserializeObject<EMClickFunction>(jo.ToString(), this),
            EMType.Slant => JsonConvert.DeserializeObject<EMSlantFunction>(jo.ToString(), this),

            // Texture types
            EMType.Reface => JsonConvert.DeserializeObject<EMRefaceFunction>(jo.ToString(), this),
            EMType.RemoveFace => JsonConvert.DeserializeObject<EMRemoveFaceFunction>(jo.ToString(), this),
            EMType.ModifyFace => JsonConvert.DeserializeObject<EMModifyFaceFunction>(jo.ToString(), this),
            EMType.AddStaticMesh => JsonConvert.DeserializeObject<EMAddStaticMeshFunction>(jo.ToString(), this),
            EMType.RemoveStaticMesh => JsonConvert.DeserializeObject<EMRemoveStaticMeshFunction>(jo.ToString(), this),
            EMType.AddFace => JsonConvert.DeserializeObject<EMAddFaceFunction>(jo.ToString(), this),
            EMType.MirrorStaticMesh => JsonConvert.DeserializeObject<EMMirrorStaticMeshFunction>(jo.ToString(), this),
            EMType.MirrorObjectTexture => JsonConvert.DeserializeObject<EMMirrorObjectTexture>(jo.ToString(), this),
            EMType.OverwriteTexture => JsonConvert.DeserializeObject<EMOverwriteTextureFunction>(jo.ToString(), this),
            EMType.MoveStaticMesh => JsonConvert.DeserializeObject<EMMoveStaticMeshFunction>(jo.ToString(), this),
            EMType.AddRoomSprite => JsonConvert.DeserializeObject<EMAddRoomSpriteFunction>(jo.ToString(), this),
            EMType.SwapFace => JsonConvert.DeserializeObject<EMSwapFaceFunction>(jo.ToString(), this),
            EMType.ImportTexture => JsonConvert.DeserializeObject<EMImportTextureFunction>(jo.ToString(), this),

            // Entity types
            EMType.MoveSlot => JsonConvert.DeserializeObject<EMMoveSlotFunction>(jo.ToString(), this),
            EMType.MoveEnemy => JsonConvert.DeserializeObject<EMMoveEnemyFunction>(jo.ToString(), this),
            EMType.MovePickup => JsonConvert.DeserializeObject<EMMovePickupFunction>(jo.ToString(), this),
            EMType.MoveEntity => JsonConvert.DeserializeObject<EMMoveEntityFunction>(jo.ToString(), this),
            EMType.ConvertEntity => JsonConvert.DeserializeObject<EMConvertEntityFunction>(jo.ToString(), this),
            EMType.MoveTrap => JsonConvert.DeserializeObject<EMMoveTrapFunction>(jo.ToString(), this),
            EMType.ConvertEnemy => JsonConvert.DeserializeObject<EMConvertEnemyFunction>(jo.ToString(), this),
            EMType.ModifyEntity => JsonConvert.DeserializeObject<EMModifyEntityFunction>(jo.ToString(), this),
            EMType.SwapSlot => JsonConvert.DeserializeObject<EMSwapSlotFunction>(jo.ToString(), this),
            EMType.AdjustEntityPositions => JsonConvert.DeserializeObject<EMAdjustEntityPositionFunction>(jo.ToString(), this),
            EMType.AddEntity => JsonConvert.DeserializeObject<EMAddEntityFunction>(jo.ToString(), this),
            EMType.ConvertWheelDoor => JsonConvert.DeserializeObject<EMConvertWheelDoorFunction>(jo.ToString(), this),
            EMType.MoveSecret => JsonConvert.DeserializeObject<EMMoveSecretFunction>(jo.ToString(), this),
            EMType.SwapGroupedSlots => JsonConvert.DeserializeObject<EMSwapGroupedSlotsFunction>(jo.ToString(), this),
            EMType.AddDoppelganger => JsonConvert.DeserializeObject<EMAddDoppelgangerFunction>(jo.ToString(), this),

            // Trigger types
            EMType.Trigger => JsonConvert.DeserializeObject<EMTriggerFunction>(jo.ToString(), this),
            EMType.RemoveTrigger => JsonConvert.DeserializeObject<EMRemoveTriggerFunction>(jo.ToString(), this),
            EMType.DuplicateTrigger => JsonConvert.DeserializeObject<EMDuplicateTriggerFunction>(jo.ToString(), this),
            EMType.DuplicateSwitchTrigger => JsonConvert.DeserializeObject<EMDuplicateSwitchTriggerFunction>(jo.ToString(), this),
            EMType.CameraTriggerFunction => JsonConvert.DeserializeObject<EMCameraTriggerFunction>(jo.ToString(), this),
            EMType.ReplaceTriggerActionParameterFunction => JsonConvert.DeserializeObject<EMReplaceTriggerActionParameterFunction>(jo.ToString(), this),
            EMType.MoveTrigger => JsonConvert.DeserializeObject<EMMoveTriggerFunction>(jo.ToString(), this),
            EMType.AppendTriggerActionFunction => JsonConvert.DeserializeObject<EMAppendTriggerActionFunction>(jo.ToString(), this),
            EMType.ConvertTrigger => JsonConvert.DeserializeObject<EMConvertTriggerFunction>(jo.ToString(), this),
            EMType.KillLara => JsonConvert.DeserializeObject<EMKillLaraFunction>(jo.ToString(), this),
            EMType.RemoveTriggerAction => JsonConvert.DeserializeObject<EMRemoveTriggerActionFunction>(jo.ToString(), this),
            EMType.RemoveEntityTriggers => JsonConvert.DeserializeObject<EMRemoveEntityTriggersFunction>(jo.ToString(), this),

            // Portals
            EMType.VisibilityPortal => JsonConvert.DeserializeObject<EMVisibilityPortalFunction>(jo.ToString(), this),
            EMType.HorizontalCollisionalPortal => JsonConvert.DeserializeObject<EMHorizontalCollisionalPortalFunction>(jo.ToString(), this),
            EMType.VerticalCollisionalPortal => JsonConvert.DeserializeObject<EMVerticalCollisionalPortalFunction>(jo.ToString(), this),
            EMType.AdjustVisibilityPortal => JsonConvert.DeserializeObject<EMAdjustVisibilityPortalFunction>(jo.ToString(), this),
            EMType.ReplaceCollisionalPortal => JsonConvert.DeserializeObject<EMReplaceCollisionalPortalFunction>(jo.ToString(), this),
            EMType.RemoveCollisionalPortal => JsonConvert.DeserializeObject<EMRemoveCollisionalPortalFunction>(jo.ToString(), this),

            // Sounds
            EMType.AddSoundSource => JsonConvert.DeserializeObject<EMAddSoundSourceFunction>(jo.ToString(), this),
            EMType.MoveSoundSource => JsonConvert.DeserializeObject<EMMoveSoundSourceFunction>(jo.ToString(), this),
            EMType.RemoveSoundSource => JsonConvert.DeserializeObject<EMRemoveSoundSourceFunction>(jo.ToString(), this),

            // Rooms
            EMType.ModifyRoom => JsonConvert.DeserializeObject<EMModifyRoomFunction>(jo.ToString(), this),
            EMType.ModifyOverlaps => JsonConvert.DeserializeObject<EMModifyOverlapsFunction>(jo.ToString(), this),
            EMType.CopyRoom => JsonConvert.DeserializeObject<EMCopyRoomFunction>(jo.ToString(), this),
            EMType.CopyVertexAttributes => JsonConvert.DeserializeObject<EMCopyVertexAttributesFunction>(jo.ToString(), this),
            EMType.ImportRoom => JsonConvert.DeserializeObject<EMImportRoomFunction>(jo.ToString(), this),
            EMType.CreateRoom => JsonConvert.DeserializeObject<EMCreateRoomFunction>(jo.ToString(), this),
            EMType.CreateWall => JsonConvert.DeserializeObject<EMCreateWallFunction>(jo.ToString(), this),
            EMType.GenerateLight => JsonConvert.DeserializeObject<EMGenerateLightFunction>(jo.ToString(), this),
            EMType.MoveCamera => JsonConvert.DeserializeObject<EMMoveCameraFunction>(jo.ToString(), this),

            // Models
            EMType.ImportModel => JsonConvert.DeserializeObject<EMImportModelFunction>(jo.ToString(), this),
            EMType.MirrorModel => JsonConvert.DeserializeObject<EMMirrorModelFunction>(jo.ToString(), this),
            EMType.ConvertSpriteSequence => JsonConvert.DeserializeObject<EMConvertSpriteSequenceFunction>(jo.ToString(), this),
            EMType.ConvertModel => JsonConvert.DeserializeObject<EMConvertModelFunction>(jo.ToString(), this),
            EMType.ImportNonGraphicsModel => JsonConvert.DeserializeObject<EMImportNonGraphicsModelFunction>(jo.ToString(), this),
            EMType.CopySpriteSequence => JsonConvert.DeserializeObject<EMCopySpriteSequenceFunction>(jo.ToString(), this),

            // NOOP
            EMType.NOOP => JsonConvert.DeserializeObject<EMPlaceholderFunction>(jo.ToString(), this),

            _ => throw new InvalidOperationException(),
        };
    }

    private object ReadConditionType(JObject jo)
    {
        EMConditionType type = (EMConditionType)jo[_conditionTypeName].Value<int>();
        return type switch
        {
            // Entities
            EMConditionType.EntityProperty => JsonConvert.DeserializeObject<EMEntityPropertyCondition>(jo.ToString(), this),
            EMConditionType.SecretInRoom => JsonConvert.DeserializeObject<EMSecretRoomCondition>(jo.ToString(), this),

            // Rooms
            EMConditionType.RoomContainsWater => JsonConvert.DeserializeObject<EMRoomContainsWaterCondition>(jo.ToString(), this),
            EMConditionType.SectorContainsSecret => JsonConvert.DeserializeObject<EMSectorContainsSecretCondition>(jo.ToString(), this),
            EMConditionType.SectorIsWall => JsonConvert.DeserializeObject<EMSectorIsWallCondition>(jo.ToString(), this),

            // Models
            EMConditionType.ModelExists => JsonConvert.DeserializeObject<EMModelExistsCondition>(jo.ToString(), this),
            EMConditionType.UnconditionalBirds => JsonConvert.DeserializeObject<EMUnconditionalBirdCheck>(jo.ToString(), this),

            _ => throw new InvalidOperationException(),
        };
    }

    public override bool CanWrite => false;

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) { }
}
