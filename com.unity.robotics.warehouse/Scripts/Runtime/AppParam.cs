using System;
using UnityEngine;

namespace Unity.Simulation.Warehouse
{
    [System.Serializable]
    public class AppParam
    {
        public int width = 50;
        public int length = 50;
        public int shelfRows = 4;
        public int shelfCols = 3;

        public override string ToString()
        {
            return "AppParam: " +
                "\nwidth: " + width +
                "\nlength: " + length +
                "\nshelfRows: " + shelfRows +
                "\nshelfCols: " + shelfCols;
        }
    }
}
