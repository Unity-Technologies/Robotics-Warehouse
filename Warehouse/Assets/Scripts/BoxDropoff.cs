using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Simulation.Warehouse {
    public class BoxDropoff : MonoBehaviour
    {
        public float m_lifetime = 10f;

        // Update is called once per frame
        void Update()
        {
            m_lifetime -= Time.deltaTime;
            if (m_lifetime <= 0){
                Destroy(this.gameObject);
            }
        }
    }
   
}