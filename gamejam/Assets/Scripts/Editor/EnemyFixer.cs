using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Editor utility to fix enemy issues.
/// Menu: Tools > Fix Enemies
/// </summary>
public static class EnemyFixer
{
    [MenuItem("Tools/Load SampleScene")]
    public static void LoadSampleScene()
    {
        // Save current prefab stage if any
        var stage = PrefabStageUtility.GetCurrentPrefabStage();
        if (stage != null)
        {
            StageUtility.GoToMainStage();
        }
        
        // Load SampleScene
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        Debug.Log("[EnemyFixer] Loaded SampleScene!");
    }
    
    [MenuItem("Tools/Fix Enemy Movement")]
    public static void FixEnemyMovement()
    {
        // First ensure we're in the main scene
        LoadSampleScene();
        
        int fixedCount = 0;
        
        // Find all enemies with Rigidbody2D
        var allObjects = GameObject.FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None);
        
        foreach (var rb in allObjects)
        {
            // Check if it has any enemy-like component
            bool isEnemy = rb.GetComponent<ShadowStalkerAI>() != null ||
                          rb.GetComponent<RageGolemAI>() != null ||
                          rb.GetComponent<GhostGuardAI>() != null ||
                          rb.GetComponent<ChainDemonAI>() != null ||
                          rb.GetComponent<EnemyBase>() != null;
            
            if (isEnemy)
            {
                // Fix physics settings
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.gravityScale = 0f;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                rb.linearDamping = 0f;
                rb.angularDamping = 0f;
                
                fixedCount++;
                Debug.Log($"[EnemyFixer] Fixed: {rb.gameObject.name}");
            }
        }
        
        // Also check animators - reset them if stuck
        var animators = GameObject.FindObjectsByType<Animator>(FindObjectsSortMode.None);
        foreach (var anim in animators)
        {
            if (anim.GetComponent<EnemyBase>() != null)
            {
                anim.Rebind();
                anim.Update(0f);
            }
        }
        
        if (fixedCount > 0)
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log($"[EnemyFixer] Fixed {fixedCount} enemies! Remember to save the scene.");
        }
        else
        {
            Debug.LogWarning("[EnemyFixer] No enemies found to fix!");
        }
    }
}
#endif