using System;
using UnityEngine;
using UnityEngine.Perception.GroundTruth;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Unity.Simulation.Warehouse
{
    public class DebrisSpawner : MonoBehaviour
    {
        const float k_MinDebrisSize = 0.005f;
        public GameObject locationPicker;

        public Slider debrisSizeSlider;
        public Text debrisSizeText;
        public Toggle debrisKinematicToggle;
        public InputField debrisField;
        bool debrisKinematic;
        float debrisSize = 0.05f;
        int debrisSpawn;
        Bounds pickerArea;
        GameObject spawnedDebris;

        // Start is called before the first frame update
        void Start()
        {
            debrisSizeSlider.onValueChanged.AddListener(delegate { OnValueChange(); });
            debrisKinematicToggle.onValueChanged.AddListener(delegate { OnValueChange(); });

            pickerArea = locationPicker.GetComponent<Renderer>().bounds;

            debrisSizeSlider.value = debrisSize;
            debrisSizeText.text = debrisSize.ToString("0.000");
            
            // If Canvas is disabled, disable this behaviour as well
            var canvas = transform.Find("Canvas").gameObject;
            enabled = canvas.activeInHierarchy;
        }

        // Button OnClick for spawning random primitives
        public void SpawnDebris()
        {
            debrisSpawn = int.Parse(debrisField.text);

            if (spawnedDebris != null)
            {
                Destroy(spawnedDebris);
            }

            spawnedDebris = new GameObject("SpawnedDebris");

            // Instantiate random primitives
            for (var i = 0; i < debrisSpawn; i++)
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
                var pos = new Vector3(
                    Random.Range(pickerArea.center.x - pickerArea.extents.x,
                        pickerArea.center.x + pickerArea.extents.x), debrisSize / 2,
                    Random.Range(pickerArea.center.z - pickerArea.extents.z,
                        pickerArea.center.z + pickerArea.extents.z));

                obj.transform.localScale = new Vector3(Random.Range(k_MinDebrisSize, debrisSize),
                    Random.Range(k_MinDebrisSize, debrisSize), Random.Range(k_MinDebrisSize, debrisSize));
                obj.transform.parent = spawnedDebris.transform;
                obj.transform.localPosition = pos;
                obj.transform.rotation = Random.rotation;
                obj.GetComponent<Renderer>().material = Resources.Load<Material>("Materials/Debris");

                var lab = obj.AddComponent<Labeling>();
                lab.labels.Add("debris");

                var rb = obj.AddComponent<Rigidbody>();
                rb.isKinematic = debrisKinematic;
            }

            spawnedDebris.transform.position = locationPicker.transform.position;
        }

        // Delegate for updating spawned debris
        void OnValueChange()
        {
            debrisSize = debrisSizeSlider.value;
            debrisSizeText.text = debrisSize.ToString("0.000");
            debrisKinematic = debrisKinematicToggle.isOn;

            // Resize debris if appropriate
            if (spawnedDebris != null)
                for (var i = 0; i < spawnedDebris.transform.childCount; i++)
                {
                    var o = spawnedDebris.transform.GetChild(i);
                    o.localScale = new Vector3(Random.Range(k_MinDebrisSize, debrisSize),
                        Random.Range(k_MinDebrisSize, debrisSize),
                        Random.Range(k_MinDebrisSize, debrisSize));
                    o.GetComponent<Rigidbody>().isKinematic = debrisKinematic;
                }
        }
    }
}
