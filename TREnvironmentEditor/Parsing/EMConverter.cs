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
        private static readonly JsonSerializerSettings _resolver = new JsonSerializerSettings
        {
            ContractResolver = new EMResolver()
        };

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(BaseEMFunction) || objectType == typeof(BaseEMCondition);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            if (jo["EMType"] != null)
            {
                return ReadEMType(jo);
            }
            else if (jo["ConditionType"] != null)
            {
                return ReadConditionType(jo);
            }

            return null;
        }

        private object ReadEMType(JObject jo)
        {
            EMType type = (EMType)jo["EMType"].Value<int>();
            switch (type)
            {
                // Surface types
                case EMType.Ladder:
                    return JsonConvert.DeserializeObject<EMLadderFunction>(jo.ToString(), _resolver);
                case EMType.Floor:
                    return JsonConvert.DeserializeObject<EMFloorFunction>(jo.ToString(), _resolver);
                case EMType.Flood:
                    return JsonConvert.DeserializeObject<EMFloodFunction>(jo.ToString(), _resolver);
                case EMType.Drain:
                    return JsonConvert.DeserializeObject<EMDrainFunction>(jo.ToString(), _resolver);
                case EMType.Ceiling:
                    return JsonConvert.DeserializeObject<EMCeilingFunction>(jo.ToString(), _resolver);

                // Texture types
                case EMType.Reface:
                    return JsonConvert.DeserializeObject<EMRefaceFunction>(jo.ToString(), _resolver);
                case EMType.RemoveFace:
                    return JsonConvert.DeserializeObject<EMRemoveFaceFunction>(jo.ToString(), _resolver);
                case EMType.ModifyFace:
                    return JsonConvert.DeserializeObject<EMModifyFaceFunction>(jo.ToString(), _resolver);
                case EMType.AddStaticMesh:
                    return JsonConvert.DeserializeObject<EMAddStaticMeshFunction>(jo.ToString(), _resolver);
                case EMType.RemoveStaticMesh:
                    return JsonConvert.DeserializeObject<EMRemoveStaticMeshFunction>(jo.ToString(), _resolver);
                case EMType.AddFace:
                    return JsonConvert.DeserializeObject<EMAddFaceFunction>(jo.ToString(), _resolver);

                // Entity types
                case EMType.MoveSlot:
                    return JsonConvert.DeserializeObject<EMMoveSlotFunction>(jo.ToString(), _resolver);
                case EMType.MoveEnemy:
                    return JsonConvert.DeserializeObject<EMMoveEnemyFunction>(jo.ToString(), _resolver);
                case EMType.MovePickup:
                    return JsonConvert.DeserializeObject<EMMovePickupFunction>(jo.ToString(), _resolver);
                case EMType.MoveEntity:
                    return JsonConvert.DeserializeObject<EMMoveEntityFunction>(jo.ToString(), _resolver);
                case EMType.ConvertEntity:
                    return JsonConvert.DeserializeObject<EMConvertEntityFunction>(jo.ToString(), _resolver);
                case EMType.MoveTrap:
                    return JsonConvert.DeserializeObject<EMMoveTrapFunction>(jo.ToString(), _resolver);
                case EMType.ConvertEnemy:
                    return JsonConvert.DeserializeObject<EMConvertEnemyFunction>(jo.ToString(), _resolver);
                case EMType.ModifyEntity:
                    return JsonConvert.DeserializeObject<EMModifyEntityFunction>(jo.ToString(), _resolver);
                case EMType.SwapSlot:
                    return JsonConvert.DeserializeObject<EMSwapSlotFunction>(jo.ToString(), _resolver);
                case EMType.AdjustEntityPositions:
                    return JsonConvert.DeserializeObject<EMAdjustEntityPositionFunction>(jo.ToString(), _resolver);
                case EMType.AddEntity:
                    return JsonConvert.DeserializeObject<EMAddEntityFunction>(jo.ToString(), _resolver);

                // Trigger types
                case EMType.Trigger:
                    return JsonConvert.DeserializeObject<EMTriggerFunction>(jo.ToString(), _resolver);
                case EMType.RemoveTrigger:
                    return JsonConvert.DeserializeObject<EMRemoveTriggerFunction>(jo.ToString(), _resolver);
                case EMType.DuplicateTrigger:
                    return JsonConvert.DeserializeObject<EMDuplicateTriggerFunction>(jo.ToString(), _resolver);
                case EMType.DuplicateSwitchTrigger:
                    return JsonConvert.DeserializeObject<EMDuplicateSwitchTriggerFunction>(jo.ToString(), _resolver);
                case EMType.CameraTriggerFunction:
                    return JsonConvert.DeserializeObject<EMCameraTriggerFunction>(jo.ToString(), _resolver);
                case EMType.ReplaceTriggerActionParameterFunction:
                    return JsonConvert.DeserializeObject<EMReplaceTriggerActionParameterFunction>(jo.ToString(), _resolver);
                case EMType.MoveTrigger:
                    return JsonConvert.DeserializeObject<EMMoveTriggerFunction>(jo.ToString(), _resolver);
                case EMType.AppendTriggerActionFunction:
                    return JsonConvert.DeserializeObject<EMAppendTriggerActionFunction>(jo.ToString(), _resolver);

                // Portals
                case EMType.VisibilityPortal:
                    return JsonConvert.DeserializeObject<EMVisibilityPortalFunction>(jo.ToString(), _resolver);
                case EMType.HorizontalCollisionalPortal:
                    return JsonConvert.DeserializeObject<EMHorizontalCollisionalPortalFunction>(jo.ToString(), _resolver);
                case EMType.VerticalCollisionalPortal:
                    return JsonConvert.DeserializeObject<EMVerticalCollisionalPortalFunction>(jo.ToString(), _resolver);

                // Sounds
                case EMType.AddSoundSource:
                    return JsonConvert.DeserializeObject<EMAddSoundSourceFunction>(jo.ToString(), _resolver);
                case EMType.MoveSoundSource:
                    return JsonConvert.DeserializeObject<EMMoveSoundSourceFunction>(jo.ToString(), _resolver);
                case EMType.RemoveSoundSource:
                    return JsonConvert.DeserializeObject<EMRemoveSoundSourceFunction>(jo.ToString(), _resolver);

                // Rooms
                case EMType.ModifyRoom:
                    return JsonConvert.DeserializeObject<EMModifyRoomFunction>(jo.ToString(), _resolver);
                case EMType.ModifyOverlaps:
                    return JsonConvert.DeserializeObject<EMModifyOverlapsFunction>(jo.ToString(), _resolver);
                case EMType.CopyRoom:
                    return JsonConvert.DeserializeObject<EMCopyRoomFunction>(jo.ToString(), _resolver);
                case EMType.CopyVertexAttributes:
                    return JsonConvert.DeserializeObject<EMCopyVertexAttributesFunction>(jo.ToString(), _resolver);

                case EMType.ImportModel:
                    return JsonConvert.DeserializeObject<EMImportModelFunction>(jo.ToString(), _resolver);

                // NOOP
                case EMType.NOOP:
                    return JsonConvert.DeserializeObject<EMPlaceholderFunction>(jo.ToString(), _resolver);

                default:
                    throw new InvalidOperationException();
            }
        }

        private object ReadConditionType(JObject jo)
        {
            EMConditionType type = (EMConditionType)jo["ConditionType"].Value<int>();
            switch (type)
            {
                case EMConditionType.EntityProperty:
                    return JsonConvert.DeserializeObject<EMEntityPropertyCondition>(jo.ToString());

                default:
                    throw new InvalidOperationException();
            }
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) { }
    }
}