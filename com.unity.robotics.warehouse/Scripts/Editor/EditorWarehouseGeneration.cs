using Unity.Robotics.PerceptionRandomizers.Shims;
using UnityEngine;
using UnityEditor;
using Unity.Simulation.Warehouse;

public class EditorWarehouseGeneration
{
    [MenuItem("Simulation/Generate Warehouse")]
    static void Generate()
    {
        WarehouseManager.Instance.Destroy();
        WarehouseManager.Instance.Generate();
        WarehouseManager.Instance.IncrementIteration();
    }

    [MenuItem("Simulation/Increment Iteration")]
    static void IncrementIteration()
    {
        WarehouseManager.Instance.IncrementIteration();
    }

    [MenuItem("Simulation/Reset Warehouse")]
    static void DeleteWarehouse()
    {
        WarehouseManager.Instance.Destroy();
    }

    [MenuItem("Simulation/Save Warehouse")]
    static void SaveWarehouse()
    {
        if (Application.isPlaying)
            SaveRuntimeWarehouse();
        else
            SaveEditorWarehouse();
    }

    static void SaveEditorWarehouse()
    {
        var spawned = GameObject.Find("FloorBoxes");
        if (WarehouseManager.Instance.ParentGenerated != null)
        {
            // Ensure path is unique; save to Assets/Resources/Prefabs/SavedWarehouses
            string localPath = "Assets/Resources/Prefabs/SavedWarehouses/" + WarehouseManager.Instance.ParentGenerated.name + ".prefab";
            localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);
            PrefabUtility.SaveAsPrefabAssetAndConnect(WarehouseManager.Instance.ParentGenerated, localPath, InteractionMode.UserAction);
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
            else {
                var cpyMat = e.materials;
                for (int i = 0; i < cpyMat.Length; i++)
                {
                    var matName = cpyMat[i].name.Replace(" (Instance)", "");
                    cpyMat[i] = Resources.Load<Material>($"Materials/{matName}");
                }
                e.materials = cpyMat;
            }
        }

        // Ensure path is unique; save to Assets/Resources/Prefabs/SavedWarehouses
        string localPath = "Assets/Resources/Prefabs/SavedWarehouses/" + WarehouseManager.Instance.ParentGenerated.name + ".prefab";
        localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);
        PrefabUtility.SaveAsPrefabAssetAndConnect(WarehouseManager.Instance.ParentGenerated, localPath, InteractionMode.UserAction);
    }

    [MenuItem("Simulation/Generate Warehouse", true, 100)]
    static bool ValidateGenerate()
    {
        return (GameObject.FindObjectOfType<WarehouseManager>() != null);
    }

    [MenuItem("Simulation/Increment Iteration", true, 100)]
    static bool ValidateIncrement()
    {
        return (WarehouseManager.Instance.ScenarioShim != null && WarehouseManager.Instance.ParentGenerated != null);
    }

    [MenuItem("Simulation/Reset Warehouse", true, 100)]
    static bool ValidateReset()
    {
        return WarehouseManager.Instance.ParentGenerated != null;
    }

    [MenuItem("Simulation/Save Warehouse", true, 100)]
    static bool ValidateSave()
    {
        return WarehouseManager.Instance.ParentGenerated != null;
    }

    [CustomEditor(typeof(WarehouseManager))]
    public class GenerateButton : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            WarehouseManager.Instance.ScenarioShim = FindObjectOfType<ScenarioShim>();
            int selected = -1;
            selected = GUILayout.SelectionGrid(selected, new string[]{"Generate", "Increment iteration", "Save prefab", "Delete"}, 2);

            switch(selected)
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
