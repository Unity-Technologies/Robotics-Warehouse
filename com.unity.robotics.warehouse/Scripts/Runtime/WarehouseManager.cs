using UnityEngine;

namespace Unity.Simulation.Warehouse {
    public class WarehouseManager : MonoBehaviour
    {
        public GameObject shelfPrefab;
        public Transform warehousePrefab;

        public AppParam appParam;
        private static AppParam _instance;
        public static AppParam instance
        {
            get { return _instance; }
        }

        GameObject parentGenerated;
        GameObject parentWarehouse;
        public static Quaternion hRot = Quaternion.Euler(0, 90, 0);

        // Start is called before the first frame update
        void Start()
        {            
            if (GameObject.FindObjectsOfType<WarehouseManager>().Length > 1) 
            {
                Destroy(GameObject.FindObjectsOfType<WarehouseManager>()[1].gameObject);
            }

            _instance = appParam;
            parentGenerated = GameObject.Find("GeneratedWarehouse");
            if (parentGenerated == null)
            {
                parentGenerated = new GameObject("GeneratedWarehouse");

                GenerateWarehouse();

                GenerateShelves();
                GenerateStations();
            }
        }

        // Generate warehouse assets based on params
        private void GenerateWarehouse()
        {
            // Find component mesh in prefab
            var floorTile = warehousePrefab.Find("Floor01").gameObject;
            var ceilingTile = warehousePrefab.Find("Ceiling01").gameObject;
            var wallTile = warehousePrefab.Find("WallPanel01").gameObject;
            var lightTile = warehousePrefab.Find("LightFixture001").gameObject;
            var skylight = warehousePrefab.Find("Skylight01").gameObject;
            var column = warehousePrefab.Find("Column01").gameObject;
            var glulam = warehousePrefab.Find("Glulam01").gameObject;

            var floorTileSize = floorTile.GetComponent<Renderer>().bounds.size;
            var wallTileSize = wallTile.GetComponent<Renderer>().bounds.size;
            var ceilingTileSize = ceilingTile.GetComponent<Renderer>().bounds.size;
            var columnSize = column.GetComponent<Renderer>().bounds.size;
            var glulamSize = glulam.GetComponent<Renderer>().bounds.size;

            if (parentWarehouse == null) 
            {
                parentWarehouse = new GameObject("Warehouse");
            }

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
            Vector3 floorOffset = new Vector3(appParam.width/2, 0, appParam.length/2) + floorScaled;

            // Instantiate warehouse shell
            for (var i = 1; i < appParam.width / floorTileSize.x + 1; i++)
            {
                for (var j = 1; j < appParam.length / floorTileSize.z + 1; j++)
                {
                    // Instantiate floors
                    Vector3 fPos = Vector3.Scale(new Vector3(i, 0, j), floorTileSize);
                    var floor = Instantiate(floorTile, fPos - floorOffset, Quaternion.identity, floorsParent);
                    floor.AddComponent<MaterialRandomizerTag>();

                    // Ceilings
                    var ceiling = Instantiate(ceilingTile, new Vector3(floor.transform.position.x, wallTileSize.y, floor.transform.position.z), Quaternion.identity, ceilingParent);
                    Instantiate(skylight, new Vector3(ceiling.transform.position.x, ceiling.transform.position.y + ceilingTileSize.y, ceiling.transform.position.z), Quaternion.identity, ceilingParent);

                    // Walls (on edges only)
                    if (i == 1)
                        Instantiate(wallTile, new Vector3(floor.transform.position.x - floorTileSize.x/2, wallTileSize.y/2, floor.transform.position.z), Quaternion.identity, wallParent);
                    
                    if (i > appParam.width / floorTileSize.x)
                        Instantiate(wallTile, new Vector3(floor.transform.position.x + floorTileSize.x/2, wallTileSize.y/2, floor.transform.position.z), Quaternion.identity, wallParent);
                    
                    if (j == 1)
                        Instantiate(wallTile, new Vector3(floor.transform.position.x, wallTileSize.y/2, floor.transform.position.z - floorTileSize.z/2), hRot, wallParent);
                    
                    if (j > appParam.length / floorTileSize.z)
                        Instantiate(wallTile, new Vector3(floor.transform.position.x, wallTileSize.y/2, floor.transform.position.z + floorTileSize.z/2), hRot, wallParent);
                    

                    // Lights
                    if (i <  appParam.width / floorTileSize.x && j < appParam.length / floorTileSize.z)
                    {
                        // Only create every other light
                        if ((i % 2 == 0) && (j % 2 == 0))
                        {
                            var light = Instantiate(lightTile, new Vector3(ceiling.transform.position.x + floorTileSize.x/2, ceiling.transform.position.y - glulamSize.y, ceiling.transform.position.z + floorTileSize.z/2), Quaternion.identity, ceilingParent);
                        }
                        // Create one glulam per tile row
                        if (i == 1)
                        {
                            var g = Instantiate(glulam, new Vector3(0, ceiling.transform.position.y - glulamSize.y/2, ceiling.transform.position.z + floorTileSize.z/2), Quaternion.identity, ceilingParent);
                            g.transform.localScale = new Vector3((appParam.width / 30f) * g.transform.localScale.x, g.transform.localScale.y, g.transform.localScale.z);
                        }
                    }
                }
            }
        }

        // Instantiate empty shelf racks
        private void GenerateShelves()
        {
            // Calculate distance between shelves
            float r = (appParam.shelfRows > 1) ? appParam.length / (appParam.shelfRows + 1.0f) : appParam.length / 2.0f;
            float c = (appParam.shelfCols > 1) ? appParam.width / (appParam.shelfCols + 1.0f) : appParam.width / 2.0f;

            var shelfSize = shelfPrefab.transform.Find("Rack").GetComponent<Renderer>().bounds.size;

            if (shelfSize.y >= c || shelfSize.x >= r)
            {
                Debug.LogWarning("Shelves will overlap with no space to navigate.");
            }

            var shelfParent = new GameObject("Shelves").transform;
            shelfParent.parent = parentGenerated.transform;

            // Instantiate shelves
            for (var i = 1; i < appParam.shelfCols + 1; i++)
            {
                for (var j = 1; j < appParam.shelfRows + 1; j++)
                {
                    var o = Instantiate(shelfPrefab, new Vector3(c * i - (appParam.width/2), 0, r * j - (appParam.length/2)), Quaternion.identity, shelfParent);
                }
            }
        }

        // Generate placeholder "cubes" for "stations," for visual effect
        private void GenerateStations()
        {
            var station = warehousePrefab.Find("Station").gameObject;

            var cur = new Vector3(-appParam.width/2, 0.1f, -appParam.length/2);

            var parentStations = new GameObject("Stations").transform;
            parentStations.parent = parentGenerated.transform;

            while (cur.x < (appParam.width/2f))
            {
                Instantiate(station, cur, Quaternion.identity, parentStations);
                cur.x += 2;
            }
        }
    }
}