using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Perception.GroundTruth;
using Unity.Robotics.SimulationControl;

namespace Unity.Simulation.Warehouse {
    public class RigidbodySpawner : MonoBehaviour
    {
        public GameObject locationPickerPrefab;
        private GameObject locationPicker;
        private Vector3 boxDims;
        private int debrisSpawn;
        private GameObject spawned;
        private Bounds pickerArea;
        private bool wasDebris = false;
        private float debrisSize = 0.05f; 
        private bool debrisKinematic = false;

        public Toggle showPickerToggle;
        public InputField boxField;
        public Slider debrisSizeSlider;
        public Text debrisSizeText;
        public Toggle debrisKinematicToggle;
        public InputField debrisField;

        // Start is called before the first frame update
        void Start()
        {
            showPickerToggle.onValueChanged.AddListener(delegate { OnValueChange(); });
            debrisSizeSlider.onValueChanged.AddListener(delegate { OnValueChange(); });
            debrisKinematicToggle.onValueChanged.AddListener(delegate { OnValueChange(); });

            if (locationPicker == null)
            {
                locationPicker = Instantiate(locationPickerPrefab);
                pickerArea = locationPicker.GetComponent<Renderer>().bounds;
            }
            locationPicker.SetActive(false);

            // Turn on Canvas GameObject during runtime
            transform.Find("Canvas").gameObject.SetActive(true);
            debrisSizeSlider.value = debrisSize;
            debrisSizeText.text = debrisSize.ToString("0.000");
        }

        // Button OnClick for spawning box towers
        public void SpawnBoxes()
        {
            wasDebris = false;

            // Parse dimensions of box tower
            var boxIn = boxField.text.Split(',');
            if (boxIn.Length != 3)
            {
                Debug.LogError($"Invalid box input dimensions!");
                return;
            }

            if (spawned != null) 
                Destroy(spawned);
                
            spawned = new GameObject("Spawned");
            boxDims = new Vector3(int.Parse(boxIn[0]), int.Parse(boxIn[1]), int.Parse(boxIn[2]));

            // Grab random box prefab
            var scenario = GameObject.FindObjectOfType<PerceptionRandomizationScenario>();
            var boxPrefab = scenario.GetRandomizer<ShelfBoxRandomizer>().GetBoxPrefab();
            var boxSize = boxPrefab.GetComponentInChildren<Renderer>().bounds.size;

            // Instantiate boxes
            for (int i = 0; i < boxDims[0]; i++)
            {
                for (int j = 0; j < boxDims[1]; j++)
                {
                    for (int k = 0; k < boxDims[2]; k++)
                    {
                        var o = Instantiate(boxPrefab, new Vector3(i * boxSize.x, k * boxSize.y, j * boxSize.z), Quaternion.identity, spawned.transform);
                        boxPrefab = scenario.GetRandomizer<ShelfBoxRandomizer>().GetBoxPrefab();
                    }
                }
            }

            spawned.transform.position = locationPicker.transform.position;
        }

        // Button OnClick for spawning random primitives
        public void SpawnDebris()
        {
            wasDebris = true;
            pickerArea = locationPicker.GetComponent<Renderer>().bounds;
            debrisSpawn = int.Parse(debrisField.text);

            if (spawned != null) 
            {
                Destroy(spawned);
            }
                
            spawned = new GameObject("Spawned");

            // Instantiate random primitives 
            for (int i = 0; i < debrisSpawn; i++)
            {
                var randShape = Random.Range(0, 4);
                GameObject obj;
                switch (randShape)
                {
                    case 0:
                        obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        break;
                    case 1:
                        obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        break;
                    case 2:
                        obj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                        break;
                    case 3:
                        obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        break;
                    default:
                        obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        break;
                }

                // Modify transform based on user input
                var pos = new Vector3(Random.Range(pickerArea.center.x - pickerArea.extents.x, pickerArea.center.x + pickerArea.extents.x), debrisSize/2, Random.Range(pickerArea.center.z - pickerArea.extents.z, pickerArea.center.z + pickerArea.extents.z));
                
                obj.transform.localScale = new Vector3(Random.Range(0.005f, debrisSize), Random.Range(0.005f, debrisSize), Random.Range(0.005f, debrisSize));
                obj.transform.parent = spawned.transform;
                obj.transform.localPosition = pos;
                obj.transform.rotation = Random.rotation;
                obj.GetComponent<Renderer>().material = Resources.Load<Material>($"Materials/Debris");

                var lab = obj.AddComponent<Labeling>();
                lab.labels.Add("debris");

                var rb = obj.AddComponent<Rigidbody>();
                rb.isKinematic = debrisKinematic;
            }

            spawned.transform.position = locationPicker.transform.position;
        }

        // Delegate for updating spawned objects
        void OnValueChange()
        {
            locationPicker.SetActive(showPickerToggle.isOn);
            debrisSize = debrisSizeSlider.value;
            debrisSizeText.text = debrisSize.ToString("0.000");
            debrisKinematic = debrisKinematicToggle.isOn;

            // Resize debris if appropriate
            if (wasDebris)
            {
                for (int i = 0; i < spawned.transform.childCount; i++)
                {
                    var o = spawned.transform.GetChild(i);
                    o.localScale = new Vector3(Random.Range(0.005f, debrisSize), Random.Range(0.005f, debrisSize), Random.Range(0.005f, debrisSize));
                    o.GetComponent<Rigidbody>().isKinematic = debrisKinematic;
                }
            }
        }
    };
}