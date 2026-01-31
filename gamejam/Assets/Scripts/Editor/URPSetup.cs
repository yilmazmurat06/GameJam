using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class URPSetup : MonoBehaviour
{
    [MenuItem("Tools/Setup URP")]
    public static void CreateAndAssign()
    {
        // 1. Check if URP is already active
        if (GraphicsSettings.defaultRenderPipeline != null)
        {
            Debug.Log($"Render Pipeline already active: {GraphicsSettings.defaultRenderPipeline.name}");
            return;
        }

        // 2. Create URP Asset
        // We need to create a Pipeline Asset and a 2D Renderer Data
        // This is complex via script API without internal access. 
        // Instead, let's try to finding an existing one or just warn.
        
        string[] guids = AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            RenderPipelineAsset asset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(path);
            GraphicsSettings.defaultRenderPipeline = asset;
            Debug.Log($"Assigned existing URP Asset: {path}");
            return;
        }

        Debug.LogError("No URP Asset found in project! Please create one via Assets > Create > Rendering > URP Asset (with 2D Renderer).");
    }
}
