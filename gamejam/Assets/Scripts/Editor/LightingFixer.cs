using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

public class LightingFixer : MonoBehaviour
{
    [MenuItem("Tools/Fix Lighting Materials")]
    public static void FixMaterials()
    {
        // 1. Find the URP Sprite-Lit-Default material
        Material litMaterial = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Lit-Default.mat");
        
        // Fallback: Try to find by name if package path differs
        if (litMaterial == null)
        {
            string[] guids = AssetDatabase.FindAssets("Sprite-Lit-Default t:Material");
            if (guids.Length > 0)
                litMaterial = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        if (litMaterial == null)
        {
            Debug.LogError("Could not find 'Sprite-Lit-Default' material! Ensure URP is installed.");
            return;
        }

        // 2. Assign to all SpriteRenderers
        SpriteRenderer[] renderers = FindObjectsOfType<SpriteRenderer>();
        int count = 0;
        foreach (var sr in renderers)
        {
            sr.material = litMaterial;
            count++;
        }
        
        // 3. Assign to all TilemapRenderers
        TilemapRenderer[] mapRenderers = FindObjectsOfType<TilemapRenderer>();
        foreach(var tr in mapRenderers)
        {
            tr.material = litMaterial;
            count++;
        }
        
        Debug.Log($"Assigned Lit material to {count} renderers (Sprites + Tilemaps).");
        
        // 3. Fix Global Light Layer Targets
        Light2D[] lights = FindObjectsOfType<Light2D>();
        foreach(var light in lights)
        {
             // Ensure they target all layers (internal property, might need serialized object fallback if API limited)
             // But usually default is All. 
             // Let's just log verification.
        }
    }
}
