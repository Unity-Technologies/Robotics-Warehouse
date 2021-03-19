using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Perception.GroundTruth;
using RosSharp;

namespace Unity.Simulation.Warehouse {
    public class RigidbodySpawner : MonoBehaviour
    {
        public GameObject locationPicker;
        public GameObject box;
        private Vector3 boxDims;
        private int debrisSpawn;
        private GameObject spawned;
        private Bounds pickerArea;

        // Start is called before the first frame update
        void Start()
        {
            ShowPicker(false);
        }

        public void SpawnBoxes()
        {
            pickerArea = locationPicker.GetComponent<Renderer>().bounds;
            var boxIn = transform.Find("SpawnBoxes").GetComponent<InputField>().text.Split(',');
            if (boxIn.Length != 3){
                Debug.LogError($"Invalid box input dimensions!");
                return;
            }

            if (spawned != null) 
            {
                Destroy(spawned);
            }
                
            spawned = new GameObject("Spawned");
            boxDims = new Vector3(int.Parse(boxIn[0]), int.Parse(boxIn[1]), int.Parse(boxIn[2]));
            
            var boxSize = box.GetComponentInChildren<Renderer>().bounds.size;
            for (int i = 0; i < boxDims[0]; i++)
            {
                for (int j = 0; j < boxDims[1]; j++)
                {
                    for (int k = 0; k < boxDims[2]; k++)
                    {
                        var o = Instantiate(box, new Vector3(i * boxSize.x, k * boxSize.y, j * boxSize.z), Quaternion.identity, spawned.transform);
                        Destroy(o.GetComponent<BoxDropoff>());
                    }
                }
            }

            spawned.transform.position = locationPicker.transform.position;
        }

        public void SpawnDebris()
        {
            pickerArea = locationPicker.GetComponent<Renderer>().bounds;
            debrisSpawn = int.Parse(transform.Find("SpawnDebris").GetComponent<InputField>().text);

            if (spawned != null) 
            {
                Destroy(spawned);
            }
                
            spawned = new GameObject("Spawned");

            var size = WarehouseManager.instance.m_debrisSize;
            var isKinematic = WarehouseManager.instance.m_debrisKinematic;

            for (int i = 0; i < debrisSpawn; i++)
            {
                var randShape = Random.Range(0, 4);
                GameObject obj;
                switch(randShape){
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
                var pos = new Vector3(Random.Range(pickerArea.center.x - pickerArea.extents.x/2, pickerArea.center.x + pickerArea.extents.x/2), size/2, Random.Range(pickerArea.center.z - pickerArea.extents.z/2, pickerArea.center.z + pickerArea.extents.z/2));
                
                obj.transform.localScale = new Vector3(Random.Range(0.005f, size), Random.Range(0.005f, size), Random.Range(0.005f, size));
                obj.transform.parent = spawned.transform;
                obj.transform.localPosition = pos;
                obj.transform.rotation = Random.rotation;
                obj.GetComponent<Renderer>().material = Resources.Load<Material>($"Materials/Debris");

                var lab = obj.AddComponent<Labeling>();
                lab.labels.Add("debris");

                var rb = obj.AddComponent<Rigidbody>();
                rb.isKinematic = isKinematic;
            }

            spawned.transform.position = locationPicker.transform.position;
        }

        public void ShowPicker(bool show)
        {
            locationPicker.SetActive(show);
        }
    };
}