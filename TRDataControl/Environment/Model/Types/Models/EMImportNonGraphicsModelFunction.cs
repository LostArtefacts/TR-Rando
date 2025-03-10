﻿using TRLevelControl.Model;

namespace TRDataControl.Environment;

public class EMImportNonGraphicsModelFunction : BaseEMFunction
{
    public List<EMMeshTextureData> Data { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        List<EMMeshTextureData> data = PrepareImportData(level.Models);
        if (!data.Any())
        {
            return;
        }

        TR1DataImporter importer = new()
        {
            Level = level,
            ClearUnusedSprites = false,
            TypesToImport = new(data.Select(m => (TR1Type)m.ModelID)),
            DataFolder = "Resources/TR1/Objects",
            IgnoreGraphics = true
        };
        importer.Import();

        RemapFaces(data, level.ObjectTextures.Count - 1, level.Models);
    }

    public override void ApplyToLevel(TR2Level level)
    {
        List<EMMeshTextureData> data = PrepareImportData(level.Models);
        if (!data.Any())
        {
            return;
        }

        TR2DataImporter importer = new()
        {
            Level = level,
            ClearUnusedSprites = false,
            TypesToImport = new(data.Select(m => (TR2Type)m.ModelID)),
            DataFolder = "Resources/TR2/Objects",
            IgnoreGraphics = true
        };
        importer.Import();

        RemapFaces(data, level.ObjectTextures.Count - 1, level.Models);
    }

    public override void ApplyToLevel(TR3Level level)
    {
        List<EMMeshTextureData> data = PrepareImportData(level.Models);
        if (!data.Any())
        {
            return;
        }

        TR3DataImporter importer = new()
        {
            Level = level,
            ClearUnusedSprites = false,
            TypesToImport = new(data.Select(m => (TR3Type)m.ModelID)),
            DataFolder = "Resources/TR3/Objects",
            IgnoreGraphics = true
        };
        importer.Import();

        RemapFaces(data, level.ObjectTextures.Count - 1, level.Models);
    }

    private List<EMMeshTextureData> PrepareImportData<T>(SortedDictionary<T, TRModel> existingModels)
        where T : Enum
    {
        return Data.Where(d => !existingModels.ContainsKey((T)(object)(uint)d.ModelID)).ToList();
    }

    private static void RemapFaces<T>(IEnumerable<EMMeshTextureData> data, int maximumTexture, SortedDictionary<T, TRModel> models)
        where T : Enum
    {
        foreach (EMMeshTextureData textureData in data)
        {
            TRModel model = models[(T)(object)(uint)textureData.ModelID];
            foreach (TRMesh mesh in model.Meshes)
            {
                foreach (TRMeshFace face in mesh.ColouredTriangles)
                {
                    face.Texture = SelectReplacementTexture(textureData, face.Texture, textureData.ColouredFace3, maximumTexture);
                }
                foreach (TRMeshFace face in mesh.ColouredRectangles)
                {
                    face.Texture = SelectReplacementTexture(textureData, face.Texture, textureData.ColouredFace4, maximumTexture);
                }
                foreach (TRMeshFace face in mesh.TexturedTriangles)
                {
                    face.Texture = SelectReplacementTexture(textureData, face.Texture, textureData.TexturedFace3, maximumTexture);
                }
                foreach (TRMeshFace face in mesh.TexturedRectangles)
                {
                    face.Texture = SelectReplacementTexture(textureData, face.Texture, textureData.TexturedFace4, maximumTexture);
                }
            }
        }
    }

    private static ushort SelectReplacementTexture(EMMeshTextureData data, ushort currentTexture, int defaultTexture, int maximumTexture)
    {
        if (data.TextureMap != null && data.TextureMap.ContainsKey(currentTexture))
        {
            return data.TextureMap[currentTexture];
        }

        if (defaultTexture != -1)
        {
            return (ushort)defaultTexture;
        }

        return currentTexture > maximumTexture ? (ushort)maximumTexture : currentTexture;
    }
}
