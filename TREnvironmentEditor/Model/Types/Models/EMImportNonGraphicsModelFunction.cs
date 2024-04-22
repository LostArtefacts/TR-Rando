using TREnvironmentEditor.Helpers;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRModelTransporter.Transport;

namespace TREnvironmentEditor.Model.Types;

public class EMImportNonGraphicsModelFunction : BaseEMFunction
{
    public List<EMMeshTextureData> Data { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        List<EMMeshTextureData> data = PrepareImportData(level.Models);
        if (data.Count == 0)
        {
            return;
        }

        TR1ModelImporter importer = new()
        {
            Level = level,
            ClearUnusedSprites = false,
            EntitiesToImport = data.Select(m => (TR1Type)m.ModelID),
            DataFolder = @"Resources\TR1\Models",
            IgnoreGraphics = true
        };
        importer.Import();

        RemapFaces(data, level.ObjectTextures.Count - 1, modelID => TRMeshUtilities.GetModelMeshes(level, (TR1Type)modelID));
    }

    public override void ApplyToLevel(TR2Level level)
    {
        List<EMMeshTextureData> data = PrepareImportData(level.Models);
        if (data.Count == 0)
        {
            return;
        }

        TR2ModelImporter importer = new()
        {
            Level = level,
            ClearUnusedSprites = false,
            EntitiesToImport = data.Select(m => (TR2Type)m.ModelID),
            DataFolder = @"Resources\TR2\Models",
            IgnoreGraphics = true
        };
        importer.Import();

        RemapFaces(data, level.ObjectTextures.Count - 1, modelID => TRMeshUtilities.GetModelMeshes(level, (TR2Type)modelID));
    }

    public override void ApplyToLevel(TR3Level level)
    {
        List<EMMeshTextureData> data = PrepareImportData(level.Models);
        if (data.Count == 0)
        {
            return;
        }

        TR3ModelImporter importer = new()
        {
            Level = level,
            ClearUnusedSprites = false,
            EntitiesToImport = data.Select(m => (TR3Type)m.ModelID),
            DataFolder = @"Resources\TR3\Models",
            IgnoreGraphics = true
        };
        importer.Import();

        RemapFaces(data, level.ObjectTextures.Count - 1, modelID => TRMeshUtilities.GetModelMeshes(level, (TR3Type)modelID));
    }

    private List<EMMeshTextureData> PrepareImportData(List<TRModel> existingModels)
    {
        List<EMMeshTextureData> importData = new();
        foreach (EMMeshTextureData data in Data)
        {
            if (existingModels.Find(m => m.ID == data.ModelID) == null)
            {
                importData.Add(data);
            }
        }
        return importData;
    }

    private static void RemapFaces(List<EMMeshTextureData> data, int maximumTexture, Func<short, List<TRMesh>> meshAction)
    {
        foreach (EMMeshTextureData textureData in data)
        {
            List<TRMesh> meshes = meshAction.Invoke(textureData.ModelID);
            foreach (TRMesh mesh in meshes)
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
