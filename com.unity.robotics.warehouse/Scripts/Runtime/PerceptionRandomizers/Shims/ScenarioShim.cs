using System;
using UnityEngine;
using UnityEngine.Perception.Randomization.Scenarios;

namespace Unity.Robotics.PerceptionRandomizers.Shims
{
    public class ScenarioShim : Scenario<ScenarioConstants>
    {
        bool m_ShouldRandomize;
        protected override bool isScenarioReadyToStart => m_ShouldRandomize;
        protected override bool isIterationComplete => m_ShouldRandomize;
        protected override bool isScenarioComplete => false;

        public void RandomizeOnce()
        {
#if UNITY_EDITOR
            m_ShouldRandomize = true;
            EditorIteration();
            m_ShouldRandomize = false;
#endif
        }

#if UNITY_EDITOR
        void EditorIteration()
        {
            foreach (var randomizer in activeRandomizers)
                if (randomizer is RandomizerShim extended)
                    extended.OnEditorIteration();
                else
                    Debug.LogWarning(
                        $"{randomizer} is not a {nameof(RandomizerShim)} and can't be randomized manually in editor.");
        }
#endif
    }
}
