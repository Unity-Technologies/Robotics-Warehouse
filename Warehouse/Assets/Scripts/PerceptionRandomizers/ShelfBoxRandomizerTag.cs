using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Perception.Randomization.Randomizers;

[Serializable]
[AddComponentMenu("Perception/RandomizerTags/Shelf Box Randomizer Tag")]
public class ShelfBoxRandomizerTag : RandomizerTag { 
    public List<Transform[]> layers;
    Transform[] layer0;
    Transform[] layer1;
    Transform[] layer2;
    Transform[] layer3;

    public void AssignMemberLayers() 
    {
        layer0 = GetChildTransforms(transform.Find("layer0"));
        layer1 = GetChildTransforms(transform.Find("layer1"));
        layer2 = GetChildTransforms(transform.Find("layer2"));
        layer3 = GetChildTransforms(transform.Find("layer3"));
        layers = new List<Transform[]>() { layer0, layer1, layer2, layer3 };

        base.OnEnable();
    }

    Transform[] GetChildTransforms(Transform t)
    {
        var children = new List<Transform>();
        foreach (Transform tr in t) 
        {
            children.Add(tr);
        }
        return children.ToArray();
    }
}