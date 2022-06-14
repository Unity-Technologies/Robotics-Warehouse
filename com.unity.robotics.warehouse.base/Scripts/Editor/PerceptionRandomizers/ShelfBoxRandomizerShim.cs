using System;
using System.Collections.Generic;
using Unity.Robotics.PerceptionRandomizers.Shims;
using Unity.Simulation.Warehouse;
using UnityEngine;
using UnityEngine.Perception.Randomization.Parameters;
using UnityEngine.Perception.Randomization.Randomizers;
using UnityEngine.Perception.Randomization.Samplers;
using Object = UnityEngine.Object;

[Serializable]
[AddRandomizerMenu("Robotics/Shelf Box Randomizer")]
public class ShelfBoxRandomizerShim : RandomizerShim
{
    public GameObjectParameter boxParameter;
    [Range(0, 1f)]
    public float boxSpawnChance = 0.5f;
    readonly Vector3 boxScale = new Vector3(0.9f, 0.9f, 0.9f);
    FloatParameter boxSpawnParam = new FloatParameter { value = new UniformSampler(0, 1f) };
    List<GameObject> spawnedBoxes = new List<GameObject>();

    protected override void OnIterationStart()
    {
        var tags = tagManager.Query<ShelfBoxRandomizerTag>();
        if (!Application.isPlaying)
        {
            tags = Object.FindObjectsOfType<ShelfBoxRandomizerTag>();
        }

        foreach (var tag in tags)
        {
            tag.AssignMemberLayers();
            foreach (var layer in tag.layers)
                foreach (var posTransform in layer)
                {
                    // Check if boxes are already spawned
                    if (posTransform.childCount > 0)
                    {
                        foreach (Transform child in posTransform)
                        {
                            WarehouseManager.Destroy(child.gameObject);
                        }
                    }

                    // Spawn boxes
                    if (boxSpawnParam.Sample() <= boxSpawnChance)
                    {
                        var box = Object.Instantiate(boxParameter.Sample(), posTransform);
                        box.transform.localScale = boxScale;
                        spawnedBoxes.Add(box);
                    }
                }
        }
    }

    protected override void OnIterationEnd()
    {
        foreach (var b in spawnedBoxes)
        {
            WarehouseManager.Destroy(b);
        }
        spawnedBoxes.Clear();
    }

    public GameObject GetBoxPrefab()
    {
        return boxParameter.Sample();
    }
}
