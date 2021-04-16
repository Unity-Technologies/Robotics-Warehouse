using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Perception.Randomization.Randomizers;
using UnityEngine.Perception.Randomization.Parameters;
using UnityEngine.Perception.Randomization.Samplers;
using Unity.Robotics.SimulationControl;

using Object = UnityEngine.Object;

[Serializable]
[AddRandomizerMenu("Robotics/Shelf Box Randomizer")]
public class ShelfBoxRandomizer : PerceptionRandomizer
{
    public GameObjectParameter boxParameter;
    [Range(0, 1f)] public float boxSpawnChance = 0.5f;
    FloatParameter boxSpawnParam = new FloatParameter { value = new UniformSampler(0, 1f) };
    readonly Vector3 boxScale = new Vector3(0.9f, 0.9f, 0.9f);
    List<GameObject> spawnedBoxes = new List<GameObject>();

    protected override void OnScenarioStart()
    {
        // TODO: why does query return nothing in editor mode?
        var tags = tagManager.Query<ShelfBoxRandomizerTag>();
        if (!Application.isPlaying)
            tags = GameObject.FindObjectsOfType<ShelfBoxRandomizerTag>();
        foreach (var tag in tags)
        {
            tag.AssignMemberLayers();
        }
        base.OnScenarioStart();
    }

    protected override void OnIterationStart()
    {
        var tags = tagManager.Query<ShelfBoxRandomizerTag>();
        if (!Application.isPlaying)
            tags = GameObject.FindObjectsOfType<ShelfBoxRandomizerTag>();
        foreach (var tag in tags)
        {
            foreach (Transform[] layer in tag.layers) 
            {
                foreach (Transform t in layer) 
                {
                    if (boxSpawnParam.Sample() <= boxSpawnChance) {
                        var box = Object.Instantiate(boxParameter.Sample(), t);
                        box.transform.localScale = boxScale;
                        spawnedBoxes.Add(box);
                    }
                }
            }
        }
    }

    protected override void OnIterationEnd()
    {
        foreach (var b in spawnedBoxes)
        {
            if (!Application.isPlaying)
                Object.DestroyImmediate(b);
            else
                Object.Destroy(b);
        }
        spawnedBoxes.Clear();
    }

    public GameObject GetBoxPrefab()
    {
        return boxParameter.Sample();
    }
}