using System;
using Unity.Robotics.PerceptionRandomizers.Shims;
using UnityEngine;
using UnityEngine.Perception.Randomization.Parameters;
using UnityEngine.Perception.Randomization.Randomizers;
using UnityEngine.Perception.Randomization.Samplers;
using Object = UnityEngine.Object;

/// <summary>
///     TODO
/// </summary>
[Serializable]
[AddRandomizerMenu("Robotics/Robot Placement")]
public class RobotPlacementRandomizerShim : RandomizerShim
{
    public GameObject[] floorObjects;
    public GameObject prefabToSpawn;
    public float distFromEdge;
    FloatParameter random = new FloatParameter { value = new UniformSampler(0, 1) };
    GameObject spawnedConstraint;
    GameObject spawnedObject;

    protected override void OnScenarioStart()
    {
        floorObjects = GameObject.FindGameObjectsWithTag("Floor");
        base.OnScenarioStart();
    }

    protected override void OnIterationStart()
    {
        var randIdx = Mathf.FloorToInt(random.Sample() * floorObjects.Length);
        var pt = SamplePoint(floorObjects[randIdx], distFromEdge, 10);
        spawnedObject = Object.Instantiate(prefabToSpawn, pt, Quaternion.identity);
    }

    protected override void OnIterationEnd()
    {
        if (spawnedConstraint == null)
        {
            spawnedConstraint = GameObject.Find("TiltConstraint");
        }
        Object.Destroy(spawnedConstraint);
        Object.Destroy(spawnedObject);
    }

    Vector3 SamplePoint(GameObject obj, float edge, int maxAttempts)
    {
        var bounds = obj.GetComponent<Renderer>().bounds;
        var attempts = 0;
        var scaledExtents = Vector3.Scale(bounds.extents, obj.transform.localScale);

        while (attempts < maxAttempts)
        {
            Vector3 pt;

            pt.x = edge > bounds.extents.x
                ? bounds.center.x
                : random.Sample() * (bounds.extents.x * 2 - edge * 2) + (bounds.center.x - bounds.extents.x);
            pt.y = bounds.center.y + bounds.extents.y;
            pt.z = edge > bounds.extents.z
                ? bounds.center.z
                : random.Sample() * (bounds.extents.z * 2 - edge * 2) + (bounds.center.z - bounds.extents.z);

            // TODO: bounding box check
            attempts++;
            return pt;
        }

        return Vector3.zero;
    }
}
