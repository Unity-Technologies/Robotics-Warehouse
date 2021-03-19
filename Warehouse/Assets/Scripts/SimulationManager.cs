using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Perception.Randomization.Randomizers.SampleRandomizers;
using UnityEngine.Perception.Randomization.Samplers;
using UnityEngine.Perception.Randomization.Parameters;

public class SimulationManager : MonoBehaviour
{
    public GameObject goalObj;
    Param param;
    AMRScenario scenario;
    bool isInitialized = false;

    // Start is called before the first frame update
    void Start()
    {
        scenario = GetComponent<AMRScenario>();
        LoadParams(PlayerPrefs.GetString("selectedParam", "default_params"));
        scenario.Randomize();
        AssignColliders();
    }

    // Update is called once per frame
    void Update()
    {
        if (isInitialized) 
        {
            if (Input.GetKeyDown(KeyCode.Space)) 
            {
                GenerateEnvironment();
            }
        }
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

        isInitialized = true;
    }

    void AssignColliders()
    {
        var colliders = FindObjectsOfType<Collider>();
 
        foreach (Collider c in colliders)
        {
            if (!c.name.Contains("Floor"))
            {
                if (c.GetComponent<CollisionPrint>() == null)
                    c.gameObject.AddComponent<CollisionPrint>();
            }
        }
    }

    public void GenerateEnvironment() 
    {
        scenario.Randomize();
        AssignColliders();
    }
}
