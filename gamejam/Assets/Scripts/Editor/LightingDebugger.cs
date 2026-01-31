using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class LightingDebugger : MonoBehaviour
{
    [UnityEditor.MenuItem("Tools/Debug Lighting")]
    public static void DebugLights()
    {
        Debug.Log($"Current Render Pipeline: {GraphicsSettings.currentRenderPipeline?.GetType().Name ?? "Null (Built-in)"}");
        
        var globalLights = FindObjectsOfType<Light2D>();
        Debug.Log($"Found {globalLights.Length} Light2D objects.");
        
        foreach(var light in globalLights)
        {
             Debug.Log($"Light: {light.name}, Type: {light.lightType}, Intensity: {light.intensity}, Color: {light.color}");
        }

        if (GraphicsSettings.currentRenderPipeline == null)
        {
            Debug.LogError("URP is NOT active! 2D Lights will not work.");
        }
    }
}
