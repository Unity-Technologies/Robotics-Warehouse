using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEditor;

using Debug = UnityEngine.Debug;

public enum SimulationType { BUILD, EDITOR }
public class ParamReader
{
    // #if UNITY_EDITOR
    public static void ReadParamFromCLI()
    {
        string[] arguments = Environment.GetCommandLineArgs();

        int startIdx = Array.IndexOf(arguments, "ParamReader.ReadParamFromCLI");

        var simType = (SimulationType)Enum.Parse(typeof(SimulationType), arguments[startIdx + 1]);
        
        PlayerPrefs.SetString("selectedParam", arguments[startIdx + 2]);
        Debug.Log($"Selecting file: {PlayerPrefs.GetString("selectedParam")}");

        switch (simType) 
        {
            // case SimulationType.BUILD:
            //     PerformBuild();
            //     break;
            case SimulationType.EDITOR:
                EditorApplication.ExecuteMenuItem("Edit/Play");
                break;
            default:
                break;
        }
    }

    // static void PerformBuild()
    // {
    //     // Get filename.
    //     string path = Application.persistentDataPath;
    //     string[] levels = new string[] {"Assets/Scenes/Test.unity"};

    //     Debug.Log($"Built file to {path + "/BuiltTest.app"}");

    //     // Build player.
    //     BuildPipeline.BuildPlayer(levels, path + "/BuiltTest.app", BuildTarget.StandaloneOSX, BuildOptions.None);
    // }

    // #endif // UNITY_EDITOR
}
