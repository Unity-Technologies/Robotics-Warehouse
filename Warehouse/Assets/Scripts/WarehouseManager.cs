using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.SimulationControl;

namespace Unity.Simulation.Warehouse {
    public class WarehouseManager : MonoBehaviour
    {
        [Header("Warehouse Prefabs")]
        public GameObject m_shelfPrefab;
        public GameObject m_stationPrefab;
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
        public static Quaternion _vRot = Quaternion.identity;
        public static Quaternion _hRot = Quaternion.Euler(0, 90, 0);

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
            }
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