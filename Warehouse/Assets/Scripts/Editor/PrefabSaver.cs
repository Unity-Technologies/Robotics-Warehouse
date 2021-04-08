using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PrefabSaver
{
    [MenuItem("Simulation/Save Prefab")]
    static void CreatePrefab()
    {
        var generatedWarehouse = GameObject.FindGameObjectWithTag("Generated");
        var children = generatedWarehouse.GetComponentsInChildren<Renderer>();
        foreach (var e in children){
            if (e.materials.Length == 1){
                var matName = e.material.name.Replace(" (Instance)", "");
                e.sharedMaterial = Resources.Load<Material>($"Materials/{matName}");
            }
            else {
                var cpyMat = e.materials;
                for (int i = 0; i < cpyMat.Length; i++){
                    var matName = cpyMat[i].name.Replace(" (Instance)", "");
                    cpyMat[i] = Resources.Load<Material>($"Materials/{matName}");
                }
                e.materials = cpyMat;
            }
        }

        string localPath = "Assets/Prefabs/" + generatedWarehouse.name + ".prefab";
        // Make sure the file name is unique, in case an existing Prefab has the same name.
        localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);
        // Create the new Prefab.
        PrefabUtility.SaveAsPrefabAssetAndConnect(generatedWarehouse, localPath, InteractionMode.UserAction);
    }

    // Disable the menu item if no selection is in place.
    [MenuItem("Simulation/Save Prefab", true)]
    static bool ValidateCreatePrefab()
    {
        return GameObject.Find("GeneratedWarehouse") != null;
    }

}