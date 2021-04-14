using System;
using UnityEngine;
using UnityEngine.Perception.Randomization.Parameters;
using UnityEngine.Perception.Randomization.Randomizers;
using UnityEngine.Perception.Randomization.Randomizers.SampleRandomizers.Tags;
using UnityEngine.Perception.Randomization.Samplers;
using Unity.Robotics.SimulationControl;

/// <summary>
/// Randomizes the rotation of objects tagged with a RotationRandomizerTag
/// </summary>
[Serializable]
[AddRandomizerMenu("Perception/Local Rotation Randomizer")]
public class LocalRotationRandomizer : PerceptionRandomizer
{
    /// <summary>
    /// Defines the range of random rotations that can be assigned to tagged objects
    /// </summary>
    public Vector3Parameter rotation = new Vector3Parameter
    {
        x = new UniformSampler(0, 360),
        y = new UniformSampler(0, 360),
        z = new UniformSampler(0, 360)
    };

    /// <summary>
    /// Randomizes the rotation of tagged objects at the start of each scenario iteration
    /// </summary>
    protected override void OnIterationStart()
    {
        var tags = tagManager.Query<RotationRandomizerTag>();
        if (!Application.isPlaying)
                tags = GameObject.FindObjectsOfType<RotationRandomizerTag>();
        foreach (var taggedObject in tags)
            taggedObject.transform.localRotation = Quaternion.Euler(rotation.Sample());
    }
}
