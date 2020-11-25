using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class LightingRandomizer : MonoBehaviour
{
    Random m_Rand;
    Light m_Light;
    WarehouseManager _manager;
    Vector3 _maxDims;
    
    // Start is called before the first frame update
    void Start()
    {
        _manager = GameObject.FindObjectOfType<WarehouseManager>();
        m_Rand = new Random(1);
        var light = GameObject.Find("Directional Light");
        if (light == null)
            return;
        m_Light = light.GetComponent<Light>();
        // To simulate phong shading we turn off shadows
        m_Light.shadows = LightShadows.None;

        _maxDims.x = _manager.m_width / 2;
        _maxDims.z = _manager.m_length / 2;
        _maxDims.y = _manager.m_warehousePrefab.transform.Find("WallPanel01").GetComponent<Renderer>().bounds.size.y;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_Light != null){
            // Randomize color
            m_Light.color = new Color(m_Rand.NextFloat(0.1f, 1f), m_Rand.NextFloat(0.1f, 1f), m_Rand.NextFloat(0.1f, 1f));

            // Rotation
            var xRotation = m_Rand.NextFloat(-90, 90);
            var yRotation = m_Rand.NextFloat(-90, 90);
            m_Light.transform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);

            // Position
            var xPos = m_Rand.NextFloat(-_maxDims.x, _maxDims.x);
            var yPos = m_Rand.NextFloat(-_maxDims.y, _maxDims.y);
            var zPos = m_Rand.NextFloat(-_maxDims.z, _maxDims.z);
            m_Light.transform.position = new Vector3(xPos, yPos, zPos);
        }
    }
}
