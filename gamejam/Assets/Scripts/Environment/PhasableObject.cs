using UnityEngine;

/// <summary>
/// Phasable object for Sorrow Room.
/// Player can pass through when using Phase ability.
/// Use layer "Phasable" and assign in Unity.
/// </summary>
public class PhasableObject : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private Color _solidColor = new Color(0.5f, 0.5f, 0.6f, 1f);
    [SerializeField] private Color _phasableHintColor = new Color(0.6f, 0.6f, 0.8f, 0.8f);
    
    private SpriteRenderer _sr;
    private Collider2D _col;
    
    private void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
        _col = GetComponent<Collider2D>();
        
        // Set to Phasable layer
        int phasableLayer = LayerMask.NameToLayer("Phasable");
        if (phasableLayer != -1)
        {
            gameObject.layer = phasableLayer;
        }
        
        if (_sr != null)
            _sr.color = _solidColor;
    }
    
    private void Update()
    {
        // Visual hint when player is phasing
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            SorrowAbility sorrow = player.GetComponent<SorrowAbility>();
            if (sorrow != null && sorrow.IsActive)
            {
                if (_sr != null)
                    _sr.color = _phasableHintColor;
            }
            else
            {
                if (_sr != null)
                    _sr.color = _solidColor;
            }
        }
    }
}
