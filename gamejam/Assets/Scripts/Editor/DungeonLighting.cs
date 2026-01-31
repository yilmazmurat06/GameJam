using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.Universal; // Requires URP package

public class DungeonLighting : MonoBehaviour
{
    [MenuItem("Tools/Setup Dungeon Lighting")]
    public static void Setup()
    {
        // 1. Create Global Darkness
        GameObject globalLightGO = GameObject.Find("Global Light 2D");
        if (globalLightGO == null)
        {
            globalLightGO = new GameObject("Global Light 2D");
        }
        
        Light2D light = globalLightGO.GetComponent<Light2D>();
        if (light == null) light = globalLightGO.AddComponent<Light2D>();
        
        light.lightType = Light2D.LightType.Global;
        light.color = new Color(0.1f, 0.1f, 0.2f, 1f); // Dark Blue/Purple
        light.intensity = 0.3f; // Dark mood
        
        Debug.Log("Dungeon Lighting Setup Complete!");
    }
}
