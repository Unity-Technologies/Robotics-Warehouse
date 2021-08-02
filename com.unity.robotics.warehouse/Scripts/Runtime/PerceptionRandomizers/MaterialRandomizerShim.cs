using System;
using Unity.Robotics.PerceptionRandomizers.Shims;
using UnityEngine;
using UnityEngine.Perception.Randomization.Randomizers;
using Object = UnityEngine.Object;

[Serializable]
public class MaterialToFriction
{
    public Material material;
    public float dynamicFriction;
    public float staticFriction;
}

/// <summary>
///     Randomizes the material texture of objects tagged with a TextureRandomizerTag
/// </summary>
[Serializable]
[AddRandomizerMenu("Robotics/Material Randomizer")]
public class MaterialRandomizerShim : RandomizerShim
{
    public MaterialToFrictionParameter material;

    /// <summary>
    ///     Randomizes the material texture of tagged objects at the start of each scenario iteration
    /// </summary>
    protected override void OnIterationStart()
    {
        var tags = tagManager.Query<MaterialRandomizerTag>();
        if (!Application.isPlaying)
        {
            tags = Object.FindObjectsOfType<MaterialRandomizerTag>();
        }
        var sample = material.Sample();
        foreach (var tag in tags)
        {
            var renderer = tag.GetComponent<Renderer>();
            renderer.material = sample.material;

            var collider = tag.GetComponent<Collider>();
            collider.material.dynamicFriction = sample.dynamicFriction;
            collider.material.staticFriction = sample.staticFriction;
        }
    }
}
