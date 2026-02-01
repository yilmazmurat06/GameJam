using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using System.Collections.Generic;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Dungeon Settings")]
    public int width = 60;
    public int height = 60;
    public int minRoomSize = 6;
    public int maxRoomSize = 12;
    public int maxRooms = 10;
    public int corridorWidth = 2;
    
    [Header("References")]
    public Sprite floorSprite;
    public Sprite wallSprite;
    
    // Internal data
    private int[,] _dungeonMap; // 0 = wall, 1 = floor
    private List<RectInt> _rooms = new List<RectInt>();

    [MenuItem("Tools/Generate Dungeon")]
    public static void Generate()
    {
        // Clear selection to avoid Inspector errors when destroying objects
        Selection.activeGameObject = null;
        
        GameObject go = new GameObject("Dungeon Generator");
        DungeonGenerator generator = go.AddComponent<DungeonGenerator>();
        
        // Try to load default sprites if not assigned
        if (generator.floorSprite == null)
            generator.floorSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Environment/dungeon_floor.png");
            
        // Fallback to searching inside the tileset if specific files aren't found
        if (generator.floorSprite == null)
        {
             // Try to find first sprite in tileset
             Object[] assets = AssetDatabase.LoadAllAssetsAtPath("Assets/Dungeon_Tileset.png");
             foreach(Object asset in assets) {
                 if (asset is Sprite s) { generator.floorSprite = s; break; }
             }
        }
        
        if (generator.wallSprite == null)
            generator.wallSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Environment/dungeon_wall.png");

         if (generator.wallSprite == null && generator.floorSprite != null)
        {
             // If we found a floor in tileset, try to find a wall (e.g. 2nd sprite)
             Object[] assets = AssetDatabase.LoadAllAssetsAtPath("Assets/Dungeon_Tileset.png");
             int count = 0;
             foreach(Object asset in assets) {
                 if (asset is Sprite s) { 
                     count++;
                     if (count == 2) { generator.wallSprite = s; break; } // Pick 2nd sprite as wall
                 }
             }
        }
            
        generator.BuildDungeon();
        
        // changes: Do NOT destroy the generator. Let it persist so the user can modify settings.
        // DestroyImmediate(go); 
        Selection.activeGameObject = go;
    }

    [ContextMenu("Regenerate Dungeon")]
    public void Regenerate()
    {
        BuildDungeon();
    }

    [ContextMenu("Clear Map")]
    public void ClearMap()
    {
        // 1. Setup/Find Grid (needed to find tilemaps)
        GameObject gridGO = GameObject.Find("Grid");
        if (gridGO != null)
        {
            Tilemap[] maps = gridGO.GetComponentsInChildren<Tilemap>();
            foreach(var tm in maps) tm.ClearAllTiles();
            
            Transform torchContainer = gridGO.transform.Find("Torches");
            if (torchContainer != null) DestroyImmediate(torchContainer.gameObject);
            
            Transform enemies = GameObject.Find("Enemies")?.transform;
            if (enemies != null) DestroyImmediate(enemies.gameObject);
        }
        
        _rooms.Clear();
        Debug.Log("Dungeon Cleared for manual building!");
    }

    public void BuildDungeon()
    {
        // Initialize map as all walls
        _dungeonMap = new int[width, height];
        _rooms.Clear();
        
        // Generate rooms using BSP-like approach
        GenerateRooms();
        
        // Connect rooms with corridors
        ConnectRooms();
        
        // Setup tilemaps
        SetupTilemaps();
        
        // Ensure EnemyManager exists
        SetupEnemyManager();
        
        // Place torches
        PlaceTorches();
        
        // Spawn player in first room
        SpawnPlayer();
        
        // Spawn enemies in other rooms
        SpawnEnemies();
        
        // Setup camera
        FixCamera();
        
        Debug.Log($"Dungeon Generated with {_rooms.Count} rooms!");
    }
    
    private void SetupEnemyManager()
    {
        GameObject managerGO = GameObject.Find("EnemyManager");
        if (managerGO == null)
        {
            managerGO = new GameObject("EnemyManager");
            managerGO.AddComponent<EnemyManager>();
            Debug.Log("[DungeonGenerator] Created EnemyManager");
        }
    }
    
    private void GenerateRooms()
    {
        int attempts = 0;
        int maxAttempts = maxRooms * 10;
        
        while (_rooms.Count < maxRooms && attempts < maxAttempts)
        {
            attempts++;
            
            // Random room size
            int roomWidth = Random.Range(minRoomSize, maxRoomSize + 1);
            int roomHeight = Random.Range(minRoomSize, maxRoomSize + 1);
            
            // Random position (with margin from edges)
            int x = Random.Range(2, width - roomWidth - 2);
            int y = Random.Range(2, height - roomHeight - 2);
            
            RectInt newRoom = new RectInt(x, y, roomWidth, roomHeight);
            
            // Check if it overlaps with existing rooms (with padding)
            bool overlaps = false;
            foreach (RectInt existingRoom in _rooms)
            {
                RectInt paddedExisting = new RectInt(
                    existingRoom.x - 2, existingRoom.y - 2,
                    existingRoom.width + 4, existingRoom.height + 4
                );
                
                if (newRoom.Overlaps(paddedExisting))
                {
                    overlaps = true;
                    break;
                }
            }
            
            if (!overlaps)
            {
                // Carve out the room
                CarveRoom(newRoom);
                _rooms.Add(newRoom);
            }
        }
    }
    
    private void CarveRoom(RectInt room)
    {
        for (int x = room.x; x < room.x + room.width; x++)
        {
            for (int y = room.y; y < room.y + room.height; y++)
            {
                _dungeonMap[x, y] = 1; // Floor
            }
        }
    }
    
    private void ConnectRooms()
    {
        // Connect each room to the next room in the list
        for (int i = 0; i < _rooms.Count - 1; i++)
        {
            Vector2Int start = GetRoomCenter(_rooms[i]);
            Vector2Int end = GetRoomCenter(_rooms[i + 1]);
            
            // Create L-shaped corridor
            if (Random.value > 0.5f)
            {
                // Horizontal first, then vertical
                CreateHorizontalCorridor(start.x, end.x, start.y);
                CreateVerticalCorridor(start.y, end.y, end.x);
            }
            else
            {
                // Vertical first, then horizontal
                CreateVerticalCorridor(start.y, end.y, start.x);
                CreateHorizontalCorridor(start.x, end.x, end.y);
            }
        }
    }
    
    private Vector2Int GetRoomCenter(RectInt room)
    {
        return new Vector2Int(room.x + room.width / 2, room.y + room.height / 2);
    }
    
    private void CreateHorizontalCorridor(int x1, int x2, int y)
    {
        int minX = Mathf.Min(x1, x2);
        int maxX = Mathf.Max(x1, x2);
        
        for (int x = minX; x <= maxX; x++)
        {
            for (int w = 0; w < corridorWidth; w++)
            {
                int yPos = y + w - corridorWidth / 2;
                if (yPos >= 0 && yPos < height && x >= 0 && x < width)
                {
                    _dungeonMap[x, yPos] = 1;
                }
            }
        }
    }
    
    private void CreateVerticalCorridor(int y1, int y2, int x)
    {
        int minY = Mathf.Min(y1, y2);
        int maxY = Mathf.Max(y1, y2);
        
        for (int y = minY; y <= maxY; y++)
        {
            for (int w = 0; w < corridorWidth; w++)
            {
                int xPos = x + w - corridorWidth / 2;
                if (xPos >= 0 && xPos < width && y >= 0 && y < height)
                {
                    _dungeonMap[xPos, y] = 1;
                }
            }
        }
    }

    private void SetupTilemaps()
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
        Tile floorTile = ScriptableObject.CreateInstance<Tile>();
        if (floorSprite != null) floorTile.sprite = floorSprite;
        else Debug.LogWarning("Floor sprite is missing!");
        
        Tile wallTile = ScriptableObject.CreateInstance<Tile>();
        if (wallSprite != null) wallTile.sprite = wallSprite;
        else Debug.LogWarning("Wall sprite is missing!");
        
        // 3. Generate Map from _dungeonMap array
        floorTm.ClearAllTiles();
        wallTm.ClearAllTiles();
        
        // Center the dungeon at origin
        int offsetX = -width / 2;
        int offsetY = -height / 2;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int pos = new Vector3Int(x + offsetX, y + offsetY, 0);
                
                if (_dungeonMap[x, y] == 1)
                {
                    // Floor
                    floorTm.SetTile(pos, floorTile);
                }
                else
                {
                    // Check if this wall is adjacent to a floor (visible wall)
                    if (IsAdjacentToFloor(x, y))
                    {
                        wallTm.SetTile(pos, wallTile);
                    }
                    // Non-adjacent walls are just empty (black void)
                }
            }
        }
    }
    
    private bool IsAdjacentToFloor(int x, int y)
    {
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                
                int nx = x + dx;
                int ny = y + dy;
                
                if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                {
                    if (_dungeonMap[nx, ny] == 1)
                        return true;
                }
            }
        }
        return false;
    }

    [Header("Enemies")]
    public GameObject enemyPrefab;
    public int enemyCount = 10;
    
    // Enemy type distribution
    public enum SpawnableEnemyType { Mummy, Ranger, Charger }
    
    void SpawnEnemies()
    {
        CleanupOldEnemies();
        GameObject container = new GameObject("Enemies");

        // Load sprites from available enemy sprite sheets
        List<Sprite> cultistSprites = LoadSpriteSheet("Assets/Sprites/Enemies/cultist_enemy.png");
        List<Sprite> hoodedSprites = LoadSpriteSheet("Assets/Sprites/Enemies/hooded_enemy.png");
        List<Sprite> defaultSprites = LoadSpriteSheet("Assets/enemy.png");
        
        // Cultist sprite sheet: 18 columns, 24 rows
        // Hooded enemy: Only 2 sprites (not directional)
        // Default enemy: Unknown layout
        
        int offsetX = -width / 2;
        int offsetY = -height / 2;
        
        // Spawn enemies in rooms (skip the first room where player spawns)
        int enemiesSpawned = 0;
        int enemiesPerRoom = Mathf.CeilToInt((float)enemyCount / Mathf.Max(1, _rooms.Count - 1));
        
        Debug.Log($"Spawning {enemyCount} enemies across {_rooms.Count - 1} rooms");
        
        for (int roomIndex = 1; roomIndex < _rooms.Count && enemiesSpawned < enemyCount; roomIndex++)
        {
            RectInt room = _rooms[roomIndex];
            int enemiesToSpawn = Mathf.Min(enemiesPerRoom, enemyCount - enemiesSpawned);
            
            for (int i = 0; i < enemiesToSpawn; i++)
            {
                // Random position within room
                float x = Random.Range(room.x + 1, room.x + room.width - 1) + offsetX;
                float y = Random.Range(room.y + 1, room.y + room.height - 1) + offsetY;
                Vector3 pos = new Vector3(x, y, 0f);
                
                // Determine enemy type based on index (cycle through types)
                SpawnableEnemyType enemyType = (SpawnableEnemyType)(enemiesSpawned % 3);
                
                // Select appropriate sprite list for this enemy type
                List<Sprite> sprites;
                
                switch (enemyType)
                {
                    case SpawnableEnemyType.Mummy:
                        // Use cultist sprites 
                        sprites = cultistSprites.Count > 0 ? cultistSprites : defaultSprites;
                        break;
                        
                    case SpawnableEnemyType.Ranger:
                        // Use hooded enemy 
                        sprites = hoodedSprites.Count > 0 ? hoodedSprites : defaultSprites;
                        break;
                        
                    case SpawnableEnemyType.Charger:
                    default:
                        // Use default enemy sprites
                        sprites = defaultSprites.Count > 0 ? defaultSprites : cultistSprites;
                        break;
                }
            
            // Create enemy GameObject
            GameObject enemy = new GameObject($"{enemyType}_{enemiesSpawned}");
            enemy.transform.position = pos;
            enemy.transform.SetParent(container.transform);
            enemy.tag = "Enemy";
            
            // Add required components
            Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            
            CircleCollider2D col = enemy.AddComponent<CircleCollider2D>();
            col.radius = 0.3f;
            
            // Setup SpriteRenderer
            SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 10;
            
            // Apply URP Lit Material
            Material litMaterial = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Lit-Default.mat");
            if (litMaterial != null) sr.material = litMaterial;
            
            // Add Health component
            Health health = enemy.AddComponent<Health>();
            // Health will use default values
            
            // Add specific enemy type behavior
            switch (enemyType)
            {
                case SpawnableEnemyType.Mummy:
                    enemy.AddComponent<MummyEnemy>();
                    enemy.name = $"Mummy_{enemiesSpawned}";
                    break;
                    
                case SpawnableEnemyType.Ranger:
                    RangerEnemy ranger = enemy.AddComponent<RangerEnemy>();
                    enemy.name = $"Ranger_{enemiesSpawned}";
                    // Ranger needs a projectile prefab (will need to be assigned later)
                    break;
                    
                case SpawnableEnemyType.Charger:
                    enemy.AddComponent<ChargerEnemy>();
                    enemy.name = $"Charger_{enemiesSpawned}";
                    break;
            }
            
            // Setup EnemyAnimator with sprites
            // Use simple sprite flip for horizontal direction (no complex directional animation)
            // This avoids issues with unknown sprite sheet layouts
            EnemyAnimator anim = enemy.AddComponent<EnemyAnimator>();
            
            if (sprites != null && sprites.Count > 0)
            {
                // Just use first sprite - the EnemyAnimator will handle flipping
                sr.sprite = sprites[0];
                
                // For single sprites or unknown layouts, don't initialize directional animation
                // The EnemyAnimator will fall back to simple horizontal flipping
            }
            
            // Set layer for detection
            int enemyLayer = LayerMask.NameToLayer("Enemy");
            if (enemyLayer != -1) enemy.layer = enemyLayer;
            
            enemiesSpawned++;
            }
        }
        
        Debug.Log($"[DungeonGenerator] Spawned {enemiesSpawned} enemies across dungeon rooms!");
    }
    
    /// <summary>
    /// Helper to load sprites from a sprite sheet.
    /// Returns sprites sorted by name suffix for proper animation order.
    /// </summary>
    private List<Sprite> LoadSpriteSheet(string path)
    {
        List<Sprite> sprites = new List<Sprite>();
        Object[] objs = AssetDatabase.LoadAllAssetsAtPath(path);
        
        foreach (Object obj in objs)
        {
            if (obj is Sprite s)
            {
                sprites.Add(s);
            }
        }
        
        // Sort by sprite name suffix number (e.g., cultist_0, cultist_1, ...)
        sprites.Sort((a, b) => {
            int suffixA = GetSuffix(a.name);
            int suffixB = GetSuffix(b.name);
            return suffixA.CompareTo(suffixB);
        });
        
        return sprites;
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
         // Note: FindObjectsByType is the modern API (Unity 2023.1+)
         var allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
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
        
        // Spawn player in center of first room
        if (_rooms.Count > 0)
        {
            Vector2Int roomCenter = GetRoomCenter(_rooms[0]);
            int offsetX = -width / 2;
            int offsetY = -height / 2;
            player.transform.position = new Vector3(roomCenter.x + offsetX, roomCenter.y + offsetY, 0);
        }
        else
        {
            // Fallback to origin
            player.transform.position = Vector3.zero;
        }
        
        // Ensure Sorting Order high enough
        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
        if (sr != null) 
        {
            sr.sortingOrder = 10; // Above walls/floor/torches
            // Assign Lit Material too
             Material litMaterial = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Lit-Default.mat");
             if (litMaterial != null) sr.material = litMaterial;
        }

        Debug.Log($"Player spawned at {player.transform.position}");
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
        
        // Center camera on player spawn position
        if (_rooms.Count > 0)
        {
            Vector2Int roomCenter = GetRoomCenter(_rooms[0]);
            int offsetX = -width / 2;
            int offsetY = -height / 2;
            cam.transform.position = new Vector3(roomCenter.x + offsetX, roomCenter.y + offsetY, -10);
        }
        else
        {
            cam.transform.position = new Vector3(0, 0, -10);
        }

        Camera cameraComponent = cam.GetComponent<Camera>();
        cameraComponent.orthographic = true;
        cameraComponent.orthographicSize = 6f; // Zoomed in for detail 
        cameraComponent.clearFlags = CameraClearFlags.SolidColor;
        cameraComponent.backgroundColor = Color.black;

        // Ensure URP Data
        var ura = cam.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
        if (ura == null) ura = cam.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
        ura.renderType = UnityEngine.Rendering.Universal.CameraRenderType.Base;

        // Add Follow Script
        SimpleCameraFollow follow = cam.GetComponent<SimpleCameraFollow>();
        if (follow == null) follow = cam.AddComponent<SimpleCameraFollow>();
        
        follow.target = GameObject.Find("Player")?.transform;
        follow.useDeadzone = true;
        follow.deadzoneSize = new Vector2(0.5f, 0.5f);
        follow.useBounds = true;
        follow.minBounds = new Vector2(-width/2f + 3, -height/2f + 3);
        follow.maxBounds = new Vector2(width/2f - 3, height/2f - 3);
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

        Sprite torchSprite = null;
        if (torchTexture != null)
            torchSprite = Sprite.Create(torchTexture, new Rect(0,0,torchTexture.width, torchTexture.height), new Vector2(0.5f, 0.5f), 1024);

        int offsetX = -width / 2;
        int offsetY = -height / 2;

        // Place torches in each room
        foreach (RectInt room in _rooms)
        {
            // Place torches at corners and along walls of each room
            List<Vector3> torchPositions = new List<Vector3>
            {
                new Vector3(room.x + 1 + offsetX, room.y + 1 + offsetY, 0),
                new Vector3(room.x + room.width - 2 + offsetX, room.y + 1 + offsetY, 0),
                new Vector3(room.x + 1 + offsetX, room.y + room.height - 2 + offsetY, 0),
                new Vector3(room.x + room.width - 2 + offsetX, room.y + room.height - 2 + offsetY, 0)
            };
            
            foreach (Vector3 pos in torchPositions)
            {
                SpawnTorch(pos, container.transform, torchSprite);
            }
        }
    }

    void SpawnTorch(Vector3 position, Transform parent, Sprite sprite)
    {
        if (sprite == null) return;
        
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
