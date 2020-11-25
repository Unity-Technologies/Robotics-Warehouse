using System;
using UnityEngine;
using Unity.Simulation;

public class ParamReader : MonoBehaviour
{
    // Loggers
    Unity.Simulation.Logger _paramLogger;

    // App Params
    public static AppParam appParams = new AppParam();

    // Internal tracking
    float _simElapsedSeconds;
    
    void Awake()
    {
        // Create loggers for AppParams for debugging purposes
        _paramLogger = new Unity.Simulation.Logger("ParamReader");
        _simElapsedSeconds = 0;

        // NOTE: AppParams can be loaded anytime except during `RuntimeInitializeLoadType.BeforeSceneLoad`
        // If the simulation is running locally load app_param_1.json
        if (Configuration.Instance.IsSimulationRunningInCloud()) {
            appParams = Configuration.Instance.GetAppParams<AppParam>();
        } else {
            appParams = GameObject.FindObjectOfType<WarehouseManager>().GetEditorParams();
            // var appParamFilename = "default_app_param.json";
            // Configuration.Instance.SimulationConfig.app_param_uri = string.Format("file://{0}/StreamingAssets/{1}", Application.dataPath, appParamFilename) ;
            // Debug.Log(Configuration.Instance.SimulationConfig.app_param_uri);
        }


        // Check if AppParam file was passed during command line execution
        if (appParams != null)
        {
            // Log AppParams to Player.Log file and Editor Console
            Debug.Log(appParams.ToString());

            // Log AppParams to DataLogger
            _paramLogger.Log(appParams);
            _paramLogger.Flushall();
        }
    }

    // Exit sim after simulation has ran for quitAfterSeconds defined in AppParams.
    void Update()
    {
        _simElapsedSeconds += Time.deltaTime;

        if (_simElapsedSeconds >= appParams.m_quitAfterSeconds)
        {
            Debug.Log($"Sim elapsed timeout: {_simElapsedSeconds}");
            Application.Quit();
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }
    }

    public float GetElapsedSeconds(){ return _simElapsedSeconds; }
}
