using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TREnvironmentEditor.Helpers;
using TREnvironmentEditor.Model.Types;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model
{
    public class EMEditorSet : List<BaseEMFunction>
    {
        // A set of modifications that must be done together e.g. adding a ladder and a step

        public void ApplyToLevel(TR2Level level, IEnumerable<EMType> excludedTypes)
        {
            if (IsApplicable(excludedTypes))
            {
                foreach (BaseEMFunction mod in this)
                {
                    mod.ApplyToLevel(level);
                }
            }
        }

        public bool IsApplicable(IEnumerable<EMType> excludedTypes)
        {
            // The modification will only be performed if all types in this set are to be included.
            foreach (BaseEMFunction mod in this)
            {
                if (excludedTypes.Contains(mod.EMType))
                {
                    return false;
                }
            }

            return true;
        }

        public void RemapTextures(Dictionary<ushort, ushort> indexMap)
        {
            // Find every EMTextureMap and remap the old texture indices to new. This isn't
            // ideal as textures are either stored in EMTextureMap or directly in ushorts.
            Type textureMapType = typeof(EMTextureMap);
            string remapMethod = "Remap";

            foreach (BaseEMFunction mod in this)
            {
                List<PropertyInfo> props = mod.GetType().GetProperties().ToList().FindAll(p => p.PropertyType == textureMapType);
                foreach (PropertyInfo prop in props)
                {
                    MethodInfo method = prop.PropertyType.GetMethod(remapMethod);
                    method.Invoke(prop.GetValue(mod, null), new object[] { indexMap });
                }

                if (mod is EMFloorFunction floorMod)
                {
                    if (indexMap.ContainsKey(floorMod.FloorTexture))
                    {
                        floorMod.FloorTexture = indexMap[floorMod.FloorTexture];
                    }
                    if (indexMap.ContainsKey(floorMod.SideTexture))
                    {
                        floorMod.SideTexture = indexMap[floorMod.SideTexture];
                    }
                }
            }
        }
    }
}