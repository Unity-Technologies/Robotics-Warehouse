using System;
using System.Collections.Generic;
using Unity.Robotics.PerceptionRandomizers.Shims;
using UnityEngine;

namespace Unity.Simulation.Warehouse
{
    [RequireComponent(typeof(ScenarioShim))]
    public class WarehouseManager : MonoBehaviour
    {
        public const string k_GeneratedWarehouseObjectName = "GeneratedWarehouse";

        // Warehouse manager singleton
        static WarehouseManager s_Instance;

        static Quaternion s_WallRotation = Quaternion.Euler(0, 90, 0);
        [SerializeField]
        GameObject m_ShelfPrefab;

        [SerializeField]
        Transform m_WarehousePrefab;

        [SerializeField]
        AppParam m_AppParam;

        GameObject m_ParentGenerated;

        public GameObject ShelfPrefab
        {
            get
            {
                if (m_ShelfPrefab == null)
                {
                    m_ShelfPrefab = Resources.Load<GameObject>("Prefabs/ShelvingRackRandom");
                }
                return m_ShelfPrefab;
            }
            set => m_ShelfPrefab = value;
        }

        public Transform WarehousePrefab
        {
            get
            {
                if (m_WarehousePrefab == null)
                {
                    m_WarehousePrefab = Resources.Load<GameObject>("Prefabs/Warehouse").transform;
                }
                return m_WarehousePrefab;
            }
            set => m_WarehousePrefab = value;
        }

        public AppParam AppParam
        {
            get => m_AppParam;
            set => m_AppParam = value;
        }

        public static WarehouseManager Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = FindObjectOfType<WarehouseManager>();
                    if (s_Instance != null)
                    {
                        s_Instance.ScenarioShim = FindObjectOfType<ScenarioShim>();
                    }
                }

                return s_Instance;
            }
        }

        public GameObject ParentGenerated
        {
            get
            {
                if (m_ParentGenerated == null)
                {
                    m_ParentGenerated = new GameObject(k_GeneratedWarehouseObjectName);
                }
                return m_ParentGenerated;
            }
            private set => m_ParentGenerated = value;
        }

        public ScenarioShim ScenarioShim { get; set; }

        public void Generate()
        {
            GenerateWarehouse();
            GenerateShelves();
            GenerateStations();
        }

        public void IncrementIteration()
        {
            ScenarioShim.RandomizeOnce();
        }

        public void Destroy()
        {
            var spawned = GameObject.Find("FloorBoxes");
            if (ParentGenerated != null)
            {
                DestroyImmediate(ParentGenerated);
                DestroyImmediate(spawned);
                ParentGenerated = null;
            }
        }

        // Generate warehouse assets based on params
        void GenerateWarehouse()
        {
            // Find component mesh in prefab
            var floorTile = GetChildTransformsByTag(WarehousePrefab, "Floor")[0].gameObject;
            var ceilingTile = GetChildTransformsByTag(WarehousePrefab, "Ceiling")[0].gameObject;
            var wallTile = GetChildTransformsByTag(WarehousePrefab, "WallPanel")[0].gameObject;
            var lightTile = GetChildTransformsByTag(WarehousePrefab, "LightFixture")[0].gameObject;
            var skylight = GetChildTransformsByTag(WarehousePrefab, "Skylight")[0].gameObject;
            var column = GetChildTransformsByTag(WarehousePrefab, "Column")[0].gameObject;
            var glulam = GetChildTransformsByTag(WarehousePrefab, "Glulam")[0].gameObject;

            var floorTileSize = floorTile.GetComponent<Renderer>().bounds.size;
            var wallTileSize = wallTile.GetComponent<Renderer>().bounds.size;
            var ceilingTileSize = ceilingTile.GetComponent<Renderer>().bounds.size;
            var columnSize = column.GetComponent<Renderer>().bounds.size;
            var glulamSize = glulam.GetComponent<Renderer>().bounds.size;

            // Create empty GameObject parents
            var parentTransform = new GameObject("Warehouse").transform;
            parentTransform.parent = ParentGenerated.transform;
            var floorsParent = new GameObject("Floors").transform;
            floorsParent.parent = parentTransform;
            var ceilingParent = new GameObject("Ceilings").transform;
            ceilingParent.parent = parentTransform;
            var wallParent = new GameObject("Walls").transform;
            wallParent.parent = parentTransform;

            // Calculate offsets
            var floorScaled = floorTileSize * 0.75f;
            var floorOffset = new Vector3(AppParam.width / 2.0f, 0, AppParam.length / 2.0f) + floorScaled;

            // Instantiate warehouse shell
            for (var i = 1; i < AppParam.width / floorTileSize.x + 1; i++)
                for (var j = 1; j < AppParam.length / floorTileSize.z + 1; j++)
                {
                    // Instantiate floors
                    var fPos = Vector3.Scale(new Vector3(i, 0, j), floorTileSize);
                    var floor = Instantiate(floorTile, fPos - floorOffset, Quaternion.identity, floorsParent);
                    floor.AddComponent<MaterialRandomizerTag>();

                    // Ceilings
                    var ceiling = Instantiate(ceilingTile,
                        new Vector3(floor.transform.position.x, wallTileSize.y, floor.transform.position.z),
                        Quaternion.identity, ceilingParent);
                    Instantiate(skylight,
                        new Vector3(ceiling.transform.position.x, ceiling.transform.position.y + ceilingTileSize.y,
                            ceiling.transform.position.z), Quaternion.identity, ceilingParent);

                    // Walls (on edges only)
                    if (i == 1)
                        Instantiate(wallTile,
                            new Vector3(floor.transform.position.x - floorTileSize.x / 2, wallTileSize.y / 2,
                                floor.transform.position.z), Quaternion.identity, wallParent);

                    if (i > AppParam.width / floorTileSize.x)
                        Instantiate(wallTile,
                            new Vector3(floor.transform.position.x + floorTileSize.x / 2, wallTileSize.y / 2,
                                floor.transform.position.z), Quaternion.identity, wallParent);

                    if (j == 1)
                        Instantiate(wallTile,
                            new Vector3(floor.transform.position.x, wallTileSize.y / 2,
                                floor.transform.position.z - floorTileSize.z / 2), s_WallRotation, wallParent);

                    if (j > AppParam.length / floorTileSize.z)
                        Instantiate(wallTile,
                            new Vector3(floor.transform.position.x, wallTileSize.y / 2,
                                floor.transform.position.z + floorTileSize.z / 2), s_WallRotation, wallParent);

                    // Lights
                    if (i < AppParam.width / floorTileSize.x && j < AppParam.length / floorTileSize.z)
                    {
                        // Only create every other light
                        if (i % 2 == 0 && j % 2 == 0)
                        {
                            var light = Instantiate(lightTile,
                                new Vector3(ceiling.transform.position.x + floorTileSize.x / 2,
                                    ceiling.transform.position.y - glulamSize.y,
                                    ceiling.transform.position.z + floorTileSize.z / 2), Quaternion.identity,
                                ceilingParent);
                        }

                        // Create one glulam per tile row
                        if (i == 1)
                        {
                            var g = Instantiate(glulam,
                                new Vector3(0, ceiling.transform.position.y - glulamSize.y / 2,
                                    ceiling.transform.position.z + floorTileSize.z / 2), Quaternion.identity,
                                ceilingParent);
                            g.transform.localScale = new Vector3(AppParam.width / 30f * g.transform.localScale.x,
                                g.transform.localScale.y, g.transform.localScale.z);
                        }
                    }
                }
        }

        // Instantiate empty shelf racks
        void GenerateShelves()
        {
            // Calculate distance between shelves
            var r = AppParam.shelfRows > 1 ? AppParam.length / (AppParam.shelfRows + 1.0f) : AppParam.length / 2.0f;
            var c = AppParam.shelfCols > 1 ? AppParam.width / (AppParam.shelfCols + 1.0f) : AppParam.width / 2.0f;

            var shelfSize = ShelfPrefab.transform.Find("Rack").GetComponent<Renderer>().bounds.size;

            if (shelfSize.y >= c || shelfSize.x >= r)
            {
                Debug.LogWarning("Shelves will overlap with no space to navigate.");
            }

            var shelfParent = new GameObject("Shelves").transform;
            shelfParent.parent = ParentGenerated.transform;

            // Instantiate shelves
            for (var i = 1; i < AppParam.shelfCols + 1; i++)
            {
                for (var j = 1; j < AppParam.shelfRows + 1; j++)
                {
                    var o = Instantiate(ShelfPrefab,
                        new Vector3(c * i - AppParam.width / 2, 0, r * j - AppParam.length / 2), Quaternion.identity,
                        shelfParent);
                }
            }
        }

        // Generate placeholder "cubes" for "stations," for visual effect
        void GenerateStations()
        {
            var station = WarehousePrefab.Find("Station").gameObject;

            var cur = new Vector3(-AppParam.width / 2, 0.1f, -AppParam.length / 2);

            var parentStations = new GameObject("Stations").transform;
            parentStations.parent = ParentGenerated.transform;

            while (cur.x < AppParam.width / 2f)
            {
                Instantiate(station, cur, Quaternion.identity, parentStations);
                cur.x += 2;
            }
        }

        static Transform[] GetChildTransformsByTag(Transform transform, string tag)
        {
            var children = new List<Transform>();
            foreach (Transform child in transform)
            {
                if (child.tag.Equals(tag))
                {
                    children.Add(child);
                }
            }
            return children.ToArray();
        }
    }
}
