using UnityEngine;

/// <summary>
/// Manages room state, mask auto-equipping, and room transitions.
/// Singleton pattern for global access.
/// </summary>
public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }
    
    public enum RoomType
    {
        None,
        Fear,      // KORKU
        Hate,      // NEFRET
        Sorrow,    // HÜZÜN
        Guilt,     // SUÇLULUK
        Bedroom    // Final room - unmasking
    }
    
    [Header("Current State")]
    [SerializeField] private RoomType _currentRoom = RoomType.None;
    
    public RoomType CurrentRoom => _currentRoom;
    
    // Events
    public System.Action<RoomType> OnRoomChanged;
    public System.Action<MaskType> OnMaskEquipped;
    
    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    /// <summary>
    /// Called when player enters a new room.
    /// </summary>
    public void EnterRoom(RoomType room)
    {
        if (room == _currentRoom) return;
        
        _currentRoom = room;
        Debug.Log($"[RoomManager] Entered {room} room");
        
        // Auto-equip corresponding mask
        MaskType maskToEquip = GetMaskForRoom(room);
        EquipMask(maskToEquip);
        
        OnRoomChanged?.Invoke(room);
    }
    
    /// <summary>
    /// Maps room type to corresponding mask.
    /// </summary>
    public MaskType GetMaskForRoom(RoomType room)
    {
        return room switch
        {
            RoomType.Fear => MaskType.Fear,
            RoomType.Hate => MaskType.Hate,
            RoomType.Sorrow => MaskType.Sorrow,
            RoomType.Guilt => MaskType.Guilt,
            RoomType.Bedroom => MaskType.None,
            _ => MaskType.None
        };
    }
    
    /// <summary>
    /// Equips a mask on the player.
    /// </summary>
    private void EquipMask(MaskType mask)
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.SetMask(mask);
            OnMaskEquipped?.Invoke(mask);
            Debug.Log($"[RoomManager] Equipped {mask} mask");
        }
    }
}
