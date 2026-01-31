using UnityEngine;
using UnityEditor;
using System.IO;

public class SpriteSlicer : MonoBehaviour
{
    [MenuItem("Tools/Slice Banana Enemy")]
    public static void SliceBanana()
    {
        string path = "Assets/Sprites/Enemies/banana_enemy.png";
        
        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null)
        {
            Debug.LogError("Texture not found at: " + path);
            return;
        }

        ti.isReadable = true;
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Multiple;
        ti.mipmapEnabled = false;
        ti.filterMode = FilterMode.Point;
        ti.spritePixelsPerUnit = 32; // Standard for pixel art
        ti.compressionQuality = 0;
        ti.textureCompression = TextureImporterCompression.Uncompressed;

        // Create sprites
        // Assumption: 4 columns, 2 rows (Top=Idle, Bottom=Run)
        // or Top=Run, Bottom=Idle depending on generation.
        // Usually generated sheets are 2 rows.
        
        int colCount = 4;
        int rowCount = 2;
        
        // Need to force import first to get width/height if not loaded
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        int width = texture.width;
        int height = texture.height;
        int cellW = width / colCount;
        int cellH = height / rowCount;

        SpriteMetaData[] metaData = new SpriteMetaData[colCount * rowCount];
        
        string[] directions = { "Down", "Up", "Side", "Side" }; // Assuming Left/Right are same or similar
        
        int index = 0;
        // Top row (y=1) -> usually Idle? Or Run?
        // Bottom row (y=0)
        
        // Let's name them generically first: Row0_Col0, etc.
        // Row 1 (Top)
        for (int x = 0; x < colCount; x++)
        {
            metaData[index] = new SpriteMetaData
            {
                name = $"Banana_RowTop_{x}",
                rect = new Rect(x * cellW, cellH, cellW, cellH),
                alignment = (int)SpriteAlignment.Center,
                pivot = new Vector2(0.5f, 0.5f)
            };
            index++;
        }
        
        // Row 0 (Bottom)
        for (int x = 0; x < colCount; x++)
        {
            metaData[index] = new SpriteMetaData
            {
                name = $"Banana_RowBot_{x}",
                rect = new Rect(x * cellW, 0, cellW, cellH),
                alignment = (int)SpriteAlignment.Center,
                pivot = new Vector2(0.5f, 0.5f)
            };
            index++;
        }

        ti.spritesheet = metaData;
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        
        Debug.Log($"Sliced {path} into {index} sprites.");
    }
}
