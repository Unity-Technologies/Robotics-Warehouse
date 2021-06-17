using UnityEngine.Perception.Randomization.Randomizers;

namespace Unity.Robotics.PerceptionRandomizers.Shims
{
    public class RandomizerShim : Randomizer
    {
        // Because Randomizer's methods are protected, we must use a derived class to give our editor
        // extensions access to protected methods for execution in EditMode, and since our Runtime
        // elements need to use this class definition, we define the Editor function in Runtime code
#if UNITY_EDITOR
        public void OnEditorIteration()
        {
            // End "previous" iteration
            OnIterationEnd();
            OnScenarioComplete();

            // Run through randomizer lifecycle
            OnAwake();
            OnScenarioStart();
            OnIterationStart();
            OnUpdate();
        }
#endif
    }
}
