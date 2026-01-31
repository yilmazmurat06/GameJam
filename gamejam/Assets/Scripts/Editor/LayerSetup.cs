using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class LayerSetup
{
    static LayerSetup()
    {
        CreateLayers();
    }

    [MenuItem("Tools/Setup Layers")]
    public static void CreateLayers()
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");
        
        bool changed = false;
        
        // Ensure Layer 6 is Player
        // Unity defaults: 0-5 are Builtin. 6, 7 are usually empty or free.
        // Let's set Layer 6 = "Player", Layer 7 = "Enemy"
        // Note: "Player" is often Layer 3 or 6 in Unity defaults? 
        // Builtin: 0=Default, 1=TransparentFX, 2=Ignore Raycast, 3=Player(sometimes), 4=Water, 5=UI
        // Let's check if "Player" exists first.
        
        if (!LayerExists("Player"))
        {
            if (SetLayer(layers, 6, "Player")) changed = true;
            else if (SetLayer(layers, 3, "Player")) changed = true; // Fallback
        }
        
        if (!LayerExists("Enemy"))
        {
            if (SetLayer(layers, 7, "Enemy")) changed = true;
        }

        if (changed)
        {
            tagManager.ApplyModifiedProperties();
            Debug.Log("Layers 'Player' and 'Enemy' created successfully.");
        }
    }

    private static bool LayerExists(string layerName)
    {
        return LayerMask.NameToLayer(layerName) != -1;
    }

    private static bool SetLayer(SerializedProperty layers, int index, string name)
    {
        if (index < 0 || index >= layers.arraySize) return false;
        
        SerializedProperty element = layers.GetArrayElementAtIndex(index);
        
        // Only set if empty or specifically overwriting unused
        if (string.IsNullOrEmpty(element.stringValue) || element.stringValue == name)
        {
            element.stringValue = name;
            return true;
        }
        return false;
    }
}
