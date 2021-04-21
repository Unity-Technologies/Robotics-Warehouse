using System;
using UnityEngine;

namespace Unity.Simulation.Warehouse {
    [System.Serializable]
    public class AppParam
    {
        public int m_width = 50;
        public int m_length = 50;
        public int m_shelfRows = 4;
        public int m_shelfCols = 3;

        public override string ToString()
        {
            return "AppParam: " +
                "\nm_width: " + m_width +
                "\nm_length: " + m_length + 
                "\nm_shelfRows: " + m_shelfRows +
                "\nm_shelfCols: " + m_shelfCols;
        }
    }
}