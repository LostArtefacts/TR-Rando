using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using TR2RandomizerCore.Environment.Types;

namespace TR2RandomizerCore.Environment
{
    public static class EnvironmentModificationFactory
    {
        public static List<BaseEnvironmentModification> GetModifications(string lvlName)
        {
            List<BaseEnvironmentModification> mods = new List<BaseEnvironmentModification>();
            string packPath = string.Format(@"Resources\Environment\{0}-Environment.json", lvlName);
            if (File.Exists(packPath))
            {
                Dictionary<EnvironmentModificationType, List<object>> definitions = JsonConvert.DeserializeObject<Dictionary<EnvironmentModificationType, List<object>>>(File.ReadAllText(packPath));
                foreach (EnvironmentModificationType type in definitions.Keys)
                {
                    foreach (object definition in definitions[type])
                    {
                        mods.Add(ConvertDefinitionToType(definition, type));
                    }
                }
            }

            return mods;
        }

        private static BaseEnvironmentModification ConvertDefinitionToType(object definition, EnvironmentModificationType type)
        {
            Type defType;
            switch (type)
            {
                case EnvironmentModificationType.Ladder:
                    defType = typeof(EMLadder);
                    break;
                case EnvironmentModificationType.Floor:
                    defType = typeof(EMFloor);
                    break;
                default:
                    throw new ArgumentException("Unsupported environment modification type: " + type);
            }

            return JsonConvert.DeserializeObject(definition.ToString(), defType) as BaseEnvironmentModification;
        }
    }
}