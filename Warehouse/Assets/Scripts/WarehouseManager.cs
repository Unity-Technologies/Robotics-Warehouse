using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.SimulationControl;

namespace Unity.Simulation.Warehouse {
    public class WarehouseManager : MonoBehaviour
    {
        [Header("Warehouse Prefabs")]
        public GameObject m_shelfPrefab;
        public GameObject m_stationPrefab;
        public GameObject m_dropoffPrefab;
        public GameObject m_roadPrefab;
        public GameObject m_warehousePrefab;

        public AppParam appParam;
        private static AppParam _instance;
        public static AppParam instance
        {
            get 
            {
                return _instance;
            }
        }

        GameObject _parentGenerated;
        GameObject _parentWarehouse;
        List<GameObject> _paths = new List<GameObject>();
        List<GameObject> _dropoffs = new List<GameObject>();
        public static Quaternion _vRot = Quaternion.identity;
        public static Quaternion _hRot = Quaternion.Euler(0, 90, 0);
        public static float _pathHeight = 0.0001f;

        // Start is called before the first frame update
        void Awake()
        {            
            if (GameObject.FindObjectsOfType<WarehouseManager>().Length > 1) 
            {
                Destroy(GameObject.FindObjectsOfType<WarehouseManager>()[1].gameObject);
            }
            _instance = appParam;
            _parentGenerated = GameObject.Find("GeneratedWarehouse");
            if (_parentGenerated == null)
            {
                _parentGenerated = new GameObject("GeneratedWarehouse");
                //_parentGenerated.tag = "Generated";

                GenerateWarehouse();

                var shelves = GenerateShelves();
                var stations = GenerateStations();

                var waypoints = GenerateWaypoints();
                _dropoffs = GenerateDropoff(_paths);       
            }
        }

        private List<GameObject> GenerateDropoff(List<GameObject> paths){
            var objects = new List<GameObject>();

            float left = 9999;
            float top = -9999;
            float bottom = 9999;
            float right = -9999;

            float pickC = (appParam.m_dropoff > 1) ? appParam.m_width / (appParam.m_dropoff + 1.0f) : appParam.m_width / 2.0f;

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
            line.transform.localScale = new Vector3(appParam.m_width, line.transform.localScale.y, line.transform.localScale.z);

            cur = new Vector3(left, 0f, top + 2f);

            var parentDropoff = new GameObject("Dropoffs").transform;
            parentDropoff.parent = _parentGenerated.transform;

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
            var wallTile = m_warehousePrefab.transform.Find("WallPanel01").gameObject;
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
            parentTransform.parent = _parentGenerated.transform;

            for (var i = 1; i < appParam.m_width / floorTileSize.x + 1; i++){
                for (var j = 1; j < appParam.m_length / floorTileSize.z + 1; j++){
                    var floor = Instantiate(floorTile, new Vector3(i * floorTileSize.x - (appParam.m_width / 2 + floorTileSize.x * 0.75f), 0, j * floorTileSize.z - (appParam.m_length / 2 + floorTileSize.z * 0.75f)), _vRot, parentTransform);
                    floor.AddComponent<MaterialRandomizerTag>();

                    var ceiling = Instantiate(ceilingTile, new Vector3(floor.transform.position.x, wallTileSize.y, floor.transform.position.z), _vRot, parentTransform);
                    Instantiate(skylight, new Vector3(ceiling.transform.position.x, ceiling.transform.position.y + ceilingTileSize.y, ceiling.transform.position.z), _vRot, parentTransform);

                    if (i == 1){
                        Instantiate(wallTile, new Vector3(floor.transform.position.x - floorTileSize.x/2, wallTileSize.y/2, floor.transform.position.z), _vRot, parentTransform);
                    }
                    if (i > appParam.m_width / floorTileSize.x){
                        Instantiate(wallTile, new Vector3(floor.transform.position.x + floorTileSize.x/2, wallTileSize.y/2, floor.transform.position.z), _vRot, parentTransform);
                    }
                    if (j == 1){
                        Instantiate(wallTile, new Vector3(floor.transform.position.x, wallTileSize.y/2, floor.transform.position.z - floorTileSize.z/2), _hRot, parentTransform);
                    }
                    if (j > appParam.m_length / floorTileSize.z){
                        Instantiate(wallTile, new Vector3(floor.transform.position.x, wallTileSize.y/2, floor.transform.position.z + floorTileSize.z/2), _hRot, parentTransform);
                    }

                    // Don't instantiate edge lights
                    if (i <  appParam.m_width / floorTileSize.x && j < appParam.m_length / floorTileSize.z){
                        if ((i % 2 == 0) && (j % 2 == 0)){
                            var light = Instantiate(lightTile, new Vector3(ceiling.transform.position.x + floorTileSize.x/2, ceiling.transform.position.y - glulamSize.y, ceiling.transform.position.z + floorTileSize.z/2), Quaternion.identity, parentTransform);
                            float rand = UnityEngine.Random.Range(0.0f, 1.0f);
                        }
                        if (i == 1){
                            var g = Instantiate(glulam, new Vector3(0, ceiling.transform.position.y - glulamSize.y/2, ceiling.transform.position.z + floorTileSize.z/2), Quaternion.identity, parentTransform);
                            g.transform.localScale = new Vector3((appParam.m_width / 30f) * g.transform.localScale.x, g.transform.localScale.y, g.transform.localScale.z);
                        }
                    }
                }
            }
        }

        // Generate shelves based on app params
        private List<GameObject> GenerateShelves(){
            var shelves = new List<GameObject>();

            float r = (appParam.m_rows > 1) ? appParam.m_length / (appParam.m_rows + 1.0f) : appParam.m_length / 2.0f;
            float c = (appParam.m_cols > 1) ? appParam.m_width / (appParam.m_cols + 1.0f) : appParam.m_width / 2.0f;

            var shelfSize = m_shelfPrefab.transform.Find("Rack").GetComponent<Renderer>().bounds.size;

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
            shelfParent.parent = _parentGenerated.transform;
            var pathParent = new GameObject("Paths").transform;
            pathParent.parent = _parentGenerated.transform;

            for (var i = 1; i < appParam.m_cols + 1; i++){
                bool colPath = false;
                for (var j = 1; j < appParam.m_rows + 1; j++){
                    var o = Instantiate(m_shelfPrefab, new Vector3(c * i - (appParam.m_width / 2), 0, r * j - (appParam.m_length / 2)), Quaternion.identity, shelfParent);
                    shelves.Add(o);

                    // need to instantiate only once per row and once per column
                    if (!colPath && i < appParam.m_cols){
                        v = Instantiate(m_roadPrefab, new Vector3(o.transform.position.x + (c/2), _pathHeight, 0), _vRot, pathParent);
                        v.transform.localScale = new Vector3(r / 30f, v.transform.localScale.y, appParam.m_length / 10f);
                        _paths.Add(v);
                        colPath = true;
                    }
                    if (i == 1 && j < appParam.m_rows){
                        h = Instantiate(m_roadPrefab, new Vector3(0, _pathHeight, o.transform.position.z + (r/2)), _hRot, pathParent);
                        h.transform.localScale = new Vector3(c / 30f, h.transform.localScale.y, appParam.m_width / 10f);
                        _paths.Add(h);
                    }
                }
            }

            // Station path
            h = Instantiate(m_roadPrefab, new Vector3(0, _pathHeight, -(appParam.m_length / 2)), _hRot, pathParent);
            h.transform.localScale = new Vector3(c / 30f, h.transform.localScale.y, appParam.m_width / 10f);

            // First path
            v = Instantiate(m_roadPrefab, new Vector3(-(appParam.m_width / 2) + c/2, _pathHeight, 0), _vRot, pathParent);
            v.transform.localScale = new Vector3(r / 30f, v.transform.localScale.y, appParam.m_length / 10f);
            h = Instantiate(m_roadPrefab, new Vector3(0, _pathHeight, -(appParam.m_length / 2) + r/2), _hRot, pathParent);
            h.transform.localScale = new Vector3(c / 30f, h.transform.localScale.y, appParam.m_width / 10f);
            _paths.Add(v);
            _paths.Add(h);

            // Last path
            v = Instantiate(m_roadPrefab, new Vector3(appParam.m_width / 2 - (c/2), _pathHeight, 0), _vRot, pathParent);
            v.transform.localScale = new Vector3(r / 30f, v.transform.localScale.y, appParam.m_length / 10f);
            h = Instantiate(m_roadPrefab, new Vector3(0, _pathHeight, appParam.m_length /2 - (r/2)), _hRot, pathParent);
            h.transform.localScale = new Vector3(c / 30f, h.transform.localScale.y, appParam.m_width / 10f);
            _paths.Add(v);
            _paths.Add(h);

            return shelves;
        }

        // Generate start/end positions for bots
        private List<Vector3> GenerateStations()
        {
            var positions = new List<Vector3>();

            var size = Vector3.one;
            var cur = new Vector3(-appParam.m_width / 2, 0.0f, -appParam.m_length / 2);

            var parentStations = new GameObject("Stations").transform;
            parentStations.parent = _parentGenerated.transform;

            while (cur.x < (appParam.m_width / 2) - size.x * 2)
            {
                Instantiate(m_stationPrefab, new Vector3(cur.x, 0.0000001f, cur.z), Quaternion.identity, parentStations);
                positions.Add(cur);
                cur.x += (2f * size.x);
            }
            return positions;
        }

        public AppParam GetEditorParams(){
            return appParam;
        }
    }
}