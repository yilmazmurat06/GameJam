using UnityEngine;
using UnityEditor;

public class EnemyBuilder : MonoBehaviour
{
    [MenuItem("Tools/Build Banana Enemy")]
    public static void BuildBanana()
    {
        // Create GO
        GameObject go = new GameObject("Enemy_Banana");
        
        // Sprite Renderer
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        // Load default sprite
        string[] guids = AssetDatabase.FindAssets("banana_idle_down"); 
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        sr.sortingOrder = 0; // Default layer

        // Animator
        Animator anim = go.AddComponent<Animator>();
        string controllerPath = "Assets/animations/_enemy_banana.controller";
        RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
        if (controller != null)
        {
            anim.runtimeAnimatorController = controller;
        }
        else
        {
            Debug.LogError("Controller not found at " + controllerPath);
        }

        // Physics
        BoxCollider2D box = go.AddComponent<BoxCollider2D>();
        box.size = new Vector2(0.8f, 0.8f); // Approx size
        
        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Set Layer to "Enemy" (Layer 7)
        go.layer = LayerMask.NameToLayer("Enemy");

        // Logic
        go.AddComponent<Health>();
        EnemyBase enemy = go.AddComponent<EnemyBase>();
        
        // Reflection to set protected field _playerLayer
        // Or better: ensure we can set it. EnemyBase fields are serialized.
        // We can modify the SerializedObject of the new component.
        
        SerializedObject so = new SerializedObject(enemy);
        so.FindProperty("_playerLayer").intValue = 1 << LayerMask.NameToLayer("Player");
        // Also set detection range if needed
        so.FindProperty("_detectionRange").floatValue = 5f;
        so.ApplyModifiedProperties();
        
        // Save as Prefab
        string prefabPath = "Assets/Prefabs/Enemy_Banana.prefab";
        PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        
        // Cleanup
        DestroyImmediate(go);
        
        Debug.Log("Created Enemy_Banana prefab at " + prefabPath);
    }
}
