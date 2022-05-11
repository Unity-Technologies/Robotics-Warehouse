using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class EditorTemplateScene
{
    const string k_TemplateScenePath = "Packages/com.unity.robotics.warehouse/Scenes/Warehouse.unity";
    const string k_NewSceneRoot = "Scenes";
    const string k_NewScenePath = "Warehouse.unity";

    /// <summary>
    /// Generate and open new Assets/Scenes/WarehouseTemplate.unity scene with WarehouseManager and instantiated
    /// Warehouse GameObjects
    /// </summary>
    [MenuItem("Warehouse/Create Template Scene")]
    static void Generate()
    {
        if (!AssetDatabase.IsValidFolder($"Assets/{k_NewSceneRoot}"))
        {
            AssetDatabase.CreateFolder("Assets", k_NewSceneRoot);
        }
        string uniquePath = AssetDatabase.GenerateUniqueAssetPath($"Assets/{k_NewSceneRoot}/{k_NewScenePath}");
        AssetDatabase.CopyAsset(k_TemplateScenePath, uniquePath);
        EditorSceneManager.OpenScene(uniquePath);
        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(uniquePath));
        Selection.activeObject = GameObject.Find("WarehouseManager");
    }
}
