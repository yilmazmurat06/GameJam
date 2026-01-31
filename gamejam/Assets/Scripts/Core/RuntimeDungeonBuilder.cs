using UnityEngine;

/// <summary>
/// Runtime dungeon layout generator.
/// Creates the horizontal room structure for the psychological dungeon.
/// </summary>
public class RuntimeDungeonBuilder : MonoBehaviour
{
    [Header("Room Settings")]
    public Vector2 RoomSize = new Vector2(20f, 15f);
    public Vector2 CorridorSize = new Vector2(8f, 5f);
    
    [Header("Colors")]
    public Color FearColor = new Color(0.1f, 0.1f, 0.15f, 1f);
    public Color HateColor = new Color(0.3f, 0.1f, 0.1f, 1f);
    public Color SorrowColor = new Color(0.1f, 0.15f, 0.3f, 1f);
    public Color GuiltColor = new Color(0.15f, 0.1f, 0.1f, 1f);
    public Color BedroomColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    public Color CorridorColor = new Color(0.1f, 0.1f, 0.1f, 1f);
    
    [Header("Generate on Start")]
    public bool GenerateOnStart = false;
    
    private void Start()
    {
        if (GenerateOnStart)
        {
            GenerateLayout();
        }
    }
    
    public void GenerateLayout()
    {
        // Create parent
        GameObject layout = new GameObject("DungeonLayout");
        layout.transform.position = Vector3.zero;
        
        float xOffset = 0f;
        
        // Room 1: Fear
        CreateRoom(layout.transform, "FearRoom", xOffset, FearColor, RoomManager.RoomType.Fear);
        xOffset += RoomSize.x;
        
        // Corridor 1
        CreateCorridor(layout.transform, "Corridor1", xOffset);
        xOffset += CorridorSize.x;
        
        // Room 2: Hate
        CreateRoom(layout.transform, "HateRoom", xOffset, HateColor, RoomManager.RoomType.Hate);
        xOffset += RoomSize.x;
        
        // Corridor 2
        CreateCorridor(layout.transform, "Corridor2", xOffset);
        xOffset += CorridorSize.x;
        
        // Room 3: Sorrow
        CreateRoom(layout.transform, "SorrowRoom", xOffset, SorrowColor, RoomManager.RoomType.Sorrow);
        xOffset += RoomSize.x;
        
        // Corridor 3
        CreateCorridor(layout.transform, "Corridor3", xOffset);
        xOffset += CorridorSize.x;
        
        // Room 4: Guilt
        CreateRoom(layout.transform, "GuiltRoom", xOffset, GuiltColor, RoomManager.RoomType.Guilt);
        xOffset += RoomSize.x;
        
        // Final: Bedroom
        CreateRoom(layout.transform, "Bedroom", xOffset, BedroomColor, RoomManager.RoomType.Bedroom);
        
        Debug.Log($"[RuntimeDungeonBuilder] Generated layout! Total width: {xOffset + RoomSize.x} units");
    }
    
    private void CreateRoom(Transform parent, string name, float xPos, Color color, RoomManager.RoomType roomType)
    {
        GameObject room = new GameObject(name);
        room.transform.SetParent(parent);
        room.transform.localPosition = new Vector3(xPos + RoomSize.x / 2f, 0f, 0f);
        
        // Floor
        GameObject floor = CreateFloor(room.transform, RoomSize, color);
        
        // Walls
        CreateWalls(room.transform);
        
        // Room Zone Trigger
        GameObject zone = new GameObject("RoomZone");
        zone.transform.SetParent(room.transform);
        zone.transform.localPosition = Vector3.zero;
        BoxCollider2D col = zone.AddComponent<BoxCollider2D>();
        col.size = RoomSize * 0.9f;
        col.isTrigger = true;
        RoomZoneTrigger trigger = zone.AddComponent<RoomZoneTrigger>();
        trigger.RoomType = roomType;
        
        // Mask Pickup (at entrance)
        if (roomType != RoomManager.RoomType.Bedroom && roomType != RoomManager.RoomType.None)
        {
            CreateMaskPickup(room.transform, roomType);
        }
    }
    
    private void CreateCorridor(Transform parent, string name, float xPos)
    {
        GameObject corridor = new GameObject(name);
        corridor.transform.SetParent(parent);
        corridor.transform.localPosition = new Vector3(xPos + CorridorSize.x / 2f, 0f, 0f);
        
        // Floor
        CreateFloor(corridor.transform, CorridorSize, CorridorColor);
        
        // Walls (top and bottom only)
        float wallThickness = 1f;
        CreateWallSegment(corridor.transform, "WallTop", new Vector3(0f, CorridorSize.y / 2f + wallThickness / 2f, 0f), new Vector2(CorridorSize.x, wallThickness));
        CreateWallSegment(corridor.transform, "WallBottom", new Vector3(0f, -CorridorSize.y / 2f - wallThickness / 2f, 0f), new Vector2(CorridorSize.x, wallThickness));
    }
    
    private GameObject CreateFloor(Transform parent, Vector2 size, Color color)
    {
        GameObject floor = new GameObject("Floor");
        floor.transform.SetParent(parent);
        floor.transform.localPosition = Vector3.zero;
        
        SpriteRenderer sr = floor.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite();
        sr.color = color;
        sr.sortingOrder = -100;
        floor.transform.localScale = new Vector3(size.x, size.y, 1f);
        
        return floor;
    }
    
    private void CreateWalls(Transform parent)
    {
        float wallThickness = 1f;
        float doorOpening = 4f; // Size of door opening
        
        // Top wall
        CreateWallSegment(parent, "WallTop", new Vector3(0f, RoomSize.y / 2f + wallThickness / 2f, 0f), new Vector2(RoomSize.x + wallThickness * 2f, wallThickness));
        
        // Bottom wall
        CreateWallSegment(parent, "WallBottom", new Vector3(0f, -RoomSize.y / 2f - wallThickness / 2f, 0f), new Vector2(RoomSize.x + wallThickness * 2f, wallThickness));
        
        // Left wall segments (with door opening)
        float sideHeight = (RoomSize.y - doorOpening) / 2f;
        CreateWallSegment(parent, "WallLeftTop", new Vector3(-RoomSize.x / 2f - wallThickness / 2f, RoomSize.y / 4f + doorOpening / 4f, 0f), new Vector2(wallThickness, sideHeight));
        CreateWallSegment(parent, "WallLeftBottom", new Vector3(-RoomSize.x / 2f - wallThickness / 2f, -RoomSize.y / 4f - doorOpening / 4f, 0f), new Vector2(wallThickness, sideHeight));
        
        // Right wall segments (with door opening)
        CreateWallSegment(parent, "WallRightTop", new Vector3(RoomSize.x / 2f + wallThickness / 2f, RoomSize.y / 4f + doorOpening / 4f, 0f), new Vector2(wallThickness, sideHeight));
        CreateWallSegment(parent, "WallRightBottom", new Vector3(RoomSize.x / 2f + wallThickness / 2f, -RoomSize.y / 4f - doorOpening / 4f, 0f), new Vector2(wallThickness, sideHeight));
    }
    
    private void CreateWallSegment(Transform parent, string name, Vector3 localPos, Vector2 size)
    {
        GameObject wall = new GameObject(name);
        wall.transform.SetParent(parent);
        wall.transform.localPosition = localPos;
        
        int wallLayer = LayerMask.NameToLayer("Wall");
        if (wallLayer != -1) wall.layer = wallLayer;
        
        SpriteRenderer sr = wall.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite();
        sr.color = new Color(0.25f, 0.2f, 0.2f, 1f);
        sr.sortingOrder = 0;
        wall.transform.localScale = new Vector3(size.x, size.y, 1f);
        
        BoxCollider2D col = wall.AddComponent<BoxCollider2D>();
    }
    
    private void CreateMaskPickup(Transform parent, RoomManager.RoomType roomType)
    {
        GameObject pickup = new GameObject($"MaskPickup_{roomType}");
        pickup.transform.SetParent(parent);
        pickup.transform.localPosition = new Vector3(-RoomSize.x / 2f + 2f, 0f, 0f);
        
        SpriteRenderer sr = pickup.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite();
        sr.color = GetMaskColor(roomType);
        sr.sortingOrder = 10;
        pickup.transform.localScale = Vector3.one;
        
        BoxCollider2D col = pickup.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;
        col.isTrigger = true;
        
        MaskPickup mp = pickup.AddComponent<MaskPickup>();
        mp.MaskToEquip = GetMaskForRoomType(roomType);
    }
    
    private Color GetMaskColor(RoomManager.RoomType roomType)
    {
        return roomType switch
        {
            RoomManager.RoomType.Fear => new Color(0.3f, 0.3f, 0.5f, 0.8f),
            RoomManager.RoomType.Hate => new Color(0.7f, 0.2f, 0.2f, 0.8f),
            RoomManager.RoomType.Sorrow => new Color(0.3f, 0.4f, 0.7f, 0.8f),
            RoomManager.RoomType.Guilt => new Color(0.5f, 0.4f, 0.4f, 0.8f),
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
    
    private Sprite CreateSquareSprite()
    {
        // Create a simple 1x1 white pixel sprite
        Texture2D tex = new Texture2D(4, 4);
        Color[] colors = new Color[16];
        for (int i = 0; i < 16; i++) colors[i] = Color.white;
        tex.SetPixels(colors);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
    }
}
