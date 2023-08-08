using TREnvironmentEditor.Helpers;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRLevelControl.Model.Enums;
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

        TR1ModelImporter importer = new TR1ModelImporter
        {
            Level = level,
            ClearUnusedSprites = false,
            EntitiesToImport = data.Select(m => (TREntities)m.ModelID),
            DataFolder = @"Resources\TR1\Models",
            IgnoreGraphics = true
        };
        importer.Import();

        RemapFaces(data, level.NumObjectTextures - 1, modelID => TRMeshUtilities.GetModelMeshes(level, (TREntities)modelID));
    }

    public override void ApplyToLevel(TR2Level level)
    {
        List<EMMeshTextureData> data = PrepareImportData(level.Models);
        if (data.Count == 0)
        {
            return;
        }

        TR2ModelImporter importer = new TR2ModelImporter
        {
            Level = level,
            ClearUnusedSprites = false,
            EntitiesToImport = data.Select(m => (TR2Entities)m.ModelID),
            DataFolder = @"Resources\TR2\Models",
            IgnoreGraphics = true
        };
        importer.Import();

        RemapFaces(data, level.NumObjectTextures - 1, modelID => TRMeshUtilities.GetModelMeshes(level, (TR2Entities)modelID));
    }

    public override void ApplyToLevel(TR3Level level)
    {
        List<EMMeshTextureData> data = PrepareImportData(level.Models);
        if (data.Count == 0)
        {
            return;
        }

        TR3ModelImporter importer = new TR3ModelImporter
        {
            Level = level,
            ClearUnusedSprites = false,
            EntitiesToImport = data.Select(m => (TR3Entities)m.ModelID),
            DataFolder = @"Resources\TR3\Models",
            IgnoreGraphics = true
        };
        importer.Import();

        RemapFaces(data, level.NumObjectTextures - 1, modelID => TRMeshUtilities.GetModelMeshes(level, (TR3Entities)modelID));
    }

    private List<EMMeshTextureData> PrepareImportData(TRModel[] existingModels)
    {
        List<EMMeshTextureData> importData = new List<EMMeshTextureData>();
        foreach (EMMeshTextureData data in Data)
        {
            if (Array.Find(existingModels, m => m.ID == data.ModelID) == null)
            {
                importData.Add(data);
            }
        }
        return importData;
    }

    private void RemapFaces(List<EMMeshTextureData> data, uint maximumTexture, Func<short, TRMesh[]> meshAction)
    {
        foreach (EMMeshTextureData textureData in data)
        {
            TRMesh[] meshes = meshAction.Invoke(textureData.ModelID);
            foreach (TRMesh mesh in meshes)
            {
                foreach (TRFace3 face in mesh.ColouredTriangles)
                {
                    face.Texture = SelectReplacementTexture(textureData, face.Texture, textureData.ColouredFace3, maximumTexture);
                }
                foreach (TRFace4 face in mesh.ColouredRectangles)
                {
                    face.Texture = SelectReplacementTexture(textureData, face.Texture, textureData.ColouredFace4, maximumTexture);
                }
                foreach (TRFace3 face in mesh.TexturedTriangles)
                {
                    face.Texture = SelectReplacementTexture(textureData, face.Texture, textureData.TexturedFace3, maximumTexture);
                }
                foreach (TRFace4 face in mesh.TexturedRectangles)
                {
                    face.Texture = SelectReplacementTexture(textureData, face.Texture, textureData.TexturedFace4, maximumTexture);
                }
            }
        }
    }

    private ushort SelectReplacementTexture(EMMeshTextureData data, ushort currentTexture, int defaultTexture, uint maximumTexture)
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
