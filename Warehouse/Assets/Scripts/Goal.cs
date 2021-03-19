using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.Control
{
    public class Goal : MonoBehaviour
    {
        public float x;
        public float z;

        public robotState GetGoal()
        {
            return new robotState(x, z, 0);
        }
    }
}
