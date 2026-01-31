using UnityEngine;

/// <summary>
/// Automatically assigns bedroom sprites to furniture objects on Awake.
/// Attach to the Bedroom parent object.
/// </summary>
public class BedroomSpriteAssigner : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite _bookshelfSprite;
    [SerializeField] private Sprite _carpetSprite;
    [SerializeField] private Sprite _dressserSprite;
    [SerializeField] private Sprite _chairSprite;
    [SerializeField] private Sprite _wallBrickSprite;
    
    [Header("Auto-Find (if not assigned)")]
    [SerializeField] private bool _autoLoadFromResources = true;
    
    private void Awake()
    {
        if (_autoLoadFromResources)
        {
            LoadSpritesFromPath();
        }
        
        AssignSprites();
    }
    
    private void LoadSpritesFromPath()
    {
        // Load sprites from Sprites/Bedroom folder
        _bookshelfSprite = LoadSprite("Assets/Sprites/Bedroom/bookshelf.jpg");
        _carpetSprite = LoadSprite("Assets/Sprites/Bedroom/carpet.jpg");
        _dressserSprite = LoadSprite("Assets/Sprites/Bedroom/dresser.jpg");
        _chairSprite = LoadSprite("Assets/Sprites/Bedroom/chair.jpg");
        _wallBrickSprite = LoadSprite("Assets/Sprites/Bedroom/wall_brick.jpg");
    }
    
    private Sprite LoadSprite(string path)
    {
#if UNITY_EDITOR
        // In editor, load from AssetDatabase
        var texture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (texture != null)
        {
            // Try to get sprite sub-asset first
            var sprites = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var asset in sprites)
            {
                if (asset is Sprite sprite)
                    return sprite;
            }
            
            // If no sprite exists, create one from texture
            return Sprite.Create(texture, 
                new Rect(0, 0, texture.width, texture.height), 
                new Vector2(0.5f, 0.5f), 16f);
        }
#endif
        return null;
    }
    
    private void AssignSprites()
    {
        // Find and assign to children
        AssignToChild("Bookshelf", _bookshelfSprite);
        AssignToChild("Carpet", _carpetSprite);
        AssignToChild("Dresser", _dressserSprite);
        AssignToChild("Chair", _chairSprite);
        AssignToChild("WallDecor", _wallBrickSprite);
        
        // Also assign to existing furniture if present
        AssignToChild("Bed", _carpetSprite); // Use carpet pattern for bed
        AssignToChild("Nightstand", _bookshelfSprite); // Use bookshelf wood for nightstand
    }
    
    private void AssignToChild(string childName, Sprite sprite)
    {
        if (sprite == null) return;
        
        var child = transform.Find(childName);
        if (child != null)
        {
            var sr = child.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = sprite;
            }
        }
    }
    
#if UNITY_EDITOR
    [ContextMenu("Assign Sprites Now")]
    public void AssignSpritesInEditor()
    {
        LoadSpritesFromPath();
        AssignSprites();
        Debug.Log("Bedroom sprites assigned!");
    }
#endif
}