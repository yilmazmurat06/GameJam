using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Settings")]
    public int width = 50;
    public int height = 50;
    public float perlinScale = 0.1f;
    
    [Header("References")]
    public Texture2D floorTexture;
    public Texture2D wallTexture;

    [MenuItem("Tools/Generate Dungeon")]
    public static void Generate()
    {
        GameObject go = new GameObject("Dungeon Generator");
        DungeonGenerator generator = go.AddComponent<DungeonGenerator>();
        
        // Find texture assets by name if not assigned (since we are calling static)
        if (generator.floorTexture == null)
            generator.floorTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/Environment/dungeon_floor.png");
        
        if (generator.wallTexture == null)
            generator.wallTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/Environment/dungeon_wall.png");
            
        generator.BuildDungeon();
        
        // Cleanup generator GO because we just wanted the script execution
        DestroyImmediate(go);
    }

    public void BuildDungeon()
    {
        // 1. Setup Grid and Tilemap
        GameObject gridGO = GameObject.Find("Grid");
        if (gridGO == null)
        {
            gridGO = new GameObject("Grid");
            gridGO.AddComponent<Grid>();
        }

        GameObject tilemapGO = GameObject.Find("DungeonTilemap");
        if (tilemapGO == null)
        {
            tilemapGO = new GameObject("DungeonTilemap");
            tilemapGO.transform.SetParent(gridGO.transform);
        }
        
        Tilemap tilemap = tilemapGO.GetComponent<Tilemap>();
        if (tilemap == null) tilemap = tilemapGO.AddComponent<Tilemap>();
        
        TilemapRenderer tr = tilemapGO.GetComponent<TilemapRenderer>();
        if (tr == null) tr = tilemapGO.AddComponent<TilemapRenderer>();
        tr.sortingOrder = -10; // Floor below everything

        // Assign URP Lit Material if available
        Material litMaterial = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Lit-Default.mat");
        if (litMaterial != null) tr.material = litMaterial;

        // Ensure we have a defined layer for walls/collisions later?
        // For now just visual.
        
        // 2. Create Tiles
        Tile floorTile = ScriptableObject.CreateInstance<Tile>();
        floorTile.sprite = Sprite.Create(floorTexture, new Rect(0,0,floorTexture.width, floorTexture.height), new Vector2(0.5f, 0.5f), 1024); 
        // Using 32 PPU to match our "huge" style or 512?
        // Let's use 512 to match the new enemy/player scale standard if textures are hi-res.
        // The generated images are likely 1024x1024 or 512x512.
        // Let's reload the sprite properly from asset database to get its settings.
        
        string floorPath = AssetDatabase.GetAssetPath(floorTexture);
        Sprite floorSprite = AssetDatabase.LoadAssetAtPath<Sprite>(floorPath);
        if (floorSprite != null) floorTile.sprite = floorSprite;
        
        Tile wallTile = ScriptableObject.CreateInstance<Tile>();
        string wallPath = AssetDatabase.GetAssetPath(wallTexture);
        Sprite wallSprite = AssetDatabase.LoadAssetAtPath<Sprite>(wallPath);
        if (wallSprite != null) wallTile.sprite = wallSprite;
        
        // 3. Generate Map
        tilemap.ClearAllTiles();

        for (int x = -width/2; x < width/2; x++)
        {
            for (int y = -height/2; y < height/2; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                
                // Borders are walls
                if (x == -width/2 || x == width/2 -1 || y == -height/2 || y == height/2 -1)
                {
                    tilemap.SetTile(pos, wallTile);
                    // Add collider for walls?
                    // TilemapCollider2D will handle this if we add it to the GameObject.
                }
                else
                {
                    tilemap.SetTile(pos, floorTile);
                }
            }
        }
        
        // Add collisions
        TilemapCollider2D col = tilemapGO.GetComponent<TilemapCollider2D>();
        if (col == null) col = tilemapGO.AddComponent<TilemapCollider2D>();
        
        
        // Add Lighting Global
        // To be done in next step: URP setup
        
        PlaceTorches();
        SpawnPlayer();
        FixCamera();
        
        Debug.Log("Dungeon Generated!");
    }

    void SpawnPlayer()
    {
        GameObject player = GameObject.Find("Player");
        if (player == null)
        {
            // Try to load prefab
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Player.prefab");
            if (prefab == null)
            {
                // Try searching
                string[] guids = AssetDatabase.FindAssets("Player t:Prefab");
                if (guids.Length > 0)
                    prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }
            
            if (prefab != null)
            {
                player = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                player.name = "Player";
            }
            else
            {
                Debug.LogError("Could not find Player prefab!");
                return;
            }
        }
        
        // Reset Position to center
        player.transform.position = Vector3.zero;
        
        // Ensure Sorting Order high enough
        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
        if (sr != null) 
        {
            sr.sortingOrder = 10; // Above walls/floor/torches
            // Assign Lit Material too
             Material litMaterial = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Lit-Default.mat");
             if (litMaterial != null) sr.material = litMaterial;
        }

        Debug.Log("Player spawned/reset at (0,0)");
    }

    void FixCamera()
    {
        GameObject cam = GameObject.Find("Main Camera");
        if (cam == null)
        {
             cam = new GameObject("Main Camera");
             cam.AddComponent<Camera>();
             cam.tag = "MainCamera";
        }
        
        cam.transform.position = new Vector3(0, 0, -10);
        
        Camera cameraComponent = cam.GetComponent<Camera>();
        cameraComponent.orthographic = true;
        cameraComponent.orthographicSize = 5;
        cameraComponent.clearFlags = CameraClearFlags.SolidColor;
        cameraComponent.backgroundColor = Color.black;

        // Ensure URP Data
        var ura = cam.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
        if (ura == null) ura = cam.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
        
        ura.renderType = UnityEngine.Rendering.Universal.CameraRenderType.Base;
    }


    [Header("Torches")]
    public Texture2D torchTexture;
    public int torchInterval = 6;

    void PlaceTorches()
    {
        if (torchTexture == null)
            torchTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/Environment/dungeon_torch.png");

        // Find grid to attach torches to
        GameObject gridGO = GameObject.Find("Grid");
        if (gridGO == null) return;

        Transform torchContainer = gridGO.transform.Find("Torches");
        if (torchContainer != null) DestroyImmediate(torchContainer.gameObject);
        
        GameObject container = new GameObject("Torches");
        container.transform.SetParent(gridGO.transform);


        Sprite torchSprite = Sprite.Create(torchTexture, new Rect(0,0,torchTexture.width, torchTexture.height), new Vector2(0.5f, 0.5f), 1024);

        // Scan borders
        for (int x = -width/2; x < width/2; x++)
        {
            if (x % torchInterval == 0)
            {
                SpawnTorch(new Vector3(x, -height/2, 0), container.transform, torchSprite); // Bottom wall
                SpawnTorch(new Vector3(x, height/2 - 1, 0), container.transform, torchSprite); // Top wall
            }
        }
        
        for (int y = -height/2; y < height/2; y++)
        {
            if (y % torchInterval == 0)
            {
                SpawnTorch(new Vector3(-width/2, y, 0), container.transform, torchSprite); // Left wall
                SpawnTorch(new Vector3(width/2 - 1, y, 0), container.transform, torchSprite); // Right wall
            }
        }
    }

    void SpawnTorch(Vector3 position, Transform parent, Sprite sprite)
    {
        GameObject go = new GameObject("WallTorch");
        go.transform.position = position + new Vector3(0, 0.2f, 0); // Slightly up on wall
        go.transform.SetParent(parent);
        
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 5; // Above wall

        // Assign URP Lit Material
        Material litMaterial = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Lit-Default.mat");
        if (litMaterial != null) sr.material = litMaterial;

        // Add Light
        UnityEngine.Rendering.Universal.Light2D light = go.AddComponent<UnityEngine.Rendering.Universal.Light2D>();
        light.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Point;
        light.color = new Color(1f, 0.6f, 0.2f, 1f); // Orange
        light.intensity = 1.0f;
        light.pointLightOuterRadius = 4f;
        light.pointLightInnerRadius = 0.5f;
    }
}
