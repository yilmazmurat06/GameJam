using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Editor tool to generate the psychological dungeon scene layout.
/// Creates: Fear Room → Corridor → Hate Room → Corridor → Sorrow Room → Corridor → Guilt Room → Bedroom
/// </summary>
public class DungeonSceneBuilder : MonoBehaviour
{
    [Header("Room Settings")]
    [SerializeField] private Vector2 _roomSize = new Vector2(20f, 15f);
    [SerializeField] private Vector2 _corridorSize = new Vector2(8f, 5f);
    [SerializeField] private float _spacing = 0f;
    
    [Header("Sprites")]
    [SerializeField] private Sprite _floorSprite;
    [SerializeField] private Sprite _wallSprite;
    
    [Header("Memory Visuals (Tableaux)")]
    [SerializeField] private Sprite _fearMemoryImage;
    [SerializeField] private Sprite _hateMemoryImage;
    [SerializeField] private Sprite _sorrowMemoryImage;
    [SerializeField] private Sprite _guiltMemoryImage;
    
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject _fearEnemyPrefab;
    [SerializeField] private GameObject _hateEnemyPrefab;
    [SerializeField] private GameObject _sorrowEnemyPrefab;
    [SerializeField] private GameObject _guiltEnemyPrefab;

    [Header("Environment Prefabs")]
    [SerializeField] private GameObject _crackedWallPrefab;   // Hate Room
    [SerializeField] private GameObject _ironBarsPrefab;      // Sorrow Room
    [SerializeField] private GameObject _regretBlockPrefab;   // Guilt Room
    [SerializeField] private GameObject _pressurePlatePrefab; // Guilt Room

    [Header("Colors")]
    [SerializeField] private Color _fearColor = new Color(0.1f, 0.1f, 0.15f, 1f);
    [SerializeField] private Color _hateColor = new Color(0.3f, 0.1f, 0.1f, 1f);
    [SerializeField] private Color _sorrowColor = new Color(0.1f, 0.15f, 0.3f, 1f);
    [SerializeField] private Color _guiltColor = new Color(0.15f, 0.1f, 0.1f, 1f);
    [SerializeField] private Color _bedroomColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    [SerializeField] private Color _corridorColor = new Color(0.1f, 0.1f, 0.1f, 1f);
    
#if UNITY_EDITOR
    [ContextMenu("Generate Dungeon Layout")]
    public void GenerateDungeonLayout()
    {
        // Clear existing dungeon
        Transform existing = transform.Find("DungeonLayout");
        if (existing != null) DestroyImmediate(existing.gameObject);
        
        // Create parent
        GameObject layout = new GameObject("DungeonLayout");
        layout.transform.SetParent(transform);
        layout.transform.localPosition = Vector3.zero;
        
        float xOffset = 0f;
        
        // Room 1: Fear
        CreateRoom(layout.transform, "FearRoom", xOffset, _roomSize, _fearColor, RoomManager.RoomType.Fear);
        xOffset += _roomSize.x + _spacing;
        
        // Corridor 1
        CreateCorridor(layout.transform, "Corridor1", xOffset, _corridorSize, _corridorColor);
        xOffset += _corridorSize.x + _spacing;
        
        // Room 2: Hate
        CreateRoom(layout.transform, "HateRoom", xOffset, _roomSize, _hateColor, RoomManager.RoomType.Hate);
        xOffset += _roomSize.x + _spacing;
        
        // Corridor 2
        CreateCorridor(layout.transform, "Corridor2", xOffset, _corridorSize, _corridorColor);
        xOffset += _corridorSize.x + _spacing;
        
        // Room 3: Sorrow
        CreateRoom(layout.transform, "SorrowRoom", xOffset, _roomSize, _sorrowColor, RoomManager.RoomType.Sorrow);
        xOffset += _roomSize.x + _spacing;
        
        // Corridor 3
        CreateCorridor(layout.transform, "Corridor3", xOffset, _corridorSize, _corridorColor);
        xOffset += _corridorSize.x + _spacing;
        
        // Room 4: Guilt
        CreateRoom(layout.transform, "GuiltRoom", xOffset, _roomSize, _guiltColor, RoomManager.RoomType.Guilt);
        xOffset += _roomSize.x + _spacing;
        
        // Final: Bedroom
        CreateRoom(layout.transform, "Bedroom", xOffset, _roomSize, _bedroomColor, RoomManager.RoomType.Bedroom);
        
        Debug.Log($"[DungeonSceneBuilder] Generated layout with total width: {xOffset + _roomSize.x} units");
    }
    
    private void CreateRoom(Transform parent, string name, float xPos, Vector2 size, Color color, RoomManager.RoomType roomType)
    {
        GameObject room = new GameObject(name);
        room.transform.SetParent(parent);
        room.transform.localPosition = new Vector3(xPos + size.x / 2f, 0f, 0f);
        
        // Floor
        GameObject floor = CreateFloor(room.transform, size, color);
        
        // Walls
        CreateWalls(room.transform, size);
        
        // Camera Zone Trigger
        CreateCameraZone(room.transform, size, roomType);
        
        // Room Zone Trigger (for RoomManager)
        CreateRoomZone(room.transform, size, roomType);
        
        // Mask Pickup (at entrance)
        if (roomType != RoomManager.RoomType.Bedroom && roomType != RoomManager.RoomType.None)
        {
            CreateMaskPickup(room.transform, size, roomType);
        }
        
        // Memory Trigger (center of room)
        if (roomType != RoomManager.RoomType.Bedroom && roomType != RoomManager.RoomType.None)
        {
            CreateMemoryTrigger(room.transform, roomType);
        }
        
        // Spawn Enemy
        if (roomType != RoomManager.RoomType.Bedroom && roomType != RoomManager.RoomType.None)
        {
            CreateEnemy(room.transform, size, roomType);
        }
        
        // Spawn Environment Objects
        CreateEnvironmentObjects(room.transform, size, roomType);
    }
    
    private void CreateCorridor(Transform parent, string name, float xPos, Vector2 size, Color color)
    {
        GameObject corridor = new GameObject(name);
        corridor.transform.SetParent(parent);
        corridor.transform.localPosition = new Vector3(xPos + size.x / 2f, 0f, 0f);
        
        // Floor
        CreateFloor(corridor.transform, size, color);
        
        // Walls (top and bottom only)
        CreateCorridorWalls(corridor.transform, size);
    }
    
    private GameObject CreateFloor(Transform parent, Vector2 size, Color color)
    {
        GameObject floor = new GameObject("Floor");
        floor.transform.SetParent(parent);
        floor.transform.localPosition = Vector3.zero;
        
        SpriteRenderer sr = floor.AddComponent<SpriteRenderer>();
        sr.sprite = _floorSprite;
        sr.color = color;
        sr.sortingOrder = -100;
        
        // Scale to size
        if (_floorSprite != null)
        {
            float ppu = _floorSprite.pixelsPerUnit;
            float spriteWidth = _floorSprite.rect.width / ppu;
            float spriteHeight = _floorSprite.rect.height / ppu;
            floor.transform.localScale = new Vector3(size.x / spriteWidth, size.y / spriteHeight, 1f);
        }
        else
        {
            floor.transform.localScale = new Vector3(size.x, size.y, 1f);
        }
        
        return floor;
    }
    
    private void CreateWalls(Transform parent, Vector2 size)
    {
        float wallThickness = 1f;
        
        // Top wall
        CreateWallSegment(parent, "WallTop", new Vector3(0f, size.y / 2f + wallThickness / 2f, 0f), new Vector2(size.x + wallThickness * 2f, wallThickness));
        
        // Bottom wall
        CreateWallSegment(parent, "WallBottom", new Vector3(0f, -size.y / 2f - wallThickness / 2f, 0f), new Vector2(size.x + wallThickness * 2f, wallThickness));
        
        // Left wall (with door opening)
        CreateWallSegment(parent, "WallLeftTop", new Vector3(-size.x / 2f - wallThickness / 2f, size.y / 4f + 1f, 0f), new Vector2(wallThickness, size.y / 2f - 2f));
        CreateWallSegment(parent, "WallLeftBottom", new Vector3(-size.x / 2f - wallThickness / 2f, -size.y / 4f - 1f, 0f), new Vector2(wallThickness, size.y / 2f - 2f));
        
        // Right wall (with door opening)
        CreateWallSegment(parent, "WallRightTop", new Vector3(size.x / 2f + wallThickness / 2f, size.y / 4f + 1f, 0f), new Vector2(wallThickness, size.y / 2f - 2f));
        CreateWallSegment(parent, "WallRightBottom", new Vector3(size.x / 2f + wallThickness / 2f, -size.y / 4f - 1f, 0f), new Vector2(wallThickness, size.y / 2f - 2f));
    }
    
    private void CreateCorridorWalls(Transform parent, Vector2 size)
    {
        float wallThickness = 1f;
        
        // Top wall
        CreateWallSegment(parent, "WallTop", new Vector3(0f, size.y / 2f + wallThickness / 2f, 0f), new Vector2(size.x, wallThickness));
        
        // Bottom wall
        CreateWallSegment(parent, "WallBottom", new Vector3(0f, -size.y / 2f - wallThickness / 2f, 0f), new Vector2(size.x, wallThickness));
    }
    
    private void CreateWallSegment(Transform parent, string name, Vector3 localPos, Vector2 size)
    {
        GameObject wall = new GameObject(name);
        wall.transform.SetParent(parent);
        wall.transform.localPosition = localPos;
        wall.layer = LayerMask.NameToLayer("Wall");
        
        SpriteRenderer sr = wall.AddComponent<SpriteRenderer>();
        sr.sprite = _wallSprite;
        sr.color = new Color(0.2f, 0.15f, 0.15f, 1f);
        sr.sortingOrder = 0;
        
        // Scale
        if (_wallSprite != null)
        {
            float ppu = _wallSprite.pixelsPerUnit;
            float spriteWidth = _wallSprite.rect.width / ppu;
            float spriteHeight = _wallSprite.rect.height / ppu;
            wall.transform.localScale = new Vector3(size.x / spriteWidth, size.y / spriteHeight, 1f);
        }
        else
        {
            wall.transform.localScale = new Vector3(size.x, size.y, 1f);
        }
        
        // Collider
        BoxCollider2D col = wall.AddComponent<BoxCollider2D>();
    }
    
    private void CreateCameraZone(Transform parent, Vector2 size, RoomManager.RoomType roomType)
    {
        GameObject zone = new GameObject("CameraZone");
        zone.transform.SetParent(parent);
        zone.transform.localPosition = Vector3.zero;
        
        BoxCollider2D col = zone.AddComponent<BoxCollider2D>();
        col.size = size;
        col.isTrigger = true;
        
        // Add CameraZoneTrigger if it exists
        CameraZoneTrigger trigger = zone.AddComponent<CameraZoneTrigger>();
    }
    
    private void CreateRoomZone(Transform parent, Vector2 size, RoomManager.RoomType roomType)
    {
        GameObject zone = new GameObject("RoomZone");
        zone.transform.SetParent(parent);
        zone.transform.localPosition = Vector3.zero;
        
        BoxCollider2D col = zone.AddComponent<BoxCollider2D>();
        col.size = size * 0.9f; // Slightly smaller
        col.isTrigger = true;
        
        RoomZoneTrigger trigger = zone.AddComponent<RoomZoneTrigger>();
        trigger.RoomType = roomType;
    }
    
    private void CreateMaskPickup(Transform parent, Vector2 size, RoomManager.RoomType roomType)
    {
        GameObject pickup = new GameObject($"MaskPickup_{roomType}");
        pickup.transform.SetParent(parent);
        pickup.transform.localPosition = new Vector3(-size.x / 2f + 2f, 0f, 0f); // Near entrance
        
        SpriteRenderer sr = pickup.AddComponent<SpriteRenderer>();
        sr.color = GetMaskColor(roomType);
        sr.sortingOrder = 10;
        
        BoxCollider2D col = pickup.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;
        col.isTrigger = true;
        
        MaskPickup mp = pickup.AddComponent<MaskPickup>();
        mp.MaskToEquip = RoomManager.Instance != null ? RoomManager.Instance.GetMaskForRoom(roomType) : GetMaskForRoomType(roomType);
    }
    
    private void CreateMemoryTrigger(Transform parent, RoomManager.RoomType roomType)
    {
        GameObject memory = new GameObject($"MemoryTrigger_{roomType}");
        memory.transform.SetParent(parent);
        memory.transform.localPosition = Vector3.zero; // Center of room
        
        BoxCollider2D col = memory.AddComponent<BoxCollider2D>();
        col.size = new Vector2(3f, 3f);
        col.isTrigger = true;
        
        MemoryTrigger mt = memory.AddComponent<MemoryTrigger>();
        
        Sprite image = roomType switch
        {
            RoomManager.RoomType.Fear => _fearMemoryImage,
            RoomManager.RoomType.Hate => _hateMemoryImage,
            RoomManager.RoomType.Sorrow => _sorrowMemoryImage,
            RoomManager.RoomType.Guilt => _guiltMemoryImage,
            _ => null
        };
        
        string text = roomType switch
        {
            RoomManager.RoomType.Fear => "The face I loved... twisted by fear.",
            RoomManager.RoomType.Hate => "Silence broken by rage.",
            RoomManager.RoomType.Sorrow => "A future that never came to be.",
            RoomManager.RoomType.Guilt => "It was always you.",
            _ => ""
        };
        
        mt.Initialize(image, text);
    }
    
    private Color GetMaskColor(RoomManager.RoomType roomType)
    {
        return roomType switch
        {
            RoomManager.RoomType.Fear => new Color(0.2f, 0.2f, 0.4f, 1f),
            RoomManager.RoomType.Hate => new Color(0.6f, 0.2f, 0.2f, 1f),
            RoomManager.RoomType.Sorrow => new Color(0.3f, 0.3f, 0.6f, 1f),
            RoomManager.RoomType.Guilt => new Color(0.4f, 0.3f, 0.3f, 1f),
            _ => Color.white
        };
    }
    
    private MaskType GetMaskForRoomType(RoomManager.RoomType roomType)
    {
        return roomType switch
        {
            RoomManager.RoomType.Fear => MaskType.Fear,
            RoomManager.RoomType.Hate => MaskType.Hate,
            RoomManager.RoomType.Sorrow => MaskType.Sorrow,
            RoomManager.RoomType.Guilt => MaskType.Guilt,
            _ => MaskType.None
        };
    }
    
    private void CreateEnemy(Transform parent, Vector2 roomSize, RoomManager.RoomType roomType)
    {
        GameObject prefab = roomType switch
        {
            RoomManager.RoomType.Fear => _fearEnemyPrefab,
            RoomManager.RoomType.Hate => _hateEnemyPrefab,
            RoomManager.RoomType.Sorrow => _sorrowEnemyPrefab,
            RoomManager.RoomType.Guilt => _guiltEnemyPrefab,
            _ => null
        };
        
        if (prefab != null)
        {
            // Spawn 2-3 enemies per room
            int count = 2;
            for (int i = 0; i < count; i++)
            {
                GameObject enemy = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                enemy.transform.SetParent(parent);
                
                // Random position away from center
                float x = Random.Range(-roomSize.x / 3f, roomSize.x / 3f);
                float y = Random.Range(-roomSize.y / 3f, roomSize.y / 3f);
                enemy.transform.localPosition = new Vector3(x, y, 0f);
            }
        }
    }

    private void CreateEnvironmentObjects(Transform parent, Vector2 roomSize, RoomManager.RoomType roomType)
    {
        switch (roomType)
        {
            case RoomManager.RoomType.Hate:
                if (_crackedWallPrefab != null)
                {
                    // Spawn 3 destructible walls
                    for (int i = 0; i < 3; i++)
                    {
                        GameObject go = PrefabUtility.InstantiatePrefab(_crackedWallPrefab) as GameObject;
                        go.transform.SetParent(parent);
                        float x = Random.Range(-roomSize.x/3f, roomSize.x/3f);
                        float y = Random.Range(-roomSize.y/3f, roomSize.y/3f);
                        go.transform.localPosition = new Vector3(x, y, 0);
                    }
                }
                break;
                
            case RoomManager.RoomType.Sorrow:
                if (_ironBarsPrefab != null)
                {
                    // Spawn Iron Bars blocking path
                    GameObject go = PrefabUtility.InstantiatePrefab(_ironBarsPrefab) as GameObject;
                    go.transform.SetParent(parent);
                    go.transform.localPosition = new Vector3(0, 0, 0); // Center block
                    go.transform.localScale = new Vector3(1, 4, 1); // Tall bars
                }
                break;
                
            case RoomManager.RoomType.Guilt:
                if (_regretBlockPrefab != null)
                {
                    // Spawn Heavy Blocks
                    GameObject go = PrefabUtility.InstantiatePrefab(_regretBlockPrefab) as GameObject;
                    go.transform.SetParent(parent);
                    go.transform.localPosition = new Vector3(2, 0, 0);
                }
                if (_pressurePlatePrefab != null)
                {
                    // Spawn Pressure Plate
                    GameObject go = PrefabUtility.InstantiatePrefab(_pressurePlatePrefab) as GameObject;
                    go.transform.SetParent(parent);
                    go.transform.localPosition = new Vector3(-2, 0, 0);
                }
                break;
        }
    }
    
    [MenuItem("Tools/Generate Psychological Dungeon")]
    public static void GenerateFromMenu()
    {
        DungeonSceneBuilder builder = FindObjectOfType<DungeonSceneBuilder>();
        if (builder == null)
        {
            GameObject go = new GameObject("DungeonSceneBuilder");
            builder = go.AddComponent<DungeonSceneBuilder>();
        }
        builder.GenerateDungeonLayout();
    }
#endif
}
