using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.Simulation.Warehouse;
using Unity.Robotics.SimulationControl;
using UnityEngine.Perception.Randomization.Randomizers.SampleRandomizers;

public class EditorWarehouseGeneration 
{
    static WarehouseManager warehouseManager;
    static GameObject parentGenerated;
    static List<GameObject> shelves;
    static List<GameObject> _paths = new List<GameObject>();
    
    [MenuItem("Simulation/Generate Warehouse")]
    static void Generate()
    {
        DeleteWarehouse();

        warehouseManager = GameObject.FindObjectOfType<WarehouseManager>();
        parentGenerated = new GameObject("GeneratedWarehouse");
        GenerateWarehouse();
        shelves = GenerateShelves();

        var test = GameObject.FindObjectOfType<PerceptionRandomizationScenario>();
        test.Randomize();
    }

    static void GenerateWarehouse() 
    {
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
        var parentTransform = parentWarehouse.transform;
        parentTransform.parent = parentGenerated.transform;

        for (var i = 1; i < warehouseManager.appParam.m_width / floorTileSize.x + 1; i++)
        {
            for (var j = 1; j < warehouseManager.appParam.m_length / floorTileSize.z + 1; j++)
            {
                GameObject floor = PrefabUtility.InstantiatePrefab(floorTile) as GameObject;
                floor.AddComponent<MaterialRandomizerTag>();
                floor.transform.position = new Vector3(i * floorTileSize.x - (warehouseManager.appParam.m_width / 2 + floorTileSize.x * 0.75f), 0, j * floorTileSize.z - (warehouseManager.appParam.m_length / 2 + floorTileSize.z * 0.75f));
                floor.transform.parent = parentTransform;

                GameObject ceiling = PrefabUtility.InstantiatePrefab(ceilingTile) as GameObject;
                ceiling.transform.position = new Vector3(floor.transform.position.x, wallTileSize.y, floor.transform.position.z);
                ceiling.transform.parent = parentTransform;

                GameObject sky = PrefabUtility.InstantiatePrefab(skylight) as GameObject;
                sky.transform.position = new Vector3(ceiling.transform.position.x, ceiling.transform.position.y + ceilingTileSize.y, ceiling.transform.position.z);
                sky.transform.parent = parentTransform;

                if (i == 1) 
                {
                    GameObject wall = PrefabUtility.InstantiatePrefab(wallTile) as GameObject;
                    wall.transform.position = new Vector3(floor.transform.position.x - floorTileSize.x/2, wallTileSize.y/2, floor.transform.position.z);
                    wall.transform.parent = parentTransform;
                }
                if (i > warehouseManager.appParam.m_width / floorTileSize.x) 
                {
                    GameObject wall = PrefabUtility.InstantiatePrefab(wallTile) as GameObject;
                    wall.transform.position = new Vector3(floor.transform.position.x + floorTileSize.x/2, wallTileSize.y/2, floor.transform.position.z);
                    wall.transform.parent = parentTransform;
                }
                if (j == 1) 
                {
                    GameObject wall = PrefabUtility.InstantiatePrefab(wallTile) as GameObject;
                    wall.transform.position = new Vector3(floor.transform.position.x, wallTileSize.y/2, floor.transform.position.z - floorTileSize.z/2);
                    wall.transform.localRotation = WarehouseManager._hRot;
                    wall.transform.parent = parentTransform;
                }
                if (j > warehouseManager.appParam.m_length / floorTileSize.z) 
                {
                    GameObject wall = PrefabUtility.InstantiatePrefab(wallTile) as GameObject;
                    wall.transform.position = new Vector3(floor.transform.position.x, wallTileSize.y/2, floor.transform.position.z + floorTileSize.z/2);
                    wall.transform.localRotation = WarehouseManager._hRot;
                    wall.transform.parent = parentTransform;
                }

                // Don't create edge lights
                if (i <  warehouseManager.appParam.m_width / floorTileSize.x && j < warehouseManager.appParam.m_length / floorTileSize.z)
                {
                    if ((i % 2 == 0) && (j % 2 == 0))
                    {
                        GameObject light = PrefabUtility.InstantiatePrefab(lightTile) as GameObject;
                        light.transform.position = new Vector3(ceiling.transform.position.x + floorTileSize.x/2, ceiling.transform.position.y - glulamSize.y, ceiling.transform.position.z + floorTileSize.z/2);
                        light.transform.parent = parentTransform;
                    }
                    if (i == 1)
                    {
                        GameObject g = PrefabUtility.InstantiatePrefab(glulam) as GameObject;
                        g.transform.position = new Vector3(0, ceiling.transform.position.y - glulamSize.y/2, ceiling.transform.position.z + floorTileSize.z/2);
                        g.transform.localScale = new Vector3((warehouseManager.appParam.m_width / 30f) * g.transform.localScale.x, g.transform.localScale.y, g.transform.localScale.z);
                        g.transform.parent = parentTransform;
                    }
                }
            }
        }
    }

    static List<GameObject> GenerateShelves() 
    {
        var shelves = new List<GameObject>();

        float r = (warehouseManager.appParam.m_rows > 1) ? warehouseManager.appParam.m_length / (warehouseManager.appParam.m_rows + 1.0f) : warehouseManager.appParam.m_length / 2.0f;
        float c = (warehouseManager.appParam.m_cols > 1) ? warehouseManager.appParam.m_width / (warehouseManager.appParam.m_cols + 1.0f) : warehouseManager.appParam.m_width / 2.0f;

        var shelfSize = warehouseManager.m_shelfPrefab.transform.Find("Rack").GetComponent<Renderer>().bounds.size;

        if (shelfSize.y >= c){
            Debug.LogWarning("Shelf columns will overlap with no space to navigate.");
        }
        if (shelfSize.x >= r){
            Debug.LogWarning("Shelf rows will overlap with no space to navigate.");
        }

        // Generate paths between shelves
        GameObject v;
        GameObject h;

        var shelfParent = new GameObject("Shelves").transform;
        shelfParent.parent = parentGenerated.transform;
        var pathParent = new GameObject("Paths").transform;
        pathParent.parent = parentGenerated.transform;

        for (var i = 1; i < warehouseManager.appParam.m_cols + 1; i++){
            bool colPath = false;
            for (var j = 1; j < warehouseManager.appParam.m_rows + 1; j++){
                GameObject o = PrefabUtility.InstantiatePrefab(warehouseManager.m_shelfPrefab) as GameObject;
                o.transform.position = new Vector3(c * i - (warehouseManager.appParam.m_width / 2), 0, r * j - (warehouseManager.appParam.m_length / 2));
                o.transform.parent = shelfParent;
                shelves.Add(o);

                // need to instantiate only once per row and once per column
                if (!colPath && i < warehouseManager.appParam.m_cols){
                    v = PrefabUtility.InstantiatePrefab(warehouseManager.m_roadPrefab) as GameObject;
                    v.transform.position = new Vector3(o.transform.position.x + (c/2), WarehouseManager._pathHeight, 0);
                    v.transform.localScale = new Vector3(r / 30f, v.transform.localScale.y, warehouseManager.appParam.m_length / 10f);
                    v.transform.parent = pathParent;
                    _paths.Add(v);
                    colPath = true;
                }
                if (i == 1 && j < warehouseManager.appParam.m_rows){
                    h = PrefabUtility.InstantiatePrefab(warehouseManager.m_roadPrefab) as GameObject;
                    h.transform.position = new Vector3(0, WarehouseManager._pathHeight, o.transform.position.z + (r/2));
                    h.transform.localRotation = WarehouseManager._hRot;
                    h.transform.localScale = new Vector3(c / 30f, h.transform.localScale.y, warehouseManager.appParam.m_width / 10f);
                    h.transform.parent = pathParent;
                    _paths.Add(h);
                }
            }
        }

        // Station path
        h = PrefabUtility.InstantiatePrefab(warehouseManager.m_roadPrefab) as GameObject;
        h.transform.position = new Vector3(0, WarehouseManager._pathHeight, -(warehouseManager.appParam.m_length / 2));
        h.transform.localScale = new Vector3(c / 30f, h.transform.localScale.y, warehouseManager.appParam.m_width / 10f);
        h.transform.localRotation = WarehouseManager._hRot;
        h.transform.parent = pathParent;

        // First path
        v = PrefabUtility.InstantiatePrefab(warehouseManager.m_roadPrefab) as GameObject;
        v.transform.position = new Vector3(-(warehouseManager.appParam.m_width / 2) + c/2, WarehouseManager._pathHeight, 0);
        v.transform.localScale = new Vector3(r / 30f, v.transform.localScale.y, warehouseManager.appParam.m_length / 10f);
        v.transform.parent = pathParent;

        h = PrefabUtility.InstantiatePrefab(warehouseManager.m_roadPrefab) as GameObject;
        h.transform.position = new Vector3(0, WarehouseManager._pathHeight, -(warehouseManager.appParam.m_length / 2) + r/2);
        h.transform.localScale = new Vector3(c / 30f, h.transform.localScale.y, warehouseManager.appParam.m_width / 10f);
        h.transform.localRotation = WarehouseManager._hRot;
        h.transform.parent = pathParent;
        _paths.Add(v);
        _paths.Add(h);

        // Last path
        v = PrefabUtility.InstantiatePrefab(warehouseManager.m_roadPrefab) as GameObject;
        v.transform.position = new Vector3(warehouseManager.appParam.m_width / 2 - (c/2), WarehouseManager._pathHeight, 0);
        v.transform.localScale = new Vector3(r / 30f, v.transform.localScale.y, warehouseManager.appParam.m_length / 10f);
        v.transform.parent = pathParent;

        h = PrefabUtility.InstantiatePrefab(warehouseManager.m_roadPrefab) as GameObject;
        h.transform.position = new Vector3(0, WarehouseManager._pathHeight, warehouseManager.appParam.m_length /2 - (r/2));
        h.transform.localScale = new Vector3(c / 30f, h.transform.localScale.y, warehouseManager.appParam.m_width / 10f);
        h.transform.localRotation = WarehouseManager._hRot;
        h.transform.parent = pathParent;
        _paths.Add(v);
        _paths.Add(h);

        return shelves;
    }

    [MenuItem("Simulation/Reset Warehouse", true)]
    static bool DeleteWarehouse() 
    {
        var warehouse = GameObject.Find("GeneratedWarehouse");
        if (warehouse != null)
        {
            Object.DestroyImmediate(warehouse);
            return true;
        }
        return false;
    }

    [CustomEditor(typeof(WarehouseManager))]
    public class GenerateButton : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            WarehouseManager warehouse = (WarehouseManager)target;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate Warehouse"))
            {
                Generate();
            }
            if (GUILayout.Button("Delete Warehouse"))
            {
                DeleteWarehouse();
            }
            GUILayout.EndHorizontal();
        }
    }

    [CustomEditor(typeof(PerceptionRandomizationScenario))]
    public class IterateButton : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            PerceptionRandomizationScenario scenario = (PerceptionRandomizationScenario)target;
            if (GUILayout.Button("Increment iteration"))
            {
                scenario.Randomize();   
            }
        }
    }
}
