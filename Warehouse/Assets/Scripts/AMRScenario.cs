using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Perception.Randomization.Scenarios;

[Serializable]
public class AMRScenarioConstants : ScenarioConstants
{
    public int totalFrames = 1000;
}

public class AMRScenario : Scenario<AMRScenarioConstants>
{
    public bool automaticIteration = false;
    
    bool shouldIterate = false; 

    public void Randomize()
    {
        shouldIterate = true;
    }

    protected override void IncrementIteration()
    {
        base.IncrementIteration();
        shouldIterate = false;
    }

    protected override bool isScenarioReadyToStart => shouldIterate;
    protected override bool isIterationComplete => automaticIteration || shouldIterate;
    protected override bool isScenarioComplete => currentIteration >= constants.totalFrames;
}


