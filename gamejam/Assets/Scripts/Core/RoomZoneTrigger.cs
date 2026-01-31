using UnityEngine;

/// <summary>
/// Trigger that notifies RoomManager when player enters a room.
/// Auto-equips the corresponding mask.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class RoomZoneTrigger : MonoBehaviour
{
    [Header("Room Settings")]
    public RoomManager.RoomType RoomType = RoomManager.RoomType.None;
    
    private void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if player entered
        if (other.CompareTag("Player") || other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            // Notify RoomManager
            if (RoomManager.Instance != null)
            {
                RoomManager.Instance.EnterRoom(RoomType);
            }
            else
            {
                Debug.LogWarning("[RoomZoneTrigger] RoomManager.Instance is null!");
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Color c = RoomType switch
            {
                RoomManager.RoomType.Fear => Color.cyan,
                RoomManager.RoomType.Hate => Color.red,
                RoomManager.RoomType.Sorrow => Color.blue,
                RoomManager.RoomType.Guilt => new Color(0.5f, 0.3f, 0.3f),
                RoomManager.RoomType.Bedroom => Color.white,
                _ => Color.gray
            };
            c.a = 0.2f;
            
            Gizmos.color = c;
            Gizmos.DrawCube(col.bounds.center, col.bounds.size);
            Gizmos.color = new Color(c.r, c.g, c.b, 1f);
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
    }
}
