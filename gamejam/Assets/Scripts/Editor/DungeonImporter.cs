using UnityEngine;
using UnityEditor;

public class DungeonImporter : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        if (assetPath.Contains("dungeon_floor.png") || assetPath.Contains("dungeon_wall.png") || assetPath.Contains("dungeon_torch.png"))
        {
            TextureImporter importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 1024;
            importer.filterMode = FilterMode.Point; // Pixel art look
            importer.compressionQuality = 0;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
        }
    }

    [MenuItem("Tools/Force Reimport Dungeon")]
    public static void Reimport()
    {
        AssetDatabase.ImportAsset("Assets/Sprites/Environment/dungeon_floor.png", ImportAssetOptions.ForceUpdate);
        AssetDatabase.ImportAsset("Assets/Sprites/Environment/dungeon_wall.png", ImportAssetOptions.ForceUpdate);
        AssetDatabase.ImportAsset("Assets/Sprites/Environment/dungeon_torch.png", ImportAssetOptions.ForceUpdate);
        Debug.Log("Reimported dungeon assets.");
    }
}
