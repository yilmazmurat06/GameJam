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
        light.color = new Color(0.8f, 0.8f, 0.9f, 1f); // Neutral Daylight/Bright Dungeon
        light.intensity = 0.8f; // Brighter "Normal" look
        
        Debug.Log("Dungeon Lighting Setup Complete!");
    }
}
