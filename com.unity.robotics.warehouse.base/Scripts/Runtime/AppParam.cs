using System;
using UnityEngine;

namespace Unity.Simulation.Warehouse
{
    [Serializable]
    public class AppParam
    {
        [Min(1)]
        public int width = 50;
        [Min(1)]
        public int length = 50;
        [Min(0)]
        public int shelfRows = 4;
        [Min(0)]
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
