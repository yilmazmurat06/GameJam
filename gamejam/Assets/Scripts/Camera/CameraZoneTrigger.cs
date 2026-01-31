using UnityEngine;

/// <summary>
/// Trigger zone that switches camera confiners when player enters.
/// Place at room transitions to change camera bounds.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class CameraZoneTrigger : MonoBehaviour
{
    [Header("Room Settings")]
    [SerializeField] private string _roomName = "Room";
    [SerializeField] private Collider2D _cameraBounds;
    
    [Header("References")]
    [SerializeField] private CameraConfinerSwitcher _confinerSwitcher;
    
    [Header("Settings")]
    [SerializeField] private bool _triggerRoomTransitionEvent = true;
    
    private void Awake()
    {
        // Ensure trigger is set
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
        
        // Find confiner switcher if not assigned
        if (_confinerSwitcher == null)
        {
            _confinerSwitcher = FindFirstObjectByType<CameraConfinerSwitcher>();
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if player entered
        if (other.CompareTag("Player"))
        {
            Debug.Log($"[CameraZoneTrigger] Player entered zone: {_roomName}");
            
            // Switch camera bounds
            if (_confinerSwitcher != null && _cameraBounds != null)
            {
                _confinerSwitcher.SwitchConfiner(_cameraBounds);
            }
            
            // Fire room transition event
            if (_triggerRoomTransitionEvent)
            {
                GameEvents.TriggerRoomTransition(_roomName);
                
                // Special case: entering dungeon
                if (_roomName.ToLower().Contains("dungeon"))
                {
                    GameEvents.TriggerDungeonEntered();
                }
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        // Draw zone visualization
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.3f);
            Gizmos.DrawCube(col.bounds.center, col.bounds.size);
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
    }
}