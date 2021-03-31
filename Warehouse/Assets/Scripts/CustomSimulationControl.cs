using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Perception.Randomization.Randomizers.SampleRandomizers;
using UnityEngine.Perception.Randomization.Samplers;
using UnityEngine.Perception.Randomization.Parameters;
using Unity.Robotics.SimulationControl;

using Object = UnityEngine.Object;

public class CustomSimulationControl : SimulationControlBuilder
{
    SimulationManager simulationManager;
    PerceptionRandomizeScenario scenario;

    public override INode Build()
    {
        simulationManager = GameObject.FindObjectOfType<SimulationManager>();
        scenario = GameObject.FindObjectOfType<PerceptionRandomizeScenario>();

        Parallel root = new Parallel();

        Sequence robot = new Sequence("Turtlebot");
        root.children.Add(robot);
        robot.children.Add(new Skip());

        Sequence sequence = new Sequence();

        Repeat repeat = new Repeat(sequence, 4);
        robot.children.Add(repeat);
        robot.children.Add(new QuitSimulation());

        // Initialize setup
        Sequence init = new Sequence("Initialization");
        init.children.Add(new InitializeSimulation(scenario));
        sequence.children.Add(init);

        // Randomize scene
        Sequence generate = new Sequence("Scene Generation");
        generate.children.Add(new PerceptionRandomize(scenario));
        generate.children.Add(new RealtimeWait(1000));
        sequence.children.Add(generate);  

        return root;
    }
}

public class InitializeSimulation : TaskNode
{
    Param param;
    PerceptionRandomizeScenario scenario;

    public InitializeSimulation(PerceptionRandomizeScenario s)
    {
        scenario = s;
    }
    protected override void Task()
    {
        LoadParams(PlayerPrefs.GetString("selectedParam", "default_params"));
        Object.Destroy(scenario.GetComponent<SimulationManager>());
        Succeed();
        Debug.Log("Finished InitializeSimulation");
    }

    void LoadParams(string selectedFile) 
    {
        Debug.Log($"Loading params from {selectedFile}");
        TextAsset file = Resources.Load<TextAsset>(selectedFile);
        if (file == null) {
            Debug.LogError($"File does not exist {selectedFile}");
            return;
        }

        string dataAsJson = file.text;
        param = JsonUtility.FromJson<Param>(dataAsJson);

        InitializeScenario();
    }

    void InitializeScenario() 
    {
        scenario.constants.randomSeed = param.seed;
        var randomizers = scenario.randomizers;
        foreach(var r in randomizers)
        {
            if (r is SunAngleRandomizer) 
            {
                var sun = (SunAngleRandomizer)r;
                sun.hour = new FloatParameter { value = new UniformSampler(param.sunAngle.hour[0], param.sunAngle.hour[1]) };
                sun.dayOfTheYear = new FloatParameter { value = new UniformSampler(param.sunAngle.dayOfTheYear[0], param.sunAngle.dayOfTheYear[1]) };
                sun.latitude = new FloatParameter { value = new UniformSampler(param.sunAngle.latitude[0], param.sunAngle.latitude[1]) };
            }
            else if (r is TestLocalRotationRandomizer)
            {
                var rot = ((TestLocalRotationRandomizer)r).rotation;
                rot.x = new UniformSampler(param.objectRotation.x[0], param.objectRotation.x[1]);
                rot.y = new UniformSampler(param.objectRotation.y[0], param.objectRotation.y[1]);
                rot.z = new UniformSampler(param.objectRotation.z[0], param.objectRotation.z[1]);
            }
            else if (r is TestRobotPlacementRandomizer) {
                ((TestRobotPlacementRandomizer)r).distFromEdge = param.robotPlacementDist;
            }
            else if (r is MaterialRandomizer) {
                
            }
            else if (r is ShelfBoxRandomizer) {
                ((ShelfBoxRandomizer)r).boxSpawnChance = param.boxSpawnChance;
            }
        }
    }
}

public class Skip : TaskNode
{
    bool isSkipped = false;

    protected override void Task()
    {
        if (!isSkipped)
        {
            isSkipped = true;
        }
        else
        {
            Succeed();
        }
    }
}