using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using TREnvironmentEditor.Model;
using TREnvironmentEditor.Model.Conditions;
using TREnvironmentEditor.Model.Types;

namespace TREnvironmentEditor.Parsing
{
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
            switch (type)
            {
                // Surface types
                case EMType.Ladder:
                    return JsonConvert.DeserializeObject<EMLadderFunction>(jo.ToString(), this);
                case EMType.Floor:
                    return JsonConvert.DeserializeObject<EMFloorFunction>(jo.ToString(), this);
                case EMType.Flood:
                    return JsonConvert.DeserializeObject<EMFloodFunction>(jo.ToString(), this);
                case EMType.Drain:
                    return JsonConvert.DeserializeObject<EMDrainFunction>(jo.ToString(), this);
                case EMType.Ceiling:
                    return JsonConvert.DeserializeObject<EMCeilingFunction>(jo.ToString(), this);
                case EMType.Click:
                    return JsonConvert.DeserializeObject<EMClickFunction>(jo.ToString(), this);
                case EMType.Slant:
                    return JsonConvert.DeserializeObject<EMSlantFunction>(jo.ToString(), this);

                // Texture types
                case EMType.Reface:
                    return JsonConvert.DeserializeObject<EMRefaceFunction>(jo.ToString(), this);
                case EMType.RemoveFace:
                    return JsonConvert.DeserializeObject<EMRemoveFaceFunction>(jo.ToString(), this);
                case EMType.ModifyFace:
                    return JsonConvert.DeserializeObject<EMModifyFaceFunction>(jo.ToString(), this);
                case EMType.AddStaticMesh:
                    return JsonConvert.DeserializeObject<EMAddStaticMeshFunction>(jo.ToString(), this);
                case EMType.RemoveStaticMesh:
                    return JsonConvert.DeserializeObject<EMRemoveStaticMeshFunction>(jo.ToString(), this);
                case EMType.AddFace:
                    return JsonConvert.DeserializeObject<EMAddFaceFunction>(jo.ToString(), this);
                case EMType.MirrorStaticMesh:
                    return JsonConvert.DeserializeObject<EMMirrorStaticMeshFunction>(jo.ToString(), this);
                case EMType.MirrorObjectTexture:
                    return JsonConvert.DeserializeObject<EMMirrorObjectTexture>(jo.ToString(), this);
                case EMType.OverwriteTexture:
                    return JsonConvert.DeserializeObject<EMOverwriteTextureFunction>(jo.ToString(), this);
                case EMType.MoveStaticMesh:
                    return JsonConvert.DeserializeObject<EMMoveStaticMeshFunction>(jo.ToString(), this);
                case EMType.AddRoomSprite:
                    return JsonConvert.DeserializeObject<EMAddRoomSpriteFunction>(jo.ToString(), this);
                case EMType.SwapFace:
                    return JsonConvert.DeserializeObject<EMSwapFaceFunction>(jo.ToString(), this);
                case EMType.ImportTexture:
                    return JsonConvert.DeserializeObject<EMImportTextureFunction>(jo.ToString(), this);

                // Entity types
                case EMType.MoveSlot:
                    return JsonConvert.DeserializeObject<EMMoveSlotFunction>(jo.ToString(), this);
                case EMType.MoveEnemy:
                    return JsonConvert.DeserializeObject<EMMoveEnemyFunction>(jo.ToString(), this);
                case EMType.MovePickup:
                    return JsonConvert.DeserializeObject<EMMovePickupFunction>(jo.ToString(), this);
                case EMType.MoveEntity:
                    return JsonConvert.DeserializeObject<EMMoveEntityFunction>(jo.ToString(), this);
                case EMType.ConvertEntity:
                    return JsonConvert.DeserializeObject<EMConvertEntityFunction>(jo.ToString(), this);
                case EMType.MoveTrap:
                    return JsonConvert.DeserializeObject<EMMoveTrapFunction>(jo.ToString(), this);
                case EMType.ConvertEnemy:
                    return JsonConvert.DeserializeObject<EMConvertEnemyFunction>(jo.ToString(), this);
                case EMType.ModifyEntity:
                    return JsonConvert.DeserializeObject<EMModifyEntityFunction>(jo.ToString(), this);
                case EMType.SwapSlot:
                    return JsonConvert.DeserializeObject<EMSwapSlotFunction>(jo.ToString(), this);
                case EMType.AdjustEntityPositions:
                    return JsonConvert.DeserializeObject<EMAdjustEntityPositionFunction>(jo.ToString(), this);
                case EMType.AddEntity:
                    return JsonConvert.DeserializeObject<EMAddEntityFunction>(jo.ToString(), this);
                case EMType.ConvertWheelDoor:
                    return JsonConvert.DeserializeObject<EMConvertWheelDoorFunction>(jo.ToString(), this);
                case EMType.MoveSecret:
                    return JsonConvert.DeserializeObject<EMMoveSecretFunction>(jo.ToString(), this);
                case EMType.SwapGroupedSlots:
                    return JsonConvert.DeserializeObject<EMSwapGroupedSlotsFunction>(jo.ToString(), this);

                // Trigger types
                case EMType.Trigger:
                    return JsonConvert.DeserializeObject<EMTriggerFunction>(jo.ToString(), this);
                case EMType.RemoveTrigger:
                    return JsonConvert.DeserializeObject<EMRemoveTriggerFunction>(jo.ToString(), this);
                case EMType.DuplicateTrigger:
                    return JsonConvert.DeserializeObject<EMDuplicateTriggerFunction>(jo.ToString(), this);
                case EMType.DuplicateSwitchTrigger:
                    return JsonConvert.DeserializeObject<EMDuplicateSwitchTriggerFunction>(jo.ToString(), this);
                case EMType.CameraTriggerFunction:
                    return JsonConvert.DeserializeObject<EMCameraTriggerFunction>(jo.ToString(), this);
                case EMType.ReplaceTriggerActionParameterFunction:
                    return JsonConvert.DeserializeObject<EMReplaceTriggerActionParameterFunction>(jo.ToString(), this);
                case EMType.MoveTrigger:
                    return JsonConvert.DeserializeObject<EMMoveTriggerFunction>(jo.ToString(), this);
                case EMType.AppendTriggerActionFunction:
                    return JsonConvert.DeserializeObject<EMAppendTriggerActionFunction>(jo.ToString(), this);
                case EMType.ConvertTrigger:
                    return JsonConvert.DeserializeObject<EMConvertTriggerFunction>(jo.ToString(), this);
                case EMType.KillLara:
                    return JsonConvert.DeserializeObject<EMKillLaraFunction>(jo.ToString(), this);
                case EMType.RemoveTriggerAction:
                    return JsonConvert.DeserializeObject<EMRemoveTriggerActionFunction>(jo.ToString(), this);

                // Portals
                case EMType.VisibilityPortal:
                    return JsonConvert.DeserializeObject<EMVisibilityPortalFunction>(jo.ToString(), this);
                case EMType.HorizontalCollisionalPortal:
                    return JsonConvert.DeserializeObject<EMHorizontalCollisionalPortalFunction>(jo.ToString(), this);
                case EMType.VerticalCollisionalPortal:
                    return JsonConvert.DeserializeObject<EMVerticalCollisionalPortalFunction>(jo.ToString(), this);
                case EMType.AdjustVisibilityPortal:
                    return JsonConvert.DeserializeObject<EMAdjustVisibilityPortalFunction>(jo.ToString(), this);
                case EMType.ReplaceCollisionalPortal:
                    return JsonConvert.DeserializeObject<EMReplaceCollisionalPortalFunction>(jo.ToString(), this);
                case EMType.RemoveCollisionalPortal:
                    return JsonConvert.DeserializeObject<EMRemoveCollisionalPortalFunction>(jo.ToString(), this);

                // Sounds
                case EMType.AddSoundSource:
                    return JsonConvert.DeserializeObject<EMAddSoundSourceFunction>(jo.ToString(), this);
                case EMType.MoveSoundSource:
                    return JsonConvert.DeserializeObject<EMMoveSoundSourceFunction>(jo.ToString(), this);
                case EMType.RemoveSoundSource:
                    return JsonConvert.DeserializeObject<EMRemoveSoundSourceFunction>(jo.ToString(), this);

                // Rooms
                case EMType.ModifyRoom:
                    return JsonConvert.DeserializeObject<EMModifyRoomFunction>(jo.ToString(), this);
                case EMType.ModifyOverlaps:
                    return JsonConvert.DeserializeObject<EMModifyOverlapsFunction>(jo.ToString(), this);
                case EMType.CopyRoom:
                    return JsonConvert.DeserializeObject<EMCopyRoomFunction>(jo.ToString(), this);
                case EMType.CopyVertexAttributes:
                    return JsonConvert.DeserializeObject<EMCopyVertexAttributesFunction>(jo.ToString(), this);
                case EMType.ImportRoom:
                    return JsonConvert.DeserializeObject<EMImportRoomFunction>(jo.ToString(), this);
                case EMType.CreateRoom:
                    return JsonConvert.DeserializeObject<EMCreateRoomFunction>(jo.ToString(), this);
                case EMType.CreateWall:
                    return JsonConvert.DeserializeObject<EMCreateWallFunction>(jo.ToString(), this);
                case EMType.GenerateLight:
                    return JsonConvert.DeserializeObject<EMGenerateLightFunction>(jo.ToString(), this);
                case EMType.MoveCamera:
                    return JsonConvert.DeserializeObject<EMMoveCameraFunction>(jo.ToString(), this);

                // Models
                case EMType.ImportModel:
                    return JsonConvert.DeserializeObject<EMImportModelFunction>(jo.ToString(), this);
                case EMType.MirrorModel:
                    return JsonConvert.DeserializeObject<EMMirrorModelFunction>(jo.ToString(), this);
                case EMType.ConvertSpriteSequence:
                    return JsonConvert.DeserializeObject<EMConvertSpriteSequenceFunction>(jo.ToString(), this);
                case EMType.ConvertModel:
                    return JsonConvert.DeserializeObject<EMConvertModelFunction>(jo.ToString(), this);
                case EMType.ImportNonGraphicsModel:
                    return JsonConvert.DeserializeObject<EMImportNonGraphicsModelFunction>(jo.ToString(), this);

                // NOOP
                case EMType.NOOP:
                    return JsonConvert.DeserializeObject<EMPlaceholderFunction>(jo.ToString(), this);

                default:
                    throw new InvalidOperationException();
            }
        }

        private object ReadConditionType(JObject jo)
        {
            EMConditionType type = (EMConditionType)jo[_conditionTypeName].Value<int>();
            switch (type)
            {
                // Entities
                case EMConditionType.EntityProperty:
                    return JsonConvert.DeserializeObject<EMEntityPropertyCondition>(jo.ToString(), this);
                case EMConditionType.SecretInRoom:
                    return JsonConvert.DeserializeObject<EMSecretRoomCondition>(jo.ToString(), this);

                // Rooms
                case EMConditionType.RoomContainsWater:
                    return JsonConvert.DeserializeObject<EMRoomContainsWaterCondition>(jo.ToString(), this);
                case EMConditionType.SectorContainsSecret:
                    return JsonConvert.DeserializeObject<EMSectorContainsSecretCondition>(jo.ToString(), this);
                case EMConditionType.SectorIsWall:
                    return JsonConvert.DeserializeObject<EMSectorIsWallCondition>(jo.ToString(), this);

                // Models
                case EMConditionType.ModelExists:
                    return JsonConvert.DeserializeObject<EMModelExistsCondition>(jo.ToString(), this);
                case EMConditionType.UnconditionalBirds:
                    return JsonConvert.DeserializeObject<EMUnconditionalBirdCheck>(jo.ToString(), this); 

                default:
                    throw new InvalidOperationException();
            }
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) { }
    }
}