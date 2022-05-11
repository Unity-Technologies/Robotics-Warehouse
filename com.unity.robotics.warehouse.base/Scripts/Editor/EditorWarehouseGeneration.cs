using System;
using System.IO;
using Unity.Robotics.PerceptionRandomizers.Shims;
using Unity.Simulation.Warehouse;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class EditorWarehouseGeneration
{
    const string k_SaveWarehousePath = "Assets/Prefabs/";

    [MenuItem("Warehouse/Generate Warehouse")]
    public static void Generate()
    {
        WarehouseManager.Instance.Destroy();
        WarehouseManager.Instance.Generate();
        WarehouseManager.Instance.IncrementIteration();
    }

    [MenuItem("Warehouse/Increment Iteration")]
    static void IncrementIteration()
    {
        WarehouseManager.Instance.IncrementIteration();
    }

    [MenuItem("Warehouse/Reset Warehouse")]
    static void DeleteWarehouse()
    {
        WarehouseManager.Instance.Destroy();
    }

    [MenuItem("Warehouse/Save Warehouse")]
    static void SaveWarehouse()
    {
        if (Application.isPlaying)
        {
            SaveRuntimeWarehouse();
        }
        else
        {
            SaveEditorWarehouse();
        }
    }

    static void SaveEditorWarehouse()
    {
        var spawned = GameObject.Find("FloorBoxes");
        if (WarehouseManager.Instance.ParentGenerated != null)
        {
            // Ensure path is unique
            if (!Directory.Exists(k_SaveWarehousePath))
            {
                Directory.CreateDirectory(k_SaveWarehousePath);
            }
            var localPath = $"{k_SaveWarehousePath}{WarehouseManager.Instance.ParentGenerated.name}.prefab";
            localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);
            PrefabUtility.SaveAsPrefabAssetAndConnect(WarehouseManager.Instance.ParentGenerated, localPath,
                InteractionMode.UserAction);
        }
    }

    static void SaveRuntimeWarehouse()
    {
        var children = WarehouseManager.Instance.ParentGenerated.GetComponentsInChildren<Renderer>();

        // Assign instances of materials from Resources
        foreach (var e in children)
        {
            if (e.materials.Length == 1)
            {
                var matName = e.material.name.Replace(" (Instance)", "");
                e.sharedMaterial = Resources.Load<Material>($"Materials/{matName}");
            }
            else
            {
                var cpyMat = e.materials;
                for (var i = 0; i < cpyMat.Length; i++)
                {
                    var matName = cpyMat[i].name.Replace(" (Instance)", "");
                    cpyMat[i] = Resources.Load<Material>($"Materials/{matName}");
                }

                e.materials = cpyMat;
            }
        }

        // Save warehouse
        if (!Directory.Exists(k_SaveWarehousePath))
        {
            Directory.CreateDirectory(k_SaveWarehousePath);
        }
        var localPath = $"{k_SaveWarehousePath}{WarehouseManager.Instance.ParentGenerated.name}.prefab";
        localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);
        PrefabUtility.SaveAsPrefabAssetAndConnect(WarehouseManager.Instance.ParentGenerated, localPath,
            InteractionMode.UserAction);
    }

    [MenuItem("Warehouse/Generate Warehouse", true, 100)]
    static bool ValidateGenerate()
    {
        return Object.FindObjectOfType<WarehouseManager>() != null;
    }

    [MenuItem("Warehouse/Increment Iteration", true, 100)]
    static bool ValidateIncrement()
    {
        if (WarehouseManager.Instance == null)
        {
            return false;
        }

        return WarehouseManager.Instance.ScenarioShim != null && WarehouseManager.Instance.ParentGenerated != null;
    }

    [MenuItem("Warehouse/Reset Warehouse", true, 100)]
    static bool ValidateReset()
    {
        if (WarehouseManager.Instance == null)
        {
            return false;
        }

        return WarehouseManager.Instance.ParentGenerated != null;
    }

    [MenuItem("Warehouse/Save Warehouse", true, 100)]
    static bool ValidateSave()
    {
        if (WarehouseManager.Instance == null)
        {
            return false;
        }

        return WarehouseManager.Instance.ParentGenerated != null;
    }

    [CustomEditor(typeof(WarehouseManager))]
    public class GenerateButton : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (WarehouseManager.Instance == null)
            {
                return;
            }

            WarehouseManager.Instance.ScenarioShim = FindObjectOfType<ScenarioShim>();
            var selected = -1;
            selected = GUILayout.SelectionGrid(selected,
                new[] { "Generate", "Increment iteration", "Save prefab", "Delete" }, 2);

            switch (selected)
            {
                case 0:
                    Generate();
                    break;
                case 1:
                    IncrementIteration();
                    break;
                case 2:
                    SaveWarehouse();
                    break;
                case 3:
                    DeleteWarehouse();
                    break;
            }
        }
    }
}
