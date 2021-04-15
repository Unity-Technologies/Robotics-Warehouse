using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEditor;
using Unity.Simulation.Client;

#if UNITY_EDITOR

namespace Unity.Simulation.Warehouse {
    public class ParamGenerator : EditorWindow {
        private static Thread _thread;
        private static string _dataDownloadLocation;
        private static bool runStarted;
        private static Run run;
        private static int _runMonitorInterval = 10;
        private static float _timeElapsed = 0.0f;
        private static float _downloadProgress = 0;
        
        private int _filename = 0;
        
        private string _width;
        private string _length;
        private string _rows;
        private string _cols;
        private bool _horizontal;
        private bool _vertical;
        private string _numbots;
        private string _dropoff;
        private bool _light_randomized;
        private bool _light_morning;
        private bool _light_afternoon;
        private bool _light_evening;
        private bool _light_none;
        private string _percentLight;

        private int _quitAfterSeconds;

        private List<string> _width_parsed;
        private List<string> _length_parsed;
        private List<string> _rows_parsed;
        private List<string> _cols_parsed;
        private List<string> _horizontal_parsed;
        private List<string> _numbots_parsed;
        private List<string> _dropoff_parsed;
        private List<string> _light_parsed;
        private List<string> _percentLight_parsed;

        private Rect _rectButtonGen;

        #region MENU

        [MenuItem("Simulation/Build Project", false, 0)]
        public static void BuildProject()
        {
            var scenes = new string[]
            {
                "Assets/Warehouse Robot/Scenes/Warehouse.unity"
            };
            Project.BuildProject("./build", "build", scenes);
        }

        [MenuItem("Simulation/Execute Run", false, 0)]
        public static void Setup()
        {
            run = Run.Create("Warehouse_Robot", "Run");
            var sysParam = API.GetSysParams()[0];
            run.SetSysParam(sysParam);
            var buildPath = System.IO.Path.Combine(Application.dataPath, "..", "build.zip");
            if (File.Exists(buildPath)) run.SetBuildLocation(buildPath); 
            else {
                Debug.LogError("Build file does not exist at: " + buildPath); 
                return;
            }

            string[] files = Directory.GetFiles(Application.dataPath + "/StreamingAssets");
            for (int i = 0; i < files.Length; i++){
                if (files[i].Contains("app_param") && !files[i].Contains("meta")){
                    Debug.Log(files[i]);
                    string json = json = File.ReadAllText(files[i]); 
                    Debug.Log(json);
                    run.SetAppParam("simulation-app-param-" + i, json, 1);
                }
            }

            EditorApplication.update += MonitorRunExecution;
        
            _thread = new Thread(new ThreadStart(() =>
            {
                Debug.Log("Uploading the build and scheduling the simulation run");
                if (!runStarted)
                {
                    run.Execute();
                    runStarted = true;
                }
            }));
            _thread.Start();
        }

        
        private static void MonitorRunExecution()
        {
            _timeElapsed += Time.deltaTime;
            
            if (_timeElapsed < _runMonitorInterval)
                return;

            _timeElapsed = 0;
            
            if (runStarted)
            {
                if (run.completed)
                {
                    _dataDownloadLocation = Application.persistentDataPath + "/SimulationRuns/" + run.executionId;
                    Debug.Log(_dataDownloadLocation);
                    if (!Directory.Exists(_dataDownloadLocation))
                        Directory.CreateDirectory(_dataDownloadLocation);
                    
                    var download = EditorUtility.DisplayDialog("Simulation Run",
                        "The simulation run for " + run.executionId + " is complete", "Download Manifest", "Cancel");

                    if (download)
                    {
                        var manifest = API.GetManifest(run.executionId);
                        DownloadData(manifest);

                        while (_downloadProgress < manifest.Count)
                        {
                            EditorUtility.DisplayProgressBar("Downloading Files", "Downloading Data generated in USim", _downloadProgress/manifest.Count);
                        }
                        
                        EditorUtility.ClearProgressBar();
                    }

                    runStarted = false;
                    EditorApplication.update -= MonitorRunExecution;
                }
            }
            else
            {
                Debug.Log("Uploading Build to USim");
            }
        }

        private static void DownloadData(Dictionary<int, ManifestEntry> manifest)
        {
            var thread = new Thread(new ThreadStart(() =>
            {
                var wc = new WebClient();
                
                foreach (var entry in manifest)
                {
                    var e = entry.Value;

                    var subDir = e.fileName.Split('/');
                    if (subDir.Length > 1)
                    {
                        if (!Directory.Exists(_dataDownloadLocation + '/' + e.appParamId + '/' + subDir[0]))
                            Directory.CreateDirectory(_dataDownloadLocation + '/' + e.appParamId + '/' + subDir[0]);
                    }
                    wc.DownloadFile(e.downloadUri, _dataDownloadLocation + '/' + e.appParamId + '/' + subDir[0] + '/' + subDir[subDir.Length-1]);
                    _downloadProgress++;
                }
            }));
            
            thread.Start();
            
        }

        [MenuItem("Simulation/Generate Params", false, 0)]
        static void ShowEditor() {
            ParamGenerator editor = EditorWindow.GetWindow<ParamGenerator>();
        }

        #endregion // MENU

        void OnGUI()
        {
            Event e = Event.current;

            // Control buttons
            _rectButtonGen = new Rect(new Vector2(10, 325), new Vector2(200, 50));
            
            DrawButtons();
            _width = EditorGUILayout.TextField("Width of warehouse", _width);
            _length = EditorGUILayout.TextField("Length of warehouse", _length);
            _rows = EditorGUILayout.TextField("Number of rows", _rows);
            _cols = EditorGUILayout.TextField("Number of columns", _cols);
            _horizontal = EditorGUILayout.ToggleLeft("Horizontal shelf layout?", _horizontal);
            _vertical = EditorGUILayout.ToggleLeft("Vertical shelf layout?", _vertical);
            _numbots = EditorGUILayout.TextField("Number of bots", _numbots);
            _dropoff = EditorGUILayout.TextField("Number of dropoff stations on each wall", _dropoff);
            _light_morning = EditorGUILayout.ToggleLeft("Morning lighting", _light_morning);
            _light_afternoon = EditorGUILayout.ToggleLeft("Afternoon lighting", _light_afternoon);
            _light_evening = EditorGUILayout.ToggleLeft("Evening lighting", _light_evening);
            _light_randomized = EditorGUILayout.ToggleLeft("Randomized lighting", _light_randomized);
            _light_none = EditorGUILayout.ToggleLeft("No lighting", _light_none);
            _percentLight = EditorGUILayout.TextField("% of ceiling lights on", _percentLight);
            _quitAfterSeconds = EditorGUILayout.IntSlider("quitAfterSeconds", _quitAfterSeconds, 30, 600);
        }

        private void DrawButtons()
        {
            if (GUI.Button(_rectButtonGen, "Generate"))
                Generate();
        }

        // https://stackoverflow.com/questions/545703/combination-of-listlistint
        private static List<List<T>> GenerateCombinations<T>(List<List<T>> collectionOfSeries){
            List<List<T>> generatedCombinations = 
                collectionOfSeries.Take(1)
                                .FirstOrDefault()
                                .Select(i => (new T[]{i}).ToList())                          
                                .ToList();

            foreach (List<T> series in collectionOfSeries.Skip(1))
            {
                generatedCombinations = 
                    generatedCombinations
                        .Join(series as List<T>,
                                combination => true,
                                i => true,
                                (combination, i) =>
                                    {
                                        List<T> nextLevelCombination = 
                                            new List<T>(combination);
                                        nextLevelCombination.Add(i);
                                        return nextLevelCombination;
                                    }).ToList();

            }

            return generatedCombinations;
        }

        private void Generate() {
            _filename = 0;

            List<List<string>> lists = ParseLists();

            List<List<string>> test = GenerateCombinations(lists);

            for (int i = 0; i < test.Count; i++){
                AppParam tmp = AssignValues(test[i]);

                var json = JsonUtility.ToJson(tmp);
                var path = "Assets/StreamingAssets/app_param_" + _filename + ".json";

                // Create StreamingAssets folder if it doesn't exist
                if (!Directory.Exists("Assets/StreamingAssets/"))
                    Directory.CreateDirectory("Assets/StreamingAssets/");

                using (FileStream fs = new FileStream(path, FileMode.Create)) {
                    using (StreamWriter writer = new StreamWriter(fs)) {
                        writer.Write(json);
                    }
                }
                UnityEditor.AssetDatabase.Refresh();

                _filename++;
            }        
        }

        private List<List<string>> ParseLists(){
            _width_parsed = _width.Split(',').ToList();
            _length_parsed = _length.Split(',').ToList();
            _rows_parsed = _rows.Split(',').ToList();
            _cols_parsed = _cols.Split(',').ToList();
            if (_horizontal && _vertical){ // set m_horizontal to both true and false
                _horizontal_parsed = new List<string> { "true", "false" };
            }
            else if (_horizontal && !_vertical){ // set m_horizontal to ONLY true
                _horizontal_parsed = new List<string> { "true" };
            }
            else if (!_horizontal && _vertical){ // set m_horizontal to ONLY false
                _horizontal_parsed = new List<string> { "false" };
            }
            else {
                Debug.LogError("Need to choose at least one of horizontal and/or vertical!");
                return null;
            }

            _light_parsed = new List<string>();
            if (_light_morning) _light_parsed.Add("Morning");
            if (_light_afternoon) _light_parsed.Add("Afternoon");
            if (_light_evening) _light_parsed.Add("Evening");
            if (_light_randomized) _light_parsed.Add("Randomized");
            if (_light_none) _light_parsed.Add("None");
            
            if (_light_parsed.Count == 0){
                Debug.LogError("Need to choose at least one lighting type!");
                return null;
            }

            _numbots_parsed = _numbots.Split(',').ToList();
            _dropoff_parsed = _dropoff.Split(',').ToList();
            _percentLight_parsed = _percentLight.Split(',').ToList();

            return new List<List<string>>() { _width_parsed, _length_parsed, _rows_parsed, _cols_parsed, _horizontal_parsed, _numbots_parsed, _dropoff_parsed, _light_parsed, _percentLight_parsed };
        }

        private AppParam AssignValues(List<string> lists){
            AppParam tmp = new AppParam();
            tmp.m_width = int.Parse(lists[0]);
            tmp.m_length = int.Parse(lists[1]);
            tmp.m_rows = int.Parse(lists[2]);
            tmp.m_cols = int.Parse(lists[3]);
            // tmp.m_horizontal = bool.Parse(lists[4]);
            tmp.m_numBots = int.Parse(lists[5]);
            tmp.m_dropoff = int.Parse(lists[6]);
            // tmp.m_lighting = Enum.Parse(typeof(LightingType), lists[7]).ToString();
            // tmp.m_percentLight = float.Parse(lists[8]);
            tmp.m_quitAfterSeconds = _quitAfterSeconds;
            
            return tmp;
        }
    }
}
#endif //UNITY_EDITOR