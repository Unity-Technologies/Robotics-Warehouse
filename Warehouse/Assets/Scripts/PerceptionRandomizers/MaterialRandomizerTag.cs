using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Perception.Randomization.Randomizers;

namespace UnityEngine.Perception.Randomization.Randomizers.SampleRandomizers
{
    /// <summary>
    /// Used in conjunction with a TextureRandomizer to vary the material texture of GameObjects
    /// </summary>
    [Serializable]
    [AddComponentMenu("Perception/RandomizerTags/Material Randomizer Tag")]
    public class MaterialRandomizerTag : RandomizerTag { }
}