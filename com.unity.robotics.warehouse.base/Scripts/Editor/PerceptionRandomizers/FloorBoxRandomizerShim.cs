using System;
using System.Collections.Generic;
using Unity.Robotics.PerceptionRandomizers.Shims;
using Unity.Simulation.Warehouse;
using UnityEditor;
using UnityEngine;
using UnityEngine.Perception.Randomization.Parameters;
using UnityEngine.Perception.Randomization.Randomizers;
using UnityEngine.Perception.Randomization.Randomizers.SampleRandomizers.Tags;
using UnityEngine.Perception.Randomization.Samplers;
using UnityEngine.Perception.Randomization.Scenarios;
using Object = UnityEngine.Object;

[Serializable]
[AddRandomizerMenu("Robotics/Floor Box Randomizer")]
public class FloorBoxRandomizerShim : RandomizerShim
{
    public int numBoxToSpawn;
    public int maxPlacementTries = 100;
    AppParam appParam;
    List<CollisionConstraint> constraints;
    ShelfBoxRandomizerShim m_ShelfBoxRandomizerShim;
    GameObject parentFloorBoxes;

    SurfaceObjectPlacer placer;
    FloatParameter random = new FloatParameter { value = new UniformSampler(0f, 1f) };

    protected override void OnAwake()
    {
        if (WarehouseManager.Instance == null)
        {
            appParam = WarehouseManager.Instance.AppParam;
        }

        // Add collision constraints to spawned shelves
        var tags = tagManager.Query<ShelfBoxRandomizerTag>();
        if (!Application.isPlaying)
        {
            tags = Object.FindObjectsOfType<ShelfBoxRandomizerTag>();
        }

        constraints = new List<CollisionConstraint>();

        foreach (var tag in tags)
        {
            var shelf = new CollisionConstraint(tag.transform.position.x, tag.transform.position.z,
                tag.GetComponentInChildren<Renderer>().bounds.extents.x);
            constraints.Add(shelf);
        }

        base.OnAwake();
    }

    protected override void OnScenarioStart()
    {
        var scenario = Object.FindObjectOfType<Scenario<ScenarioConstants>>();
        m_ShelfBoxRandomizerShim = scenario.GetRandomizer<ShelfBoxRandomizerShim>();
        base.OnScenarioStart();
    }

    protected override void OnIterationStart()
    {
        if (WarehouseManager.Instance.ParentGenerated == null) return;

        // Create floor boundaries for spawning
        if (appParam == null)
        {
            appParam = WarehouseManager.Instance.AppParam;
        }
        var bounds = new Bounds(Vector3.zero, new Vector3(appParam.width, 0, appParam.length));
        placer = new SurfaceObjectPlacer(bounds, random, maxPlacementTries);

        // Instantiate boxes at arbitrary location
        if (parentFloorBoxes != null)
        {
            WarehouseManager.Destroy(parentFloorBoxes);
        }

        var existingBoxes = GameObject.FindGameObjectsWithTag("FloorBoxes");
        if (existingBoxes.Length > 0)
        {
            foreach (var b in existingBoxes)
            {
                WarehouseManager.Destroy(b);
            }
        }

        parentFloorBoxes = new GameObject("FloorBoxes");
        parentFloorBoxes.tag = "FloorBoxes";
        for (var i = 0; i < numBoxToSpawn; i++)
        {
            GameObject o = null;
            if (!Application.isPlaying)
            {
#if UNITY_EDITOR
                o = PrefabUtility.InstantiatePrefab(m_ShelfBoxRandomizerShim.GetBoxPrefab()) as GameObject;
                o.transform.parent = parentFloorBoxes.transform;
#endif //UNITY_EDITOR
            }
            else
            {
                o = Object.Instantiate(m_ShelfBoxRandomizerShim.GetBoxPrefab(), parentFloorBoxes.transform);
            }

            o.AddComponent<FloorBoxRandomizerTag>();
            o.AddComponent<RotationRandomizerTag>();
        }

        // Begin placement interation
        placer.IterationStart();

        var tags = tagManager.Query<FloorBoxRandomizerTag>();
        if (!Application.isPlaying)
        {
            tags = Object.FindObjectsOfType<FloorBoxRandomizerTag>();
        }

        foreach (var tag in tags)
        {
            var success = placer.PlaceObject(tag.gameObject);
            if (!success)
            {
                return;
            }
        }
    }

    protected override void OnIterationEnd()
    {
        WarehouseManager.Destroy(parentFloorBoxes);
    }
}
