using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.Simulation.Warehouse;
using Unity.Robotics.SimulationControl;

public class EditorWarehouseGeneration 
{
    static WarehouseManager warehouseManager;
    static GameObject parentGenerated;
    static PerceptionRandomizationScenario scenario;
    
    [MenuItem("Simulation/Generate Warehouse")]
    static void Generate()
    {
        DeleteWarehouse();

        warehouseManager = GameObject.FindObjectOfType<WarehouseManager>();
        if (warehouseManager == null) 
        {
            Debug.LogWarning("No WarehouseManager script in the scene!");
            return;
        }

        parentGenerated = GameObject.Find("GeneratedWarehouse");
        if (parentGenerated == null)
        {
            parentGenerated = new GameObject("GeneratedWarehouse");

            GenerateWarehouse();

            GenerateShelves();
            GenerateStations();
        }

        IncrementIteration();
    }

    [MenuItem("Simulation/Increment Iteration")]
    static void IncrementIteration() 
    {
        if (scenario == null)
            scenario = GameObject.FindObjectOfType<PerceptionRandomizationScenario>();
        scenario.Randomize();
    }

    [MenuItem("Simulation/Reset Warehouse")]
    static void DeleteWarehouse() 
    {
        var warehouse = GameObject.Find("GeneratedWarehouse");
        var spawned = GameObject.Find("FloorBoxes");
        if (warehouse != null)
        {
            Object.DestroyImmediate(warehouse);
            Object.DestroyImmediate(spawned);
        }
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
        var warehouse = GameObject.Find("GeneratedWarehouse");
        var spawned = GameObject.Find("FloorBoxes");
        if (warehouse != null)
        {
            // Ensure path is unique; save to Assets/Resources/Prefabs/SavedWarehouses
            string localPath = "Assets/Resources/Prefabs/SavedWarehouses/" + warehouse.name + ".prefab";
            localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);
            PrefabUtility.SaveAsPrefabAssetAndConnect(warehouse, localPath, InteractionMode.UserAction);
        }
    }

    static void SaveRuntimeWarehouse()
    {
        var warehouse = GameObject.Find("GeneratedWarehouse");
        var children = warehouse.GetComponentsInChildren<Renderer>();

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
        string localPath = "Assets/Resources/Prefabs/SavedWarehouses/" + warehouse.name + ".prefab";
        localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);
        PrefabUtility.SaveAsPrefabAssetAndConnect(warehouse, localPath, InteractionMode.UserAction);
    }

    [MenuItem("Simulation/Generate Warehouse", true, 100)]
    static bool ValidateGenerate()
    {
        return (GameObject.FindObjectOfType<WarehouseManager>() != null);
    }

    [MenuItem("Simulation/Increment Iteration", true, 100)]
    static bool ValidateIncrement()
    {
        return (scenario != null && GameObject.Find("GeneratedWarehouse") != null);
    }

    [MenuItem("Simulation/Reset Warehouse", true, 100)]
    static bool ValidateReset()
    {
        return (GameObject.Find("GeneratedWarehouse") != null);
    }

    [MenuItem("Simulation/Save Warehouse", true, 100)]
    static bool ValidateSave()
    {
        return (GameObject.Find("GeneratedWarehouse") != null);
    }

    [CustomEditor(typeof(WarehouseManager))]
    public class GenerateButton : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var warehouse = (WarehouseManager)target;
            scenario = GameObject.FindObjectOfType<PerceptionRandomizationScenario>();
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

    static void GenerateWarehouse() 
    {
        // Find component mesh in prefab
        var floorTile = Resources.Load<GameObject>("Prefabs/WarehouseParts/Floor01");
        var ceilingTile = Resources.Load<GameObject>("Prefabs/WarehouseParts/Ceiling01");
        var wallTile = Resources.Load<GameObject>("Prefabs/WarehouseParts/WallPanel01");
        var lightTile = Resources.Load<GameObject>("Prefabs/WarehouseParts/LightFixture001");
        var skylight = Resources.Load<GameObject>("Prefabs/WarehouseParts/Skylight01");
        var column = Resources.Load<GameObject>("Prefabs/WarehouseParts/Column01");
        var glulam = Resources.Load<GameObject>("Prefabs/WarehouseParts/Glulam01");

        var floorTileSize = floorTile.GetComponent<Renderer>().bounds.size;
        var wallTileSize = wallTile.GetComponent<Renderer>().bounds.size;
        var ceilingTileSize = ceilingTile.GetComponent<Renderer>().bounds.size;
        var columnSize = column.GetComponent<Renderer>().bounds.size;
        var glulamSize = glulam.GetComponent<Renderer>().bounds.size;

        var parentWarehouse = new GameObject("Warehouse");

        // Create empty GameObject parents
        var parentTransform = parentWarehouse.transform;
        parentTransform.parent = parentGenerated.transform;
        var floorsParent = new GameObject("Floors").transform;
        floorsParent.parent = parentTransform;
        var ceilingParent = new GameObject("Ceilings").transform;
        ceilingParent.parent = parentTransform;
        var wallParent = new GameObject("Walls").transform;
        wallParent.parent = parentTransform;

        // Calculate offsets
        Vector3 floorScaled = floorTileSize * 0.75f;
        Vector3 floorOffset = new Vector3(warehouseManager.appParam.width/2, 0, warehouseManager.appParam.length/2) + floorScaled;

        // Instantiate warehouse shell
        for (var i = 1; i < warehouseManager.appParam.width / floorTileSize.x + 1; i++)
        {
            for (var j = 1; j < warehouseManager.appParam.length / floorTileSize.z + 1; j++)
            {
                // Instantiate floors
                Vector3 fPos = Vector3.Scale(new Vector3(i, 0, j), floorTileSize);
                GameObject floor = PrefabUtility.InstantiatePrefab(floorTile) as GameObject;
                floor.AddComponent<MaterialRandomizerTag>();
                floor.transform.position = fPos - floorOffset;
                floor.transform.parent = floorsParent;

                // Ceilings
                GameObject ceiling = PrefabUtility.InstantiatePrefab(ceilingTile) as GameObject;
                ceiling.transform.position = new Vector3(floor.transform.position.x, wallTileSize.y, floor.transform.position.z);
                ceiling.transform.parent = ceilingParent;

                // Skylight
                GameObject sky = PrefabUtility.InstantiatePrefab(skylight) as GameObject;
                sky.transform.position = new Vector3(ceiling.transform.position.x, ceiling.transform.position.y + ceilingTileSize.y, ceiling.transform.position.z);
                sky.transform.parent = ceilingParent;

                // Walls (on edges only)
                if (i == 1) 
                {
                    GameObject wall = PrefabUtility.InstantiatePrefab(wallTile) as GameObject;
                    wall.transform.position = new Vector3(floor.transform.position.x - floorTileSize.x/2, wallTileSize.y/2, floor.transform.position.z);
                    wall.transform.parent = wallParent;
                }
                if (i > warehouseManager.appParam.width / floorTileSize.x) 
                {
                    GameObject wall = PrefabUtility.InstantiatePrefab(wallTile) as GameObject;
                    wall.transform.position = new Vector3(floor.transform.position.x + floorTileSize.x/2, wallTileSize.y/2, floor.transform.position.z);
                    wall.transform.parent = wallParent;
                }
                if (j == 1) 
                {
                    GameObject wall = PrefabUtility.InstantiatePrefab(wallTile) as GameObject;
                    wall.transform.position = new Vector3(floor.transform.position.x, wallTileSize.y/2, floor.transform.position.z - floorTileSize.z/2);
                    wall.transform.localRotation = WarehouseManager.hRot;
                    wall.transform.parent = wallParent;
                }
                if (j > warehouseManager.appParam.length / floorTileSize.z) 
                {
                    GameObject wall = PrefabUtility.InstantiatePrefab(wallTile) as GameObject;
                    wall.transform.position = new Vector3(floor.transform.position.x, wallTileSize.y/2, floor.transform.position.z + floorTileSize.z/2);
                    wall.transform.localRotation = WarehouseManager.hRot;
                    wall.transform.parent = wallParent;
                }

                // Lights
                if (i < warehouseManager.appParam.width / floorTileSize.x && j < warehouseManager.appParam.length / floorTileSize.z)
                {
                    // Only create every other light
                    if ((i % 2 == 0) && (j % 2 == 0))
                    {
                        GameObject light = PrefabUtility.InstantiatePrefab(lightTile) as GameObject;
                        light.transform.position = new Vector3(ceiling.transform.position.x + floorTileSize.x/2, ceiling.transform.position.y - glulamSize.y, ceiling.transform.position.z + floorTileSize.z/2);
                        light.transform.parent = ceilingParent;
                    }
                    // Create one glulam per tile row
                    if (i == 1)
                    {
                        GameObject g = PrefabUtility.InstantiatePrefab(glulam) as GameObject;
                        g.transform.position = new Vector3(0, ceiling.transform.position.y - glulamSize.y/2, ceiling.transform.position.z + floorTileSize.z/2);
                        g.transform.localScale = new Vector3((warehouseManager.appParam.width / 30f) * g.transform.localScale.x, g.transform.localScale.y, g.transform.localScale.z);
                        g.transform.parent = ceilingParent;
                    }
                }
            }
        }
    }

    // Instantiate empty shelf racks
    static void GenerateShelves() 
    {
        // Calculate distance between shelves
        float r = (warehouseManager.appParam.shelfRows > 1) ? warehouseManager.appParam.length / (warehouseManager.appParam.shelfRows + 1.0f) : warehouseManager.appParam.length / 2.0f;
        float c = (warehouseManager.appParam.shelfCols > 1) ? warehouseManager.appParam.width / (warehouseManager.appParam.shelfCols + 1.0f) : warehouseManager.appParam.width / 2.0f;

        var shelfSize = warehouseManager.shelfPrefab.transform.Find("Rack").GetComponent<Renderer>().bounds.size;

        if (shelfSize.y >= c || shelfSize.x >= r)
        {
            Debug.LogWarning("Shelves will overlap with no space to navigate.");
        }

        var shelfParent = new GameObject("Shelves").transform;
        shelfParent.parent = parentGenerated.transform;

        for (var i = 1; i < warehouseManager.appParam.shelfCols + 1; i++)
        {
            for (var j = 1; j < warehouseManager.appParam.shelfRows + 1; j++)
            {
                GameObject o = PrefabUtility.InstantiatePrefab(warehouseManager.shelfPrefab) as GameObject;
                o.transform.position = new Vector3(c * i - (warehouseManager.appParam.width / 2), 0, r * j - (warehouseManager.appParam.length / 2));
                o.transform.parent = shelfParent;
            }
        }
    } 

    // Generate placeholder "cubes" for "stations," for visual effect
    static void GenerateStations()
    {
        var station = Resources.Load<GameObject>("Prefabs/WarehouseParts/Station");
        var cur = new Vector3(-warehouseManager.appParam.width/2, 0.1f, -warehouseManager.appParam.length/2);

        var parentStations = new GameObject("Stations").transform;
        parentStations.parent = parentGenerated.transform;

        while (cur.x < (warehouseManager.appParam.width/2f))
        {
            GameObject s = PrefabUtility.InstantiatePrefab(station) as GameObject;
            s.transform.position = cur;
            s.transform.parent = parentStations;
            cur.x += 2;
        }
    }
}
