using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionPrint : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.root.name.Contains("turtlebot3_waffle")){
            Debug.Log($"{collision.gameObject.name} (on {collision.transform.root}) collided with {gameObject.name}");
        }
    }
}
