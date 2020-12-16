using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Perception.GroundTruth;

public class RigidbodySpawner : MonoBehaviour
{
    public GameObject LocationPicker;
    public GameObject m_box;
    private Vector3 m_boxDims;
    private int m_debrisSpawn;
    private GameObject m_spawnedBoxes;

    // Start is called before the first frame update
    void Start()
    {
        HidePicker();
    }

    public void SpawnBoxes(){
        var boxIn = transform.Find("SpawnBoxes").GetComponent<InputField>().text.Split(',');
        if (boxIn.Length != 3){
            Debug.LogError($"Invalid box input dimensions!");
            return;
        }

        if (m_spawnedBoxes != null) {
            Destroy(m_spawnedBoxes);
        }
            
        m_spawnedBoxes = new GameObject("Spawned");
        m_boxDims = new Vector3(int.Parse(boxIn[0]), int.Parse(boxIn[1]), int.Parse(boxIn[2]));
        
        var boxSize = m_box.GetComponentInChildren<Renderer>().bounds.size;
        for (int i = 0; i < m_boxDims[0]; i++){
            for (int j = 0; j < m_boxDims[1]; j++){
                for (int k = 0; k < m_boxDims[2]; k++){
                    var o = Instantiate(m_box, new Vector3(i * boxSize.x, k * boxSize.y, j * boxSize.z), Quaternion.identity, m_spawnedBoxes.transform);
                    Destroy(o.GetComponent<BoxDropoff>());
                }
            }
        }

        m_spawnedBoxes.transform.position = LocationPicker.transform.position;
    }

    public void SpawnDebris(){
        m_debrisSpawn = int.Parse(transform.Find("SpawnDebris").GetComponent<InputField>().text);

        if (m_spawnedBoxes != null) {
            Destroy(m_spawnedBoxes);
        }
            
        m_spawnedBoxes = new GameObject("Spawned");

        var size = GameObject.FindObjectOfType<WarehouseManager>().GetEditorParams().m_debrisSize;

        for (int i = 0; i < m_debrisSpawn; i++){
            var randShape = UnityEngine.Random.Range(0, 4);
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
            var pos = new Vector3(UnityEngine.Random.Range(-size * 5,size * 5), UnityEngine.Random.Range(0.01f, 1f), UnityEngine.Random.Range(-size * 5,size * 5));
            
            obj.transform.localScale = new Vector3(UnityEngine.Random.Range(0.005f, size), UnityEngine.Random.Range(0.005f, size), UnityEngine.Random.Range(0.005f, size));
            obj.transform.parent = m_spawnedBoxes.transform;
            obj.transform.localPosition = pos;
            obj.transform.rotation = UnityEngine.Random.rotation;
            obj.GetComponent<Renderer>().material = Resources.Load<Material>($"Materials/Debris");

            var lab = obj.AddComponent<Labeling>();
            lab.labels.Add("debris");

            var rb = obj.AddComponent<Rigidbody>();
        }

        m_spawnedBoxes.transform.position = LocationPicker.transform.position;
    }

    public void ShowPicker(){
        LocationPicker.SetActive(true);
    }

    public void HidePicker(){
        LocationPicker.SetActive(false);
    }
};