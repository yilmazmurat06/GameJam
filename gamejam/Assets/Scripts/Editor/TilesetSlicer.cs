using UnityEngine;
using UnityEditor;

public class TilesetSlicer
{
    [MenuItem("Tools/Slice Dungeon Tileset")]
    public static void SliceTileset()
    {
        string path = "Assets/Dungeon_Tileset.png";
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 16; // 16 PPU typical for 16x16 tiles
            importer.filterMode = FilterMode.Point; // Pixel art crispness
            
            // Assume 16x16 tiles for now
            int tileWidth = 16;
            int tileHeight = 16;
            
            // Need to read texture size, but we can't easily get it without loading
            // Let's force a reload to ensure we can read it
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            
            if (tex != null)
            {
                int colCount = tex.width / tileWidth;
                int rowCount = tex.height / tileHeight;
                
                System.Collections.Generic.List<SpriteMetaData> metas = new System.Collections.Generic.List<SpriteMetaData>();
                
                for (int r = 0; r < rowCount; r++) // Rows from bottom to top? Unity coords are bottom-left 0,0
                {
                    for (int c = 0; c < colCount; c++)
                    {
                        SpriteMetaData meta = new SpriteMetaData();
                        meta.rect = new Rect(c * tileWidth, (rowCount - 1 - r) * tileHeight, tileWidth, tileHeight); // Start top-left
                        // OR: meta.rect = new Rect(c * tileWidth, r * tileHeight, tileWidth, tileHeight); // Start bottom-left
                        
                        // Let's do standard bottom->top scanning matching Unity's texture coords
                        // But renaming them to logical Grid indices (x_y)
                         meta.rect = new Rect(c * tileWidth, r * tileHeight, tileWidth, tileHeight);
                         meta.name = $"Dungeon_Tileset_{c}_{r}";
                         meta.pivot = new Vector2(0.5f, 0.5f);
                         metas.Add(meta);
                    }
                }
                
                importer.spritesheet = metas.ToArray();
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                Debug.Log($"Sliced {path} into {metas.Count} sprites.");
            }
            else
            {
                Debug.LogError("Could not load texture to slice at " + path);
            }
        }
        else
        {
            Debug.LogError("Could not find TextureImporter at " + path);
        }
    }
}