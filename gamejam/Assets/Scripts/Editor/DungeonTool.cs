using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.IO;
using System.Collections.Generic;

public class DungeonTool : MonoBehaviour
{
    [MenuItem("Tools/Setup Manual Building")]
    public static void SetupManual()
    {
        Debug.Log("Starting Manual Building Setup...");
        
        // 1. Ensure Tiles Directory
        string tilePath = "Assets/Tiles";
        if (!Directory.Exists(tilePath))
        {
            Directory.CreateDirectory(tilePath);
            AssetDatabase.Refresh();
        }

        // 2. Load Sprites & Create Tile Assets
        List<Tile> allTiles = new List<Tile>();
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath("Assets/Dungeon_Tileset.png");
        
        foreach (Object asset in assets)
        {
            if (asset is Sprite sprite)
            {
                string tileName = sprite.name;
                string path = $"{tilePath}/{tileName}.asset";
                
                // Check if tile exists, otherwise create
                Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
                if (tile == null)
                {
                    tile = ScriptableObject.CreateInstance<Tile>();
                    tile.sprite = sprite;
                    tile.colliderType = Tile.ColliderType.Sprite; 
                    AssetDatabase.CreateAsset(tile, path);
                }
                allTiles.Add(tile);
            }
        }
        Debug.Log($"Ensured {allTiles.Count} Tile assets in {tilePath}.");

        // 3. Create Palette Prefab
        string palettePath = "Assets/Palettes";
        if (!Directory.Exists(palettePath))
        {
            Directory.CreateDirectory(palettePath);
            AssetDatabase.Refresh();
        }
        
        string palettePrefabPath = $"{palettePath}/DungeonPalette.prefab";
        GameObject paletteGO = new GameObject("DungeonPalette");
        GameObject layerGO = new GameObject("Layer1");
        layerGO.transform.SetParent(paletteGO.transform);
        
        // Add components required for a Palette
        Grid grid = paletteGO.AddComponent<Grid>();
        // Palettes usually use standard cell layout
        
        Tilemap tm = layerGO.AddComponent<Tilemap>();
        TilemapRenderer tr = layerGO.AddComponent<TilemapRenderer>();
        
        // Layout tiles in a grid on the palette
        int cols = 10; // 10 columns wide
        for (int i = 0; i < allTiles.Count; i++)
        {
            int x = i % cols;
            int y = -(i / cols); // Build downwards
            tm.SetTile(new Vector3Int(x, y, 0), allTiles[i]);
        }
        
        // Save as Prefab
        PrefabUtility.SaveAsPrefabAsset(paletteGO, palettePrefabPath);
        DestroyImmediate(paletteGO);
        Debug.Log($"Created Palette at {palettePrefabPath}");

        // 4. Setup Scene
        SetupScene();
        
        AssetDatabase.Refresh();
        Debug.Log("Manual Building Setup Complete!");
    }

    private static void SetupScene()
    {
        // Cleanup DungeonGenerator if it exists
        GameObject gen = GameObject.Find("Dungeon Generator");
        if (gen != null) DestroyImmediate(gen);

        // Ensure Grid
        GameObject gridGO = GameObject.Find("Grid");
        if (gridGO == null)
        {
            gridGO = new GameObject("Grid");
            gridGO.AddComponent<Grid>();
        }

        // Floor Layer
        CreateLayer(gridGO.transform, "Floor", -10, false);
        
        // Wall Layer (Collision)
        CreateLayer(gridGO.transform, "Walls", 0, true);
        
        // Decoration Layer
        CreateLayer(gridGO.transform, "Decoration", 1, false);
    }
    
    private static void CreateLayer(Transform parent, string name, int sortOrder, bool collision)
    {
        Transform child = parent.Find(name);
        GameObject go;
        if (child == null)
        {
            go = new GameObject(name);
            go.transform.SetParent(parent);
        }
        else
        {
            go = child.gameObject;
        }
        
        Tilemap tm = go.GetComponent<Tilemap>();
        if (tm == null) tm = go.AddComponent<Tilemap>();
        
        TilemapRenderer tr = go.GetComponent<TilemapRenderer>();
        if (tr == null) tr = go.AddComponent<TilemapRenderer>();
        tr.sortingOrder = sortOrder;
        
        // Lit material
        Material litMaterial = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Lit-Default.mat");
        if (litMaterial != null) tr.material = litMaterial;

        if (collision)
        {
            TilemapCollider2D col = go.GetComponent<TilemapCollider2D>();
            if (col == null) go.AddComponent<TilemapCollider2D>();
            
            // Optional: CompositeCollider2D for smoother walls
            // Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
            // if (rb == null) rb = go.AddComponent<Rigidbody2D>();
            // rb.bodyType = RigidbodyType2D.Static;
            // CompositeCollider2D comp = go.GetComponent<CompositeCollider2D>();
            // if (comp == null) comp = go.AddComponent<CompositeCollider2D>();
            // col.usedByComposite = true;
        }
    }
}