using System;
using UnityEngine;
using UnityEngine.Perception.Randomization.Scenarios;
using UnityEngine.UI;

namespace Unity.Simulation.Warehouse
{
    public class BoxTowerSpawner : MonoBehaviour
    {
        public GameObject locationPicker;

        public Toggle showPickerToggle;
        public InputField boxField;
        Vector3 boxDims;
        GameObject spawnedBoxes;

        // Start is called before the first frame update
        void Start()
        {
            showPickerToggle.onValueChanged.AddListener(delegate { OnValueChange(); });

            // Turn on Canvas GameObject during runtime
            transform.Find("Canvas").gameObject.SetActive(true);
        }

        // Button OnClick for spawning box towers
        public void SpawnBoxes()
        {
            // Parse dimensions of box tower
            var boxIn = boxField.text.Split(',');
            if (boxIn.Length != 3)
            {
                Debug.LogError("Invalid box input dimensions!");
                return;
            }

            if (spawnedBoxes != null)
                Destroy(spawnedBoxes);

            spawnedBoxes = new GameObject("SpawnedBoxes");
            boxDims = new Vector3(int.Parse(boxIn[0]), int.Parse(boxIn[1]), int.Parse(boxIn[2]));

            // Grab random box prefab
            var scenario = FindObjectOfType<Scenario<ScenarioConstants>>();
            var boxPrefab = scenario.GetRandomizer<ShelfBoxRandomizerShim>().GetBoxPrefab();
            var boxSize = boxPrefab.GetComponentInChildren<Renderer>().bounds.size;

            // Instantiate boxes
            for (var i = 0; i < boxDims[0]; i++)
                for (var j = 0; j < boxDims[1]; j++)
                    for (var k = 0; k < boxDims[2]; k++)
                    {
                        var o = Instantiate(boxPrefab, new Vector3(i * boxSize.x, k * boxSize.y, j * boxSize.z),
                            Quaternion.identity, spawnedBoxes.transform);
                        boxPrefab = scenario.GetRandomizer<ShelfBoxRandomizerShim>().GetBoxPrefab();
                    }

            spawnedBoxes.transform.position = locationPicker.transform.position;
        }

        // Delegate for updating Location Picker status
        void OnValueChange()
        {
            locationPicker.SetActive(showPickerToggle.isOn);
        }
    }
}
