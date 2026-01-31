using UnityEngine;

/// <summary>
/// Pickup that auto-equips a mask when player enters.
/// Placed at dungeon room entrances.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class MaskPickup : MonoBehaviour
{
    [Header("Mask Settings")]
    public MaskType MaskToEquip = MaskType.None;
    
    [Header("Visual")]
    [SerializeField] private bool _destroyOnPickup = true;
    [SerializeField] private float _floatSpeed = 2f;
    [SerializeField] private float _floatAmount = 0.2f;
    
    private Vector3 _startPos;
    private SpriteRenderer _sr;
    
    private void Start()
    {
        _startPos = transform.position;
        _sr = GetComponent<SpriteRenderer>();
        
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
        
        // Set color based on mask type
        if (_sr != null)
        {
            _sr.color = GetMaskColor();
        }
    }
    
    private void Update()
    {
        // Floating animation
        float yOffset = Mathf.Sin(Time.time * _floatSpeed) * _floatAmount;
        transform.position = _startPos + new Vector3(0f, yOffset, 0f);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.SetMask(MaskToEquip);
                Debug.Log($"[MaskPickup] Player equipped {MaskToEquip} mask!");
                
                // TODO: Add visual/audio feedback
                
                if (_destroyOnPickup)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
    
    private Color GetMaskColor()
    {
        return MaskToEquip switch
        {
            MaskType.Fear => new Color(0.3f, 0.3f, 0.5f, 0.8f),
            MaskType.Hate => new Color(0.7f, 0.2f, 0.2f, 0.8f),
            MaskType.Sorrow => new Color(0.3f, 0.4f, 0.7f, 0.8f),
            MaskType.Guilt => new Color(0.5f, 0.4f, 0.4f, 0.8f),
            _ => Color.white
        };
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = GetMaskColor();
        Gizmos.DrawSphere(transform.position, 0.5f);
    }
}
