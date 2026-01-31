using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using System.Collections.Generic;

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
        // Clear selection to avoid Inspector errors when destroying objects
        Selection.activeGameObject = null;
        
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
        // 1. Setup Grid
        GameObject gridGO = GameObject.Find("Grid");
        if (gridGO == null)
        {
            gridGO = new GameObject("Grid");
            gridGO.AddComponent<Grid>();
        }

        // --- Floor Tilemap (No Collision) ---
        GameObject floorGO = GameObject.Find("FloorTilemap");
        if (floorGO == null)
        {
            floorGO = new GameObject("FloorTilemap");
            floorGO.transform.SetParent(gridGO.transform);
        }
        
        Tilemap floorTm = floorGO.GetComponent<Tilemap>();
        if (floorTm == null) floorTm = floorGO.AddComponent<Tilemap>();
        
        TilemapRenderer floorTr = floorGO.GetComponent<TilemapRenderer>();
        if (floorTr == null) floorTr = floorGO.AddComponent<TilemapRenderer>();
        floorTr.sortingOrder = -10; 
        
        // Remove old collider if it exists on floor
        if (floorGO.GetComponent<TilemapCollider2D>() != null) 
            DestroyImmediate(floorGO.GetComponent<TilemapCollider2D>());

        // --- Wall Tilemap (Collision) ---
        GameObject wallGO = GameObject.Find("WallTilemap");
        if (wallGO == null)
        {
            wallGO = new GameObject("WallTilemap");
            wallGO.transform.SetParent(gridGO.transform);
        }
        
        Tilemap wallTm = wallGO.GetComponent<Tilemap>();
        if (wallTm == null) wallTm = wallGO.AddComponent<Tilemap>();
        
        TilemapRenderer wallTr = wallGO.GetComponent<TilemapRenderer>();
        if (wallTr == null) wallTr = wallGO.AddComponent<TilemapRenderer>();
        wallTr.sortingOrder = 0; 

        // Add Collision to Walls ONLY
        TilemapCollider2D wallCol = wallGO.GetComponent<TilemapCollider2D>();
        if (wallCol == null) wallCol = wallGO.AddComponent<TilemapCollider2D>();
        
        // Cleanup old single "DungeonTilemap" if it exists to avoid confusion
        GameObject oldTm = GameObject.Find("DungeonTilemap");
        if (oldTm != null) DestroyImmediate(oldTm);

        // Assign URP Lit Material
        Material litMaterial = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Lit-Default.mat");
        if (litMaterial != null) 
        {
            floorTr.material = litMaterial;
            wallTr.material = litMaterial;
        }

        // 2. Create Tiles
        // ... (Keep existing tile creation logic, just updated variable names)
        
        Tile floorTile = ScriptableObject.CreateInstance<Tile>();
        // Using PPU 1024 or standard? Let's check existing texture. 
        // We will just re-load from path to be safe and use its settings.
        
        string floorPath = AssetDatabase.GetAssetPath(floorTexture);
        if (string.IsNullOrEmpty(floorPath)) floorPath = "Assets/Sprites/Environment/dungeon_floor.png";
        
        Sprite floorSprite = AssetDatabase.LoadAssetAtPath<Sprite>(floorPath);
        if (floorSprite != null) floorTile.sprite = floorSprite;
        else 
        {
             // Fallback create
             if (floorTexture != null)
                 floorTile.sprite = Sprite.Create(floorTexture, new Rect(0,0,floorTexture.width, floorTexture.height), new Vector2(0.5f, 0.5f), 1024);
        }
        
        Tile wallTile = ScriptableObject.CreateInstance<Tile>();
        string wallPath = AssetDatabase.GetAssetPath(wallTexture);
        if (string.IsNullOrEmpty(wallPath)) wallPath = "Assets/Sprites/Environment/dungeon_wall.png";
        
        Sprite wallSprite = AssetDatabase.LoadAssetAtPath<Sprite>(wallPath);
        if (wallSprite != null) wallTile.sprite = wallSprite;
        else
        {
             if (wallTexture != null)
                 wallTile.sprite = Sprite.Create(wallTexture, new Rect(0,0,wallTexture.width, wallTexture.height), new Vector2(0.5f, 0.5f), 1024);
        }
        
        // 3. Generate Map
        floorTm.ClearAllTiles();
        wallTm.ClearAllTiles();

        for (int x = -width/2; x < width/2; x++)
        {
            for (int y = -height/2; y < height/2; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                
                // Borders are walls
                if (x == -width/2 || x == width/2 -1 || y == -height/2 || y == height/2 -1)
                {
                    wallTm.SetTile(pos, wallTile);
                }
                else
                {
                    floorTm.SetTile(pos, floorTile);
                }
            }
        }
        
        Debug.Log("Dungeon Generated with Separate Layers!");
        
        PlaceTorches();
        SpawnPlayer();
        SpawnEnemies();
        FixCamera();
    }

    [Header("Enemies")]
    public GameObject enemyPrefab;
    public int enemyCount = 10;
    
    void SpawnEnemies()
    {
        CleanupOldEnemies();
        GameObject container = new GameObject("Enemies");

        // 1. Try to load Enemy Prefab
        GameObject loadedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Enemy.prefab");
        if (loadedPrefab == null)
             loadedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy.prefab");

        // 2. Load ALL Sprites from enemy.png
        List<Sprite> enemySprites = new List<Sprite>();
        string enemyPngPath = "Assets/enemy.png";
        
        Object[] objs = AssetDatabase.LoadAllAssetsAtPath(enemyPngPath);
        
        foreach(Object obj in objs)
        {
            if (obj is Sprite s)
            {
                enemySprites.Add(s);
            }
        }
        
        // Sort by Y-position DESCENDING (top of sheet = Row 0), then by X (left to right)
        // This ensures correct row order for standard sprite sheets
        enemySprites.Sort((a, b) => {
            // Primary sort: Y descending (higher Y = earlier in list = lower row index)
            int yCompare = b.rect.y.CompareTo(a.rect.y);
            if (yCompare != 0) return yCompare;
            // Secondary sort: X ascending (left to right within row)
            return a.rect.x.CompareTo(b.rect.x);
        });
        
        int columns = 8; // Standard sheet has 8 frames per direction
        int rowCount = enemySprites.Count / columns;
        Debug.Log($"Spawning {enemyCount} enemies with {enemySprites.Count} sprites ({rowCount} rows x {columns} cols)");
        
        // Spawn Loop
        for (int i = 0; i < enemyCount; i++)
        {
            // Position in circle around player (0,0)
            float angle = i * (360f / enemyCount);
            float radius = 4.5f + Random.Range(-0.5f, 0.5f);
            float x = Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
            float y = Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
            Vector3 pos = new Vector3(x, y, 0f); 

            // Instantiate
            GameObject enemy;
            if (loadedPrefab != null)
                enemy = (GameObject)PrefabUtility.InstantiatePrefab(loadedPrefab);
            else
                enemy = new GameObject($"Enemy_{i}");
            
            enemy.transform.position = pos;
            enemy.transform.SetParent(container.transform);
            enemy.name = $"Enemy_{i}";
            
            // Setup SpriteRenderer
            SpriteRenderer sr = enemy.GetComponent<SpriteRenderer>();
            if (sr == null) sr = enemy.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 10;
            
            // Setup EnemyAnimator with sprites
            EnemyAnimator anim = enemy.GetComponent<EnemyAnimator>();
            if (anim == null) anim = enemy.AddComponent<EnemyAnimator>();
            
            if (enemySprites.Count >= columns)
            {
                anim.Initialize(enemySprites, columns);
                
                // Configure row mappings based on typical sprite sheet layout:
                // Row 0 = Front/Down, Row 1 = Left, Row 2 = Right, Row 3 = Back/Up
                // If sheet differs, these can be adjusted
                anim.ConfigureRows(
                    down: 0,  // Front facing (walking toward camera)
                    left: 1,  // Side left
                    right: 2, // Side right  
                    up: 3     // Back facing (walking away from camera)
                );
                
                // Set initial sprite (facing down/toward player)
                sr.sprite = enemySprites[0];
            }
            else if (enemySprites.Count > 0)
            {
                // Fallback: just one sprite
                sr.sprite = enemySprites[0];
            }
        }
    }
    
    // Helper for sorting
    private static int GetSuffix(string name)
    {
        int lastUnderscore = name.LastIndexOf('_');
        if (lastUnderscore >= 0 && lastUnderscore < name.Length - 1)
        {
            if (int.TryParse(name.Substring(lastUnderscore + 1), out int result))
            {
                return result;
            }
        }
        return 0;
    }


    // Helper to clean up old stuff
    private void CleanupOldEnemies()
    {
         // Prevent Inspector from trying to inspect destroyed objects
         Selection.activeGameObject = null;
         
         GameObject container = GameObject.Find("Enemies");
         if (container != null) DestroyImmediate(container);
         
         // Also search for any object with "banana" in name just in case
         // Note: FindObjectsOfTypeAll is generally editor only but we are in editor tool
         var allObjects = GameObject.FindObjectsOfType<GameObject>();
         foreach(var go in allObjects)
         {
             if (go.name.ToLower().Contains("banana") || go.name.ToLower().Contains("enemy"))
             {
                 // Keep the generated container if we just made it (not relevant here as called before)
                 // But safer to just destory everything that looks like an old enemy
                 if (go.scene.isLoaded) // ensure in scene
                     DestroyImmediate(go);
             }
         }
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
        
        // Fit Height - NO, User wants Follow
        // cameraComponent.orthographicSize = height / 2f; 
        // Center camera initially
        cam.transform.position = new Vector3(0, 0, -10);

        Camera cameraComponent = cam.GetComponent<Camera>();
        cameraComponent.orthographic = true;
        cameraComponent.orthographicSize = 6f; // Zoomed in for detail 
        cameraComponent.clearFlags = CameraClearFlags.SolidColor;
        cameraComponent.backgroundColor = Color.black;

        // Ensure URP Data (rest of code...)
        var ura = cam.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
        if (ura == null) ura = cam.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
        ura.renderType = UnityEngine.Rendering.Universal.CameraRenderType.Base;

        // Add Follow Script
        SimpleCameraFollow follow = cam.GetComponent<SimpleCameraFollow>();
        if (follow == null) follow = cam.AddComponent<SimpleCameraFollow>();
        
        follow.target = GameObject.Find("Player")?.transform;
        follow.useDeadzone = true;
        follow.deadzoneSize = new Vector2(0.5f, 0.5f); // Very small deadzone for immediate follow
        follow.useBounds = true;
        follow.minBounds = new Vector2(-width/2f + 5, -height/2f + 5); // Add margin
        follow.maxBounds = new Vector2(width/2f - 5, height/2f - 5);
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
        light.color = new Color(1f, 0.7f, 0.4f, 1f); // Warm Orange
        light.intensity = 1.5f;
        light.pointLightOuterRadius = 6f;
        light.pointLightInnerRadius = 1f;
    }
}
