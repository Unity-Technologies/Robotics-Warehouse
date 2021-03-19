using System;
using UnityEngine;

namespace Unity.Simulation.Warehouse {
    [System.Serializable]
    public class AppParam
    {
        public int m_width = 50;
        public int m_length = 50;
        public int m_rows = 4;
        public int m_cols = 3;
        public int m_numBots = 3;
        public int m_dropoff = 10;
        [Range(0.0f, 1.0f)] public float m_generateFloorBoxes = 0f;
        [Range(0.0f, 1.0f)] public float m_generateFloorDebris = 0f; 
        [Range(0.0f, 1.0f)] public float m_debrisSize = 0f; 
        public bool m_debrisKinematic = false;
        // [Range(0.0f, 1.0f)] public float m_percentLight = 1.0f;
        public int m_quitAfterSeconds = 60;

        public override string ToString()
        {
            return "AppParam: " +
                "\nm_width: " + m_width +
                "\nm_length: " + m_length + 
                "\nm_rows: " + m_rows +
                "\nm_cols: " + m_cols +
                "\nm_numBots: " + m_numBots +
                "\nm_dropoff: " + m_dropoff +
                "\nm_generateFloorBoxes: " + m_generateFloorBoxes +
                "\nm_generateFloorDebris: " + m_generateFloorDebris +
                "\nm_debrisSize: " + m_debrisSize +
                // "\nm_percentLight: " + m_percentLight +
                "\nm_quitAfterSeconds: " + m_quitAfterSeconds;
        }
    }
}