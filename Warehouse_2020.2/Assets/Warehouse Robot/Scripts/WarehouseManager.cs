using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Perception.GroundTruth;
using Unity.Simulation;
using Random = UnityEngine.Random;

using Warehouse;

namespace Warehouse {
    public enum FloorType { PolishedCement, Hardwood, LowPlyCarpet, ShagCarpet }
    public enum LightingType { Randomized, Morning, Afternoon, Evening, None }
}

public class WarehouseManager : MonoBehaviour
{
    [Header("Warehouse Prefabs")]
    public GameObject m_shelfPrefab;
    public GameObject m_botPrefab;
    public GameObject m_stationPrefab;
    public GameObject m_dropoffPrefab;
    public GameObject m_roadPrefab;
    public GameObject m_warehousePrefab;

    [Header("App params")]
    public int m_width;                 // width of warehouse
    public int m_length;                // length of warehouse
    public int m_rows;
    public int m_cols;
    public bool m_horizontal;           // true if shelves aligned on X-axis
    public int m_numBots;
    public int m_dropoff;
    public FloorType m_floorType;
    public LightingType m_lighting = LightingType.Afternoon;
    [Range(0.0f, 1.0f)]
    public float m_generateFloorBoxes;
    [Range(0.0f, 1.0f)]
    public float m_generateFloorDebris;
    [Range(0.0f, 1.0f)]
    public float m_percentLight = 0.0f;
    public int m_quitAfterSeconds = 60;

    NavMeshSurface _navmeshSurface;
    GameObject _parentWarehouse;
    Transform _parentDebris;
    List<GameObject> _paths = new List<GameObject>();
    List<GameObject> _dropoffs = new List<GameObject>();
    readonly Quaternion _vRot = Quaternion.identity;
    readonly Quaternion _hRot = Quaternion.Euler(0, 90, 0);

    // Start is called before the first frame update
    void Start()
    {
        if (Configuration.Instance.IsSimulationRunningInCloud()) {
            m_width = ParamReader.appParams.m_width;
            m_length = ParamReader.appParams.m_length;
            m_rows = ParamReader.appParams.m_rows;
            m_cols = ParamReader.appParams.m_cols;
            m_horizontal = ParamReader.appParams.m_horizontal;
            m_numBots = ParamReader.appParams.m_numBots;
            m_dropoff = ParamReader.appParams.m_dropoff;
            m_floorType = (FloorType)Enum.Parse(typeof(FloorType), ParamReader.appParams.m_floorType);
            m_lighting = (LightingType)Enum.Parse(typeof(LightingType), ParamReader.appParams.m_lighting);
            m_percentLight = ParamReader.appParams.m_percentLight;
            m_quitAfterSeconds = ParamReader.appParams.m_quitAfterSeconds;
        }

        GenerateWarehouse();

        var shelves = GenerateShelves();
        var sleepPos = GenerateSleepPosition();

        // Spawn bots
        for (int i = 1; i < m_numBots; i++){
            Instantiate(m_botPrefab, Vector3.zero, Quaternion.identity);
        }

        // Assign robot goals
        var bots = new List<GameObject>(GameObject.FindGameObjectsWithTag("Robot"));

        if (bots.Count > sleepPos.Count){
            Debug.LogError("Too many bots for width of warehouse!");
        }

        var waypoints = GenerateWaypoints();
        GenerateRandomBoxes(waypoints);
        _dropoffs = GenerateDropoff(_paths);

        foreach (var go in bots) {
            var b = go.GetComponent<RobotAgent>();

            if (sleepPos.Count > 0){
                // Set spawn/end location
                int station = UnityEngine.Random.Range(0, sleepPos.Count);
                b.SetSpawn(sleepPos[station]);

                // Set values
                b.SetDropoffs(_dropoffs);
                b.SetShelves(shelves);

                // Don't allow bots to have the same location
                sleepPos.RemoveAt(station);
            }
        }

        var light = GameObject.Find("Directional Light").GetComponent<Light>();
        switch(m_lighting){
            case LightingType.Afternoon:
                light.transform.rotation = Quaternion.Euler(54, 218, 193);    
                light.color = Color.white;
                break;
            case LightingType.Evening:
                light.transform.rotation = Quaternion.Euler(148, 74, 46);    
                light.color = new Color32(137, 53, 0, 255);
                break;
            case LightingType.Morning:
                light.transform.rotation = Quaternion.Euler(23, 120, 72);
                light.color = new Color32(197, 255, 237, 255);
                break;
            case LightingType.Randomized:
                GetComponent<LightingRandomizer>().enabled = true;
                break;
            case LightingType.None:
                light.gameObject.SetActive(false);
                break;
            default:
                break;
        }
    }

    private List<GameObject> GenerateDropoff(List<GameObject> paths){
        var objects = new List<GameObject>();

        float left = 9999;
        float top = -9999;
        float bottom = 9999;
        float right = -9999;

        float pickC = (m_dropoff > 1) ? m_width / (m_dropoff + 1.0f) : m_width / 2.0f;

        foreach (var path in paths){
            if (path.transform.position.x < left){
                left = path.transform.position.x;
            }
            if (path.transform.position.z > top){
                top = path.transform.position.z;
            }
            if (path.transform.position.x > right){
                right = path.transform.position.x;
            }
            if (path.transform.position.z < bottom){
                bottom = path.transform.position.z;
            }
        }

        var cur = new Vector3(0, 0f, top);

        var line = Instantiate(m_dropoffPrefab, cur, Quaternion.identity, _parentWarehouse.transform);
        line.transform.localScale = new Vector3(m_width, line.transform.localScale.y, line.transform.localScale.z);

        cur = new Vector3(left, 0f, top + 2f);

        var parentDropoff = new GameObject("Dropoffs").transform;

        while (cur.x < right + pickC){
            var b = new GameObject("Dropoff").transform;
            b.position = cur;
            b.rotation = Quaternion.identity;
            b.parent = parentDropoff;
            cur.x += pickC;
            objects.Add(b.gameObject);
        }

        return objects;
    }

    private void GenerateFloorDebris(Transform floorTile){
        if (_parentDebris == null) _parentDebris = new GameObject("Debris").transform;

        var dist = floorTile.GetComponent<Renderer>().bounds.size;
        var minDist = dist.x / 2 - 0.1f;
        var floorPos = floorTile.localPosition;

        for(int i = 0; i < (int)(m_generateFloorDebris * 100); i++){
            var pos = new Vector3(floorPos.x + Random.Range(-minDist,minDist), floorPos.y + 0.5f, floorPos.z + Random.Range(-minDist,minDist));

            var randShape = Random.Range(0, 4);
            GameObject obj;
            switch(randShape){
                case 0:
                    obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    break;
                case 1:
                    obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    break;
                case 2:
                    obj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    break;
                case 3:
                    obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    break;
                default:
                    obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    break;
                
            }
            obj.transform.localScale = new Vector3(0.04f, 0.04f, 0.04f);
            obj.transform.parent = _parentDebris;
            obj.transform.position = pos;
            obj.transform.rotation = Random.rotation;

            var lab = obj.AddComponent<Labeling>();
            lab.labels.Add("debris");

            var rb = obj.AddComponent<Rigidbody>();

        }
    }

    private void GenerateRandomBoxes(List<Vector3> waypoints){
        var boxPrefab0 = m_shelfPrefab.GetComponentInChildren<Shelve>().m_boxes[0];
        var boxPrefab1 = m_shelfPrefab.GetComponentInChildren<Shelve>().m_boxes[1];
        var parentBoxes = new GameObject("FloorBoxes").transform;

        float rand = Random.Range(0f, 1f);

        foreach (var pt in waypoints){
            if (rand < m_generateFloorBoxes) {
                int rand_box = Random.Range(0, 2);
                var box = rand_box == 1 ? boxPrefab1 : boxPrefab0;
                var b = Instantiate(box, new Vector3(pt.x, 1, pt.z), Quaternion.identity, parentBoxes);
                b.transform.rotation = Random.rotation;
                b.transform.localScale *= 0.0075f;
            }
            rand = Random.Range(0f, 1f);
        }
    }

    // Generate bot waypoint system
    private List<Vector3> GenerateWaypoints(){
        var pts = new List<Vector3>();

        var xCoord = new List<float>();
        var zCoord = new List<float>();

        foreach (var p in _paths){
            xCoord.Add(p.transform.position.x);
            zCoord.Add(p.transform.position.z);
        }

        xCoord.Sort();
        zCoord.Sort();

        for (int i = 0; i < xCoord.Count; i++){
            for (int j = 0; j < zCoord.Count; j++){
                if (!pts.Contains(new Vector3(xCoord[i], 0, zCoord[j])) && xCoord[i] != 0){
                    pts.Add(new Vector3(xCoord[i], 0, zCoord[j]));
                }
            }
        }

        return pts;
    }

    // Generate warehouse assets based on params
    private void GenerateWarehouse(){
        var floorTile = m_warehousePrefab.transform.Find("Floor01").gameObject;
        var ceilingTile = m_warehousePrefab.transform.Find("Ceiling01").gameObject;
        var wallTile = m_warehousePrefab.transform.Find("Wall").gameObject;
        var lightTile = m_warehousePrefab.transform.Find("LightFixture001").gameObject;
        var skylight = m_warehousePrefab.transform.Find("Skylight01").gameObject;
        var column = m_warehousePrefab.transform.Find("Column01").gameObject;
        var glulam = m_warehousePrefab.transform.Find("Glulam01").gameObject;

        var floorTileSize = floorTile.GetComponent<Renderer>().bounds.size;
        var wallTileSize = wallTile.GetComponent<Renderer>().bounds.size;
        var ceilingTileSize = ceilingTile.GetComponent<Renderer>().bounds.size;
        var columnSize = column.GetComponent<Renderer>().bounds.size;
        var glulamSize = glulam.GetComponent<Renderer>().bounds.size;

        if (_parentWarehouse == null) _parentWarehouse = new GameObject("Warehouse");
        var parentTransform = _parentWarehouse.transform;

        for (var i = 1; i < m_width / floorTileSize.x + 1; i++){
            for (var j = 1; j < m_length / floorTileSize.z + 1; j++){
                var floor = Instantiate(floorTile, new Vector3(i * floorTileSize.x - (m_width / 2 + floorTileSize.x * 0.75f), 0, j * floorTileSize.z - (m_length / 2 + floorTileSize.z * 0.75f)), _vRot, parentTransform);
                floor.GetComponent<BoxCollider>().material = m_warehousePrefab.GetComponent<ParameterSettings>().GetMaterial(m_floorType);
                GenerateFloorDebris(floor.transform);

                var ceiling = Instantiate(ceilingTile, new Vector3(floor.transform.position.x, wallTileSize.y, floor.transform.position.z), _vRot, parentTransform);
                Instantiate(skylight, new Vector3(ceiling.transform.position.x, ceiling.transform.position.y + ceilingTileSize.y, ceiling.transform.position.z), _vRot, parentTransform);

                if (i == 1){
                    Instantiate(wallTile, new Vector3(floor.transform.position.x - floorTileSize.x/2, wallTileSize.y/2, floor.transform.position.z), _vRot, parentTransform);
                }
                if (i > m_width / floorTileSize.x){
                    Instantiate(wallTile, new Vector3(floor.transform.position.x + floorTileSize.x/2, wallTileSize.y/2, floor.transform.position.z), _vRot, parentTransform);
                }
                if (j == 1){
                    Instantiate(wallTile, new Vector3(floor.transform.position.x, wallTileSize.y/2, floor.transform.position.z - floorTileSize.z/2), _hRot, parentTransform);
                }
                if (j > m_length / floorTileSize.z){
                    Instantiate(wallTile, new Vector3(floor.transform.position.x, wallTileSize.y/2, floor.transform.position.z + floorTileSize.z/2), _hRot, parentTransform);
                }

                // Don't instantiate edge lights
                if (i <  m_width / floorTileSize.x && j < m_length / floorTileSize.z){
                    if ((i % 2 == 0) && (j % 2 == 0)){
                        var light = Instantiate(lightTile, new Vector3(ceiling.transform.position.x + floorTileSize.x/2, ceiling.transform.position.y - glulamSize.y, ceiling.transform.position.z + floorTileSize.z/2), Quaternion.identity, parentTransform);
                        float rand = UnityEngine.Random.Range(0.0f, 1.0f);
                        if (rand > m_percentLight){
                            light.GetComponentInChildren<Light>().enabled = false;
                        }
                    }
                    if (i == 1){
                        var g = Instantiate(glulam, new Vector3(0, ceiling.transform.position.y - glulamSize.y/2, ceiling.transform.position.z + floorTileSize.z/2), Quaternion.identity, parentTransform);
                        g.transform.localScale = new Vector3((m_width / 30f) * g.transform.localScale.x, g.transform.localScale.y, g.transform.localScale.z);
                    }
                }
            }
        }
    }

    // Generate shelves based on app params
    private List<GameObject> GenerateShelves(){
        var shelves = new List<GameObject>();

        float r = (m_rows > 1) ? m_length / (m_rows + 1.0f) : m_length / 2.0f;
        float c = (m_cols > 1) ? m_width / (m_cols + 1.0f) : m_width / 2.0f;

        var shelfSize = m_shelfPrefab.transform.Find("Rack").GetComponent<Renderer>().bounds.size;

        if (m_horizontal){
            if (shelfSize.x >= c){
                Debug.LogWarning("Shelf columns will overlap with no space to navigate.");
            }
            if (shelfSize.y >= r){
                Debug.LogWarning("Shelf rows will overlap with no space to navigate.");
            }
        }
        else {
            if (shelfSize.y >= c){
                Debug.LogWarning("Shelf columns will overlap with no space to navigate.");
            }
            if (shelfSize.x >= r){
                Debug.LogWarning("Shelf rows will overlap with no space to navigate.");
            }
        }

        // Generate paths between shelves
        GameObject v;
        GameObject h;

        var shelfParent = new GameObject("Shelves").transform;
        var pathParent = new GameObject("Paths").transform;

        for (var i = 1; i < m_cols + 1; i++){
            bool colPath = false;
            for (var j = 1; j < m_rows + 1; j++){
                var o = Instantiate(m_shelfPrefab, new Vector3(c * i - (m_width / 2), 0, r * j - (m_length / 2)), Quaternion.identity, shelfParent);
                if (m_horizontal){
                    o.transform.rotation = Quaternion.Euler(0, 90, 0);
                }
                shelves.Add(o);

                // need to instantiate only once per row and once per column
                if (!colPath && i < m_cols){
                    v = Instantiate(m_roadPrefab, new Vector3(o.transform.position.x + (c/2), 0.01f, 0), _vRot, pathParent);
                    v.transform.localScale = new Vector3(r / 30f, v.transform.localScale.y, m_length / 10f);
                    v.GetComponent<BoxCollider>().material = m_warehousePrefab.GetComponent<ParameterSettings>().GetMaterial(m_floorType);
                    _paths.Add(v);
                    colPath = true;
                }
                if (i == 1 && j < m_rows){
                    h = Instantiate(m_roadPrefab, new Vector3(0, 0.01f, o.transform.position.z + (r/2)), _hRot, pathParent);
                    h.GetComponent<BoxCollider>().material = m_warehousePrefab.GetComponent<ParameterSettings>().GetMaterial(m_floorType);
                    h.transform.localScale = new Vector3(c / 30f, h.transform.localScale.y, m_width / 10f);
                    _paths.Add(h);
                }
            }
        }

        // Edge paths

        // Station path
        h = Instantiate(m_roadPrefab, new Vector3(0, 0.01f, -(m_length / 2)), _hRot, pathParent);
        h.GetComponent<BoxCollider>().material = m_warehousePrefab.GetComponent<ParameterSettings>().GetMaterial(m_floorType);
        h.transform.localScale = new Vector3(c / 30f, h.transform.localScale.y, m_width / 10f);

        // First path
        v = Instantiate(m_roadPrefab, new Vector3(-(m_width / 2) + c/2, 0.01f, 0), _vRot, pathParent);
        v.GetComponent<BoxCollider>().material = m_warehousePrefab.GetComponent<ParameterSettings>().GetMaterial(m_floorType);
        v.transform.localScale = new Vector3(r / 30f, v.transform.localScale.y, m_length / 10f);
        h = Instantiate(m_roadPrefab, new Vector3(0, 0.01f, -(m_length / 2) + r/2), _hRot, pathParent);
        h.GetComponent<BoxCollider>().material = m_warehousePrefab.GetComponent<ParameterSettings>().GetMaterial(m_floorType);
        h.transform.localScale = new Vector3(c / 30f, h.transform.localScale.y, m_width / 10f);
        _paths.Add(v);
        _paths.Add(h);

        // Last path
        v = Instantiate(m_roadPrefab, new Vector3(m_width / 2 - (c/2), 0.01f, 0), _vRot, pathParent);
        v.GetComponent<BoxCollider>().material = m_warehousePrefab.GetComponent<ParameterSettings>().GetMaterial(m_floorType);
        v.transform.localScale = new Vector3(r / 30f, v.transform.localScale.y, m_length / 10f);
        h = Instantiate(m_roadPrefab, new Vector3(0, 0.01f, m_length /2 - (r/2)), _hRot, pathParent);
        h.GetComponent<BoxCollider>().material = m_warehousePrefab.GetComponent<ParameterSettings>().GetMaterial(m_floorType);
        h.transform.localScale = new Vector3(c / 30f, h.transform.localScale.y, m_width / 10f);
        h.GetComponent<NavMeshSurface>().BuildNavMesh();
        _paths.Add(v);
        _paths.Add(h);

        return shelves;
    }

    // Generate start/end positions for bots
    private List<Vector3> GenerateSleepPosition(){
        var positions = new List<Vector3>();

        var botSize = m_botPrefab.GetComponent<Renderer>().bounds.size;
        var cur = new Vector3(-m_width / 2, 0.0f, -m_length / 2);

        var parentStations = new GameObject("Stations").transform;

        while (cur.x < (m_width / 2) - botSize.x * 2){
            Instantiate(m_stationPrefab, new Vector3(cur.x, 0.0000001f, cur.z), Quaternion.identity, parentStations);
            positions.Add(cur);
            cur.x += (2f * botSize.x);
        }
        return positions;
    }

    public AppParam GetEditorParams(){
        var param = new AppParam();
        param.m_width = m_width;
        param.m_length = m_length;
        param.m_rows = m_rows;
        param.m_cols = m_cols;
        param.m_horizontal = m_horizontal;
        param.m_numBots = m_numBots;
        param.m_dropoff = m_dropoff;
        param.m_floorType = m_floorType.ToString();
        param.m_lighting = m_lighting.ToString();
        param.m_generateFloorBoxes = m_generateFloorBoxes;
        param.m_generateFloorDebris = m_generateFloorDebris;
        param.m_percentLight = m_percentLight;
        param.m_quitAfterSeconds = m_quitAfterSeconds;
        
        return param;
    }
}
